﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Meadow.Cloud;

namespace Meadow.Logging
{

    public class CloudLogger : ILogProvider
    {
        /// <summary>
        /// Instantiate a new CloudLogger
        /// </summary>
        /// <param name="level">Minimum level to log (cannot be lower than Information)</param>
        /// <exception cref="ArgumentException"></exception>
        public CloudLogger(LogLevel level = LogLevel.Information)
        {
            if (level <= LogLevel.Debug)
            {
                // there is debug logging in the method that sends the logs to the cloud. If we allowed
                // cloud logging at the level (or below) we'd end up in an infinite loop.
                throw new ArgumentException("Minimum level for CloudLogger is Information");
            }

            MinLevel = level;

            LogFilePath = Path.Combine(Resolver.Device.PlatformOS.FileSystem.DocumentsDirectory, "cloud.log");
            if (!File.Exists(LogFilePath))
            {
                File.Create(LogFilePath).Close();
            }

            EventFilePath = Path.Combine(Resolver.Device.PlatformOS.FileSystem.DocumentsDirectory, "events.log");
            if (!File.Exists(EventFilePath))
            {
                File.Create(EventFilePath).Close();
            }
        }

        /// <summary>
        /// Path to the log file
        /// </summary>
        public string LogFilePath { get; protected set; }
        /// <summary>
        /// Path to the event file
        /// </summary>
        public string EventFilePath { get; protected set; }
        /// <summary>
        /// Current minimum level for the CloudLogger
        /// </summary>
        public LogLevel MinLevel { get; protected set; }
        private char delim = '|';
        static SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);

        /// <summary>
        /// Send a log message to Meadow.Cloud.
        /// </summary>
        /// <param name="level">LogLevel</param>
        /// <param name="message">Message of the log</param>
        public void Log(LogLevel level, string message)
        {
            if (level >= MinLevel)
            {
                var cloudLog = new CloudLog()
                {
                    Severity = level.ToString(),
                    Message = message,
                    Timestamp = DateTime.UtcNow
                };

                Send(LogFilePath, cloudLog, Resolver.MeadowCloudService.SendLog);
            }
        }

        /// <summary>
        /// Log an exception.
        /// </summary>
        /// <param name="ex"></param>
        public async void LogException(Exception ex)
        {
            var log = new CloudLog()
            {
                Severity = LogLevel.Error.ToString(),
                Exception = ex.StackTrace,
                Message = ex.Message,
                Timestamp = DateTime.UtcNow
            };

            Send(LogFilePath, log, Resolver.MeadowCloudService.SendLog);
        }

        /// <summary>
        /// Log an event.
        /// </summary>
        /// <param name="eventId">id used for a set of events.</param>
        /// <param name="description">Description of the event.</param>
        /// <param name="measurements">Dynamic payload of measurements to be recorded.</param>
        public async void LogEvent(int eventId, string description, Dictionary<string, object> measurements)
        {
            var cloudEvent = new CloudEvent()
            {
                EventId = eventId,
                Description = description,
                Measurements = measurements,
                Timestamp = DateTime.UtcNow
            };

            Send(EventFilePath, cloudEvent, Resolver.MeadowCloudService.SendEvent);
        }

        private async void Send<T>(string file, T item, Func<T, Task> sendFunc)
        {
            var serializeOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

            var connected = Resolver.Device.NetworkAdapters.Any(a => a.IsConnected);

            if (connected)
            {
                await semaphoreSlim.WaitAsync();

                try
                {
                    // send messages that were stored offline
                    var lines = File.ReadAllLines(file);
                    if (lines.Length > 0)
                    {
                        Resolver.Log.Debug($"processing {lines.Length} stored {typeof(T)}");
                        foreach (var line in lines)
                        {
                            if (string.IsNullOrWhiteSpace(line))
                            {
                                continue;
                            }

                            var o = JsonSerializer.Deserialize<T>(line, serializeOptions);
                            if (o != null)
                            {
                                await sendFunc(o);
                            }
                        }

                        File.Create(file).Close();
                        Resolver.Log.Debug($"cleared stored {typeof(T)}");
                    }

                    // send current message
                    Resolver.Log.Debug($"sending {typeof(T)}");
                    await sendFunc(item);
                }
                catch (Exception ex)
                {
                    Resolver.Log.Debug($"error sending {typeof(T)}: {ex.Message}");
                }
                finally
                {
                    semaphoreSlim.Release();
                }
            }
            else
            {
                var json = JsonSerializer.Serialize(item, serializeOptions);
                File.AppendAllLines(file, new[] { json });
                Resolver.Log.Debug($"saved cloud log to local store {json}");
            }
        }
    }
}
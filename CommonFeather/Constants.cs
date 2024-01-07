using System;
using UnitsNet;

namespace CommonFeather
{
    public class Constants
    {
        public class Bluetooth
        {
            public const string SSID = "8c3bb16c0d954fb8b37999e1f040b279";
            public const string PASSWORD = "b1cb00bd69424cba937a597fabb93052";
            public const string TOGGLE_WIFI_CONNECTION = "98dea8431f7443a3bf24f78650672361";
            public const string ROTATION = "e78f7b5e-842b-4b99-94e3-7401bf72b870";
            public const string DISTANCE = "c1d4bde2-9c09-4592-b7f4-3b4861e6e388";
            public const string BUTTON = "246ca532-fbdf-4c16-96a2-d1ac026322d2";
            public const string LAUNCH = "ecdd38ff-d30e-4c08-bf10-6bce435b31e0";
            public const string TOGGLE_BLE_CONNECTION = "8ee2de4ced7c4f2c92a1f984507d87e3";
            public const string WRITEABLE = "00002902-0000-1000-8000-00805f9b34fb";

            public static readonly byte[] ENABLE_NOTIFICATION_VALUE = { 0x01, 0x00 };


            public const int MACHINE_SERVICE_UID = 253;
            public const string MACHINE_SERVICE_NAME = "machine-service";
            public const string DEFINITION_SERVICE_NAME = "MeadowBasketBallShooter";
            
        }

        public class Sensors
        {
            public static readonly Length MINIMUM_SENSOR_DISTANCE = Length.FromCentimeters(10);
            public static readonly int MINIMUM_SENSOR_DISTANCE_COUNTS = 5;
            public static readonly int MINIMUM_COUNTDOWN_SECONDS = 1;
            public static readonly TimeSpan SENSOR_DISTANCE_READ_FREQUENCY = TimeSpan.FromMilliseconds(500);
        }
    }
}

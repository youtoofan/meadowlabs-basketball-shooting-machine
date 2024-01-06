using MobileFeather.ViewModel;

namespace MobileFeather
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            BindingContext = new MainViewModel();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (!(BindingContext as BaseViewModel).IsBlePaired)
            {
                (BindingContext as BaseViewModel).CmdSearchForDevices.Execute(null);
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
        }
    }

}

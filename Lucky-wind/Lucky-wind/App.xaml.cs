using Lucky_wind.Services;
using Lucky_wind.Views;
using Xamarin.Forms;

namespace Lucky_wind
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            // Si ya existe sesión activa, ir directo al Dashboard;
            // de lo contrario, comenzar en la pantalla Splash.
            if (AuthService.IsLoggedIn)
            {
                MainPage = new NavigationPage(new DashboardPage())
                {
                    BarBackgroundColor = Color.FromHex("#3211d4"),
                    BarTextColor       = Color.White
                };
            }
            else
            {
                MainPage = new NavigationPage(new SplashPage())
                {
                    BarBackgroundColor = Color.FromHex("#f6f6f8"),
                    BarTextColor       = Color.FromHex("#3211d4")
                };
            }
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}

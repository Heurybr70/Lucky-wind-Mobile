using System.Windows.Input;
using Lucky_wind.Views;
using Xamarin.Forms;

namespace Lucky_wind.ViewModels
{
    /// <summary>
    /// ViewModel de la pantalla de presentación (Splash).
    /// Gestiona la navegación inicial hacia el Login.
    /// </summary>
    public class SplashViewModel : BaseViewModel
    {
        private readonly INavigation _navigation;

        public SplashViewModel(INavigation navigation)
        {
            _navigation = navigation;
            Title = "Lucky-Win";

            // Comando que navega hacia la pantalla de Login
            StartCommand = new Command(async () =>
            {
                if (IsBusy) return;
                IsBusy = true;
                await _navigation.PushAsync(new LoginPage());
                IsBusy = false;
            });
        }

        /// <summary>Comando ejecutado al pulsar "Comenzar".</summary>
        public ICommand StartCommand { get; }
    }
}

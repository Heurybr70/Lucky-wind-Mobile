using System.Windows.Input;
using Lucky_wind.Services;
using Lucky_wind.Views;
using Xamarin.Forms;

namespace Lucky_wind.ViewModels
{
    /// <summary>
    /// ViewModel del menú principal / Dashboard.
    /// Expone las opciones del menú y el comando de cierre de sesión.
    /// </summary>
    public class DashboardViewModel : BaseViewModel
    {
        private readonly AuthService _authService;
        private readonly INavigation _navigation;

        // ─── Propiedades ─────────────────────────────────────────────────────────
        private string _userEmail;
        /// <summary>Correo del usuario actualmente autenticado.</summary>
        public string UserEmail
        {
            get => _userEmail;
            set => SetProperty(ref _userEmail, value);
        }

        // ─── Comandos del menú ───────────────────────────────────────────────────
        /// <summary>Navega a la sección de registro de sorteos.</summary>
        public ICommand RegisterSorteoCommand { get; }

        /// <summary>Navega a la sección de historial.</summary>
        public ICommand VerHistorialCommand { get; }

        /// <summary>Navega a la sección de análisis estadístico.</summary>
        public ICommand AnalisisCommand { get; }

        /// <summary>Alterna entre modo claro y oscuro.</summary>
        public ICommand ToggleThemeCommand { get; }

        /// <summary>Icono del tema actual (☀️ / 🌙).</summary>
        public string ThemeIcon => ThemeService.IsDark ? "☀️" : "🌙";

        /// <summary>Cierra la sesión y regresa al Login.</summary>
        public ICommand LogoutCommand { get; }

        // ─── Constructor ─────────────────────────────────────────────────────────
        public DashboardViewModel(INavigation navigation)
        {
            _navigation  = navigation;
            _authService = new AuthService();
            Title        = "Menú Principal";

            // Cargar el email del usuario en sesión
            UserEmail = AuthService.CurrentUser?.Email ?? "Usuario";

            RegisterSorteoCommand = new Command(async () =>
                await _navigation.PushAsync(new RegisterSorteoPage()));

            VerHistorialCommand = new Command(async () =>
                await _navigation.PushAsync(new HistorialPage()));

            AnalisisCommand = new Command(async () =>
                await _navigation.PushAsync(new EstadisticasPage()));

            ToggleThemeCommand = new Command(() =>
            {
                ThemeService.Toggle();
                OnPropertyChanged(nameof(ThemeIcon));
            });

            LogoutCommand = new Command(async () =>
            {
                bool confirm = await Application.Current.MainPage
                    .DisplayAlert("Cerrar sesión",
                                  "¿Estás seguro de que deseas cerrar sesión?",
                                  "Sí, cerrar", "Cancelar");

                if (!confirm) return;

                // Limpiar sesión y reiniciar navegación al Login
                _authService.Logout();

                Application.Current.MainPage =
                    new NavigationPage(new LoginPage())
                    {
                        BarBackgroundColor = Color.FromHex("#3211d4"),
                        BarTextColor       = Color.White
                    };
            });
        }
    }
}

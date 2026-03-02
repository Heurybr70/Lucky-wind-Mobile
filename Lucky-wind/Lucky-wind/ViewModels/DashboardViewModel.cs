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

        /// <summary>Cierra la sesión y regresa al Login.</summary>
        public ICommand LogoutCommand { get; }

        // ─── Constructor ─────────────────────────────────────────────────────────
        public DashboardViewModel()
        {
            _authService = new AuthService();
            Title        = "Menú Principal";

            // Cargar el email del usuario en sesión
            UserEmail = AuthService.CurrentUser?.Email ?? "Usuario";

            RegisterSorteoCommand = new Command(async () =>
            {
                await Application.Current.MainPage
                    .DisplayAlert("Registrar Sorteo",
                                  "Esta funcionalidad estará disponible próximamente.",
                                  "Aceptar");
            });

            VerHistorialCommand = new Command(async () =>
            {
                await Application.Current.MainPage
                    .DisplayAlert("Ver Historial",
                                  "Esta funcionalidad estará disponible próximamente.",
                                  "Aceptar");
            });

            AnalisisCommand = new Command(async () =>
            {
                await Application.Current.MainPage
                    .DisplayAlert("Análisis Estadístico",
                                  "Esta funcionalidad estará disponible próximamente.",
                                  "Aceptar");
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

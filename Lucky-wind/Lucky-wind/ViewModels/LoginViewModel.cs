using System.Windows.Input;
using Lucky_wind.Services;
using Lucky_wind.Views;
using Xamarin.Forms;

namespace Lucky_wind.ViewModels
{
    /// <summary>
    /// ViewModel de la pantalla de inicio de sesión.
    /// Gestiona la autenticación contra Firebase y la navegación.
    /// </summary>
    public class LoginViewModel : BaseViewModel
    {
        private readonly INavigation _navigation;
        private readonly AuthService _authService;

        // ─── Propiedades ─────────────────────────────────────────────────────────
        private string _email;
        /// <summary>Correo electrónico ingresado por el usuario.</summary>
        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        private string _password;
        /// <summary>Contraseña ingresada por el usuario.</summary>
        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        private bool _isPasswordVisible;
        /// <summary>Determina si la contraseña es visible en texto plano.</summary>
        public bool IsPasswordVisible
        {
            get => _isPasswordVisible;
            set
            {
                SetProperty(ref _isPasswordVisible, value);
                // Notifica también la propiedad inversa (para el Entry IsPassword)
                OnPropertyChanged(nameof(IsPasswordHidden));
            }
        }

        /// <summary>Inverso de IsPasswordVisible, para el binding IsPassword del Entry.</summary>
        public bool IsPasswordHidden => !_isPasswordVisible;

        // ─── Comandos ────────────────────────────────────────────────────────────
        /// <summary>Alterna la visibilidad de la contraseña.</summary>
        public ICommand TogglePasswordCommand { get; }

        /// <summary>Inicia el proceso de autenticación.</summary>
        public ICommand LoginCommand { get; }
        /// <summary>Inicia sesión con Google.</summary>
        public ICommand GoogleLoginCommand { get; }
        /// <summary>Navega hacia la pantalla de registro.</summary>
        public ICommand GoToRegisterCommand { get; }

        // ─── Constructor ─────────────────────────────────────────────────────────
        public LoginViewModel(INavigation navigation)
        {
            _navigation  = navigation;
            _authService = new AuthService();
            Title        = "Iniciar Sesión";
            _isPasswordVisible = false;

            TogglePasswordCommand = new Command(() =>
                IsPasswordVisible = !IsPasswordVisible);

            LoginCommand = new Command(async () =>
            {
                if (IsBusy) return;

                // ── Validaciones básicas ──────────────────────────────────────
                if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
                {
                    await Application.Current.MainPage
                        .DisplayAlert("Campos requeridos",
                                      "Por favor completa el correo y la contraseña.",
                                      "Entendido");
                    return;
                }

                if (!IsValidEmail(Email))
                {
                    await Application.Current.MainPage
                        .DisplayAlert("Correo inválido",
                                      "Ingresa un correo electrónico con formato válido.",
                                      "Entendido");
                    return;
                }

                IsBusy = true;

                try
                {
                    var (success, error) = await _authService.LoginAsync(Email, Password);

                    if (success)
                    {
                        // Guardar sesión para persistencia entre reinicios
                        await AuthService.SaveSessionAsync();
                        // Login exitoso: navegar al Dashboard y limpiar historial
                        Application.Current.MainPage =
                            new NavigationPage(new DashboardPage())
                            {
                                BarBackgroundColor = Color.FromHex("#3211d4"),
                                BarTextColor       = Color.White
                            };
                    }
                    else
                    {
                        await Application.Current.MainPage
                            .DisplayAlert("Error al iniciar sesión", error, "Entendido");
                    }
                }
                finally
                {
                    IsBusy = false;
                }
            });

            GoToRegisterCommand = new Command(async () =>
            {
                if (IsBusy) return;
                await _navigation.PushAsync(new RegisterPage());
            });

            GoogleLoginCommand = new Command(async () =>
            {
                if (IsBusy) return;

                // Resolver la implementación de plataforma via DependencyService
                var googleAuth = Xamarin.Forms.DependencyService.Get<Lucky_wind.Services.IGoogleAuthService>();
                if (googleAuth == null)
                {
                    await Application.Current.MainPage
                        .DisplayAlert("No disponible",
                                      "Google Sign-In no está disponible en esta plataforma.",
                                      "Ok");
                    return;
                }

                IsBusy = true;

                try
                {
                    var (idToken, googleError) = await googleAuth.GetGoogleIdTokenAsync();

                    // El usuario canceló la selección de cuenta
                    if (idToken == null && googleError == null)
                        return;

                    // Error en el flujo de Google
                    if (googleError != null)
                    {
                        await Application.Current.MainPage
                            .DisplayAlert("Error con Google", googleError, "Entendido");
                        return;
                    }

                    var (success, error) = await _authService.SignInWithGoogleAsync(idToken);

                    if (success)
                    {
                        await AuthService.SaveSessionAsync();
                        Application.Current.MainPage =
                            new NavigationPage(new DashboardPage())
                            {
                                BarBackgroundColor = Color.FromHex("#3211d4"),
                                BarTextColor       = Color.White
                            };
                    }
                    else
                    {
                        await Application.Current.MainPage
                            .DisplayAlert("Error con Google", error, "Entendido");
                    }
                }
                finally
                {
                    IsBusy = false;
                }
            });
        }

        // ─── Helpers ─────────────────────────────────────────────────────────────
        private static bool IsValidEmail(string email)
        {
            if (string.IsNullOrEmpty(email)) return false;
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}

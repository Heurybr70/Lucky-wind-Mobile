using System.Windows.Input;
using Lucky_wind.Services;
using Lucky_wind.Views;
using Xamarin.Forms;

namespace Lucky_wind.ViewModels
{
    /// <summary>
    /// ViewModel de la pantalla de registro de nuevos usuarios.
    /// </summary>
    public class RegisterViewModel : BaseViewModel
    {
        private readonly INavigation _navigation;
        private readonly AuthService _authService;

        // ─── Propiedades ─────────────────────────────────────────────────────────
        private string _email;
        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        private string _password;
        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        private string _confirmPassword;
        public string ConfirmPassword
        {
            get => _confirmPassword;
            set => SetProperty(ref _confirmPassword, value);
        }

        private bool _isPasswordVisible;
        public bool IsPasswordVisible
        {
            get => _isPasswordVisible;
            set
            {
                SetProperty(ref _isPasswordVisible, value);
                OnPropertyChanged(nameof(IsPasswordHidden));
            }
        }
        public bool IsPasswordHidden => !_isPasswordVisible;

        private bool _isConfirmPasswordVisible;
        public bool IsConfirmPasswordVisible
        {
            get => _isConfirmPasswordVisible;
            set
            {
                SetProperty(ref _isConfirmPasswordVisible, value);
                OnPropertyChanged(nameof(IsConfirmPasswordHidden));
            }
        }
        public bool IsConfirmPasswordHidden => !_isConfirmPasswordVisible;

        private bool _acceptedTerms;
        public bool AcceptedTerms
        {
            get => _acceptedTerms;
            set => SetProperty(ref _acceptedTerms, value);
        }

        // ─── Comandos ────────────────────────────────────────────────────────────
        public ICommand TogglePasswordCommand        { get; }
        public ICommand ToggleConfirmPasswordCommand { get; }
        public ICommand RegisterCommand              { get; }
        public ICommand GoToLoginCommand             { get; }

        // ─── Constructor ─────────────────────────────────────────────────────────
        public RegisterViewModel(INavigation navigation)
        {
            _navigation  = navigation;
            _authService = new AuthService();
            Title        = "Crear Cuenta";

            TogglePasswordCommand = new Command(() =>
                IsPasswordVisible = !IsPasswordVisible);

            ToggleConfirmPasswordCommand = new Command(() =>
                IsConfirmPasswordVisible = !IsConfirmPasswordVisible);

            GoToLoginCommand = new Command(async () =>
            {
                if (IsBusy) return;
                await _navigation.PopAsync();
            });

            RegisterCommand = new Command(async () =>
            {
                if (IsBusy) return;

                // ── Validaciones ─────────────────────────────────────────────
                if (string.IsNullOrWhiteSpace(Email)    ||
                    string.IsNullOrWhiteSpace(Password) ||
                    string.IsNullOrWhiteSpace(ConfirmPassword))
                {
                    await Application.Current.MainPage
                        .DisplayAlert("Campos requeridos",
                                      "Por favor completa todos los campos.",
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

                if (Password.Length < 6)
                {
                    await Application.Current.MainPage
                        .DisplayAlert("Contraseña débil",
                                      "La contraseña debe tener al menos 6 caracteres.",
                                      "Entendido");
                    return;
                }

                if (Password != ConfirmPassword)
                {
                    await Application.Current.MainPage
                        .DisplayAlert("Las contraseñas no coinciden",
                                      "Verifica que ambas contraseñas sean iguales.",
                                      "Entendido");
                    return;
                }

                if (!AcceptedTerms)
                {
                    await Application.Current.MainPage
                        .DisplayAlert("Términos y condiciones",
                                      "Debes aceptar los términos y condiciones para continuar.",
                                      "Entendido");
                    return;
                }

                IsBusy = true;

                try
                {
                    var (success, error) = await _authService.RegisterAsync(Email, Password);

                    if (success)
                    {
                        await Application.Current.MainPage
                            .DisplayAlert("¡Registro exitoso!",
                                          "Tu cuenta ha sido creada correctamente. Ahora puedes iniciar sesión.",
                                          "Continuar");
                        // Redirigir automáticamente al Login
                        await _navigation.PopAsync();
                    }
                    else
                    {
                        await Application.Current.MainPage
                            .DisplayAlert("Error al registrarse", error, "Entendido");
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

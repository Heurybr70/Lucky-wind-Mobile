using Lucky_wind.ViewModels;
using Xamarin.Forms;

namespace Lucky_wind.Views
{
    /// <summary>
    /// Pantalla de inicio de sesión.
    /// Solo asigna el BindingContext; toda la lógica vive en LoginViewModel.
    /// </summary>
    public partial class LoginPage : ContentPage
    {
        public LoginPage()
        {
            InitializeComponent();
            BindingContext = new LoginViewModel(Navigation);
        }
    }
}

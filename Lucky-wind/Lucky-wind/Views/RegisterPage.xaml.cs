using Lucky_wind.ViewModels;
using Xamarin.Forms;

namespace Lucky_wind.Views
{
    /// <summary>
    /// Pantalla de registro de nuevos usuarios.
    /// Solo asigna el BindingContext; toda la lógica vive en RegisterViewModel.
    /// </summary>
    public partial class RegisterPage : ContentPage
    {
        public RegisterPage()
        {
            InitializeComponent();
            BindingContext = new RegisterViewModel(Navigation);
        }
    }
}

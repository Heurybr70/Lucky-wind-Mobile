using System.Threading.Tasks;
using Lucky_wind.ViewModels;
using Xamarin.Forms;

namespace Lucky_wind.Views
{
    /// <summary>
    /// Pantalla de registro de nuevos usuarios.
    /// Solo asigna el BindingContext; la animación de entrada es lógica visual pura.
    /// </summary>
    public partial class RegisterPage : ContentPage
    {
        public RegisterPage()
        {
            InitializeComponent();
            BindingContext = new RegisterViewModel(Navigation);
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            FormContainer.Opacity      = 0.0;
            FormContainer.TranslationY = 24;

            await Task.WhenAll(
                FormContainer.FadeTo(1.0, 550, Easing.CubicOut),
                FormContainer.TranslateTo(0, 0, 550, Easing.CubicOut)
            );
        }
    }
}

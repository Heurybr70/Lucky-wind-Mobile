using System.Threading.Tasks;
using Lucky_wind.ViewModels;
using Xamarin.Forms;

namespace Lucky_wind.Views
{
    public partial class RegisterSorteoPage : ContentPage
    {
        public RegisterSorteoPage()
        {
            InitializeComponent();
            BindingContext = new RegisterSorteoViewModel(Navigation);
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // Entrada suave desde abajo
            FormContainer.Opacity = 0;
            FormContainer.TranslationY = 30;

            await Task.WhenAll(
                FormContainer.FadeTo(1.0, 600, Easing.CubicOut),
                FormContainer.TranslateTo(0, 0, 600, Easing.CubicOut)
            );
        }
    }
}

using System.Threading.Tasks;
using Lucky_wind.Models;
using Lucky_wind.ViewModels;
using Xamarin.Forms;

namespace Lucky_wind.Views
{
    public partial class DetalleSorteoPage : ContentPage
    {
        public DetalleSorteoPage(RaffleModel raffle)
        {
            InitializeComponent();
            BindingContext = new DetalleSorteoViewModel(raffle, Navigation);
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // Animación de entrada
            HeaderBar.Opacity    = 0;
            HeaderBar.TranslationY = -20;
            InfoCard.Opacity     = 0;
            InfoCard.TranslationY = 20;

            await Task.WhenAll(
                HeaderBar.FadeTo(1.0, 400, Easing.CubicOut),
                HeaderBar.TranslateTo(0, 0, 400, Easing.CubicOut)
            );

            await Task.WhenAll(
                InfoCard.FadeTo(1.0, 350, Easing.CubicOut),
                InfoCard.TranslateTo(0, 0, 350, Easing.CubicOut)
            );
        }

        protected override bool OnBackButtonPressed()
        {
            Navigation.PopAsync();
            return true;
        }

        private async void OnBackTapped(object sender, System.EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}

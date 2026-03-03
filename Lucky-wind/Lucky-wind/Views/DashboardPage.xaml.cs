using System.Threading.Tasks;
using Lucky_wind.ViewModels;
using Xamarin.Forms;

namespace Lucky_wind.Views
{
    /// <summary>
    /// Menú principal de la aplicación.
    /// Solo asigna el BindingContext; la animación de tarjetas es lógica visual pura.
    /// </summary>
    public partial class DashboardPage : ContentPage
    {
        public DashboardPage()
        {
            InitializeComponent();
            BindingContext = new DashboardViewModel(Navigation);
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // Ocultar header y tarjetas
            HeaderStack.Opacity      = 0.0;
            HeaderStack.TranslationY = -20;
            Card1.Opacity = Card2.Opacity = Card3.Opacity = Card4.Opacity = 0.0;
            Card1.TranslationY = Card2.TranslationY = Card3.TranslationY = Card4.TranslationY = 30;

            // 1. Header baja desde arriba
            await Task.WhenAll(
                HeaderStack.FadeTo(1.0, 400, Easing.CubicOut),
                HeaderStack.TranslateTo(0, 0, 400, Easing.CubicOut)
            );

            // 2. Tarjetas en cascada (150 ms de desfase)
            var cardDelay = 120;
            await Task.WhenAll(
                AnimateCard(Card1, 0 * cardDelay),
                AnimateCard(Card2, 1 * cardDelay),
                AnimateCard(Card3, 2 * cardDelay),
                AnimateCard(Card4, 3 * cardDelay)
            );
        }

        private static async Task AnimateCard(Xamarin.Forms.View card, int delayMs)
        {
            if (delayMs > 0) await Task.Delay(delayMs);
            await Task.WhenAll(
                card.FadeTo(1.0, 350, Easing.CubicOut),
                card.TranslateTo(0, 0, 350, Easing.CubicOut)
            );
        }
    }
}

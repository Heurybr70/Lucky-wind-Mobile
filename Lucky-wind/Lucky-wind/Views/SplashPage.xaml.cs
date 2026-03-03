using System.Threading.Tasks;
using Lucky_wind.ViewModels;
using Xamarin.Forms;

namespace Lucky_wind.Views
{
    /// <summary>
    /// Pantalla de presentación. Solo contiene la animación de entrada (UI pura)
    /// y la asignación del BindingContext. Toda la lógica vive en SplashViewModel.
    /// </summary>
    public partial class SplashPage : ContentPage
    {
        public SplashPage()
        {
            InitializeComponent();
            BindingContext = new SplashViewModel(Navigation);
        }

        /// <summary>
        /// Ejecuta la animación de entrada cada vez que la página aparece.
        /// Esto es lógica puramente visual: no modifica el estado del ViewModel.
        /// </summary>
        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // ── Estado inicial: todo invisible / desplazado ──────────────────
            LogoFrame.Scale       = 0.6;
            LogoFrame.Opacity     = 0.0;
            TitleLabel.Opacity    = 0.0;
            TitleLabel.TranslationY = 20;
            DividerBox.Opacity    = 0.0;
            TaglineLabel.Opacity  = 0.0;
            TaglineLabel.TranslationY = 10;
            VersionLabel.Opacity  = 0.0;
            StartFrame.Opacity    = 0.0;
            StartFrame.TranslationY = 20;

            // ── 1. Logo aparece con rebote ───────────────────────────────────
            await Task.WhenAll(
                LogoFrame.ScaleTo(1.0, 700, Easing.SpringOut),
                LogoFrame.FadeTo(1.0, 600, Easing.CubicOut)
            );

            // ── 2. Título desliza hacia arriba ───────────────────────────────
            await Task.WhenAll(
                TitleLabel.FadeTo(1.0, 400, Easing.CubicOut),
                TitleLabel.TranslateTo(0, 0, 400, Easing.CubicOut)
            );

            // ── 3. Divisor y tagline juntos ──────────────────────────────────
            await Task.WhenAll(
                DividerBox.FadeTo(1.0, 350, Easing.CubicOut),
                TaglineLabel.FadeTo(1.0, 350, Easing.CubicOut),
                TaglineLabel.TranslateTo(0, 0, 350, Easing.CubicOut)
            );

            await Task.Delay(100);

            // ── 4. Versión y botón de inicio ─────────────────────────────────
            await Task.WhenAll(
                VersionLabel.FadeTo(1.0, 350, Easing.CubicOut),
                StartFrame.FadeTo(1.0, 350, Easing.CubicOut),
                StartFrame.TranslateTo(0, 0, 350, Easing.CubicOut)
            );
        }
    }
}

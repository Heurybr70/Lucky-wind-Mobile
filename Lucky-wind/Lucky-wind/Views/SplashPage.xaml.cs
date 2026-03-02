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

            // Estado inicial antes de animar
            LogoFrame.Scale   = 0.7;
            LogoFrame.Opacity = 0.0;

            // Animar: escalar y desvanecer hacia la opacidad plena (≥ 800 ms)
            await Task.WhenAll(
                LogoFrame.ScaleTo(1.0, 900, Easing.SpringOut),
                LogoFrame.FadeTo(1.0, 800, Easing.CubicOut)
            );
        }
    }
}

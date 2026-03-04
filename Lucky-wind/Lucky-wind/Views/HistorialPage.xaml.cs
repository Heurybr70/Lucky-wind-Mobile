using System.Threading.Tasks;
using Lucky_wind.ViewModels;
using Xamarin.Forms;

namespace Lucky_wind.Views
{
    public partial class HistorialPage : ContentPage
    {
        public HistorialPage()
        {
            InitializeComponent();
            BindingContext = new HistorialViewModel(Navigation);
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // Animar la cabecera desde arriba
            HeaderBar.Opacity = 0;
            HeaderBar.TranslationY = -20;

            // Animar las listas desde abajo
            ActiveList.Opacity = 0;
            ActiveList.TranslationY = 24;
            FinishedList.Opacity = 0;
            FinishedList.TranslationY = 24;

            // Secuencia escalonada
            await Task.WhenAll(
                HeaderBar.FadeTo(1.0, 400, Easing.CubicOut),
                HeaderBar.TranslateTo(0, 0, 400, Easing.CubicOut)
            );

            var vm = (HistorialViewModel)BindingContext;
            vm.LoadCommand.Execute(null);

            await Task.WhenAll(
                ActiveList.FadeTo(1.0, 500, Easing.CubicOut),
                ActiveList.TranslateTo(0, 0, 500, Easing.CubicOut),
                FinishedList.FadeTo(1.0, 500, Easing.CubicOut),
                FinishedList.TranslateTo(0, 0, 500, Easing.CubicOut)
            );
        }

        private async void OnBackTapped(object sender, System.EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}

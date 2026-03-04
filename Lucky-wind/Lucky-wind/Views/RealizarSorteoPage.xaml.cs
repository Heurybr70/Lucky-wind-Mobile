using System;
using System.Threading.Tasks;
using Lucky_wind.Models;
using Lucky_wind.ViewModels;
using Xamarin.Forms;

namespace Lucky_wind.Views
{
    public partial class RealizarSorteoPage : ContentPage
    {
        private RealizarSorteoViewModel _vm;

        public RealizarSorteoPage(RaffleModel raffle)
        {
            InitializeComponent();
            _vm = new RealizarSorteoViewModel(raffle, Navigation);
            BindingContext = _vm;

            // Observar cuando el VM dispara el sorteo
            _vm.PropertyChanged += OnViewModelPropertyChanged;
        }

        // ─── Observador de propiedades del ViewModel ──────────────────────────────
        private async void OnViewModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(RealizarSorteoViewModel.IsSpinning) && _vm.IsSpinning)
            {
                await RunSpinAnimationAsync();
            }
        }

        // ─── Animación de sorteo (3 fases) ───────────────────────────────────────
        private async Task RunSpinAnimationAsync()
        {
            var participants = _vm.Raffle?.Participants;
            if (participants == null || participants.Count == 0) return;

            int  count   = participants.Count;
            var  rng     = new Random();

            // Fase 1: rápida — 20 ciclos × 50ms
            for (int i = 0; i < 20; i++)
            {
                string name = participants[i % count];
                Device.BeginInvokeOnMainThread(() =>
                {
                    SpinLabel.Text = name;
                    _vm.CurrentDisplay = name;
                });
                await Task.Delay(50);
            }

            // Fase 2: desacelerando
            int[] delays = { 80, 100, 140, 180, 240, 320, 420, 540, 680, 850 };
            foreach (int delay in delays)
            {
                string name = participants[rng.Next(count)];
                Device.BeginInvokeOnMainThread(() =>
                {
                    SpinLabel.Text     = name;
                    _vm.CurrentDisplay = name;
                });
                await Task.Delay(delay);
            }

            // Fase 3: revelar ganador con animación
            string winner = _vm.PendingWinner;

            Device.BeginInvokeOnMainThread(() =>
            {
                SpinLabel.Text     = winner;
                _vm.CurrentDisplay = winner;
            });

            // Animación del círculo: escala 1.1 → 1.0
            await HeroCircle.ScaleTo(1.12, 200, Easing.CubicOut);
            await HeroCircle.ScaleTo(1.0,  250, Easing.BounceOut);

            // Confirmar ganador en el ViewModel (guarda en Firestore, cambia estado)
            _vm.ConfirmWinnerCommand.Execute(null);
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            if (_vm != null)
                _vm.PropertyChanged -= OnViewModelPropertyChanged;
        }

        protected override bool OnBackButtonPressed()
        {
            Navigation.PopAsync();
            return true;
        }

        private async void OnBackTapped(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}

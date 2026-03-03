using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Lucky_wind.ViewModels;
using Xamarin.Forms;

namespace Lucky_wind.Views
{
    public partial class EstadisticasPage : ContentPage
    {
        private EstadisticasViewModel _vm;
        private bool _dataReady;
        private bool _barsRendered;

        public EstadisticasPage()
        {
            InitializeComponent();
            _vm = new EstadisticasViewModel();
            BindingContext = _vm;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            _barsRendered = false;

            // Resetear opacidades para animación de entrada
            PageScroll.Opacity = 0;
            PageScroll.TranslationY = 20;
            HeaderBar.Opacity = 0;
            MetricsGrid.Opacity = 0;
            ChartCard.Opacity = 0;
            DistCard.Opacity = 0;

            // Suscribirse a cambios del ViewModel
            _vm.PropertyChanged += OnVmPropertyChanged;

            // Lanzar carga de datos
            _vm.LoadCommand.Execute(null);

            // Animar la página completa
            await Task.WhenAll(
                PageScroll.FadeTo(1.0, 500, Easing.CubicOut),
                PageScroll.TranslateTo(0, 0, 500, Easing.CubicOut)
            );

            // Animar secciones en cascada
            await Task.WhenAll(
                HeaderBar.FadeTo(1.0, 350, Easing.CubicOut)
            );
            await Task.WhenAll(
                MetricsGrid.FadeTo(1.0, 400, Easing.CubicOut)
            );
            await Task.WhenAll(
                ChartCard.FadeTo(1.0, 400, Easing.CubicOut)
            );
            await Task.WhenAll(
                DistCard.FadeTo(1.0, 400, Easing.CubicOut)
            );
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _vm.PropertyChanged -= OnVmPropertyChanged;
        }

        private void OnVmPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Cuando la carga termina, renderizar gráficas
            if (e.PropertyName == nameof(_vm.IsBusy) && !_vm.IsBusy && !_barsRendered)
            {
                _dataReady = true;
                Device.BeginInvokeOnMainThread(RenderChartAndBars);
            }
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);
            // Re-renderizar si el layout ya tiene tamaño y los datos están listos
            if (_dataReady && !_barsRendered && width > 0)
                Device.BeginInvokeOnMainThread(RenderChartAndBars);
        }

        private async void RenderChartAndBars()
        {
            if (_barsRendered || _vm.MonthlyBars == null || _vm.MonthlyBars.Count == 0)
                return;

            _barsRendered = true;

            // ── Gráfico de barras ─────────────────────────────────────────────────
            BarChart.Children.Clear();
            MonthLabels.Children.Clear();

            const double maxBarHeight = 110.0;
            var bars = _vm.MonthlyBars;

            for (int i = 0; i < Math.Min(bars.Count, 6); i++)
            {
                var item = bars[i];
                double barH = Math.Max(6, item.NormalizedHeight * maxBarHeight);

                // Barra de fondo (gris tenue)
                var bgBar = new BoxView
                {
                    BackgroundColor = Application.Current.RequestedTheme == OSAppTheme.Dark
                        ? Color.FromHex("#2d2556") : Color.FromHex("#e2e8f0"),
                    CornerRadius = 4,
                    WidthRequest = 24,
                    HeightRequest = maxBarHeight,
                    VerticalOptions = LayoutOptions.End,
                    HorizontalOptions = LayoutOptions.Center
                };
                Grid.SetColumn(bgBar, i);
                BarChart.Children.Add(bgBar);

                // Barra de valor
                var bar = new BoxView
                {
                    BackgroundColor = Color.FromHex("#3211d4"),
                    CornerRadius = 4,
                    WidthRequest = 24,
                    HeightRequest = 0,
                    VerticalOptions = LayoutOptions.End,
                    HorizontalOptions = LayoutOptions.Center,
                    Opacity = 0
                };
                Grid.SetColumn(bar, i);
                BarChart.Children.Add(bar);

                // Etiqueta de mes
                var lbl = new Label
                {
                    Text = item.Label,
                    FontSize = 9,
                    TextColor = Color.FromHex("#94a3b8"),
                    HorizontalOptions = LayoutOptions.Center,
                    HorizontalTextAlignment = TextAlignment.Center
                };
                Grid.SetColumn(lbl, i);
                MonthLabels.Children.Add(lbl);

                // Animar altura de barra (secuencial con delay por columna)
                int delay = i * 80;
                var capturedBar = bar;
                var capturedH = barH;
#pragma warning disable CS4014
                Task.Run(async () =>
                {
                    await Task.Delay(delay);
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        capturedBar.HeightRequest = capturedH;
                        capturedBar.FadeTo(1.0, 350, Easing.CubicOut);
                    });
                });
#pragma warning restore CS4014
            }

            // ── Barras de progreso de distribución ────────────────────────────────
            // Esperamos a que el layout esté disponible
            await Task.Delay(100);

            double containerW = DistCard.Width;
            // Si el width aún no está disponible, usar estimado seguro basado en pantalla
            if (containerW <= 0)
                containerW = Application.Current.MainPage?.Width ?? 360;

            double innerW = Math.Max(0, containerW - 40); // padding 20+20

            await Task.WhenAll(
                AnimateProgressBar(WinRateBar, _vm.WinRateValue, innerW, 0),
                AnimateProgressBar(CashBar, _vm.CashPercent, innerW, 80),
                AnimateProgressBar(TechBar, _vm.TechPercent, innerW, 140),
                AnimateProgressBar(OtherBar, _vm.OtherPercent, innerW, 200)
            );
        }

        private static Task AnimateProgressBar(BoxView bar, double percent, double containerWidth, int delayMs)
        {
            double targetWidth = containerWidth * Math.Max(0, Math.Min(1, percent));
            var tcs = new TaskCompletionSource<bool>();

            Device.BeginInvokeOnMainThread(async () =>
            {
                await Task.Delay(delayMs);
                bar.WidthRequest = 0;

                var animation = new Animation(
                    v => bar.WidthRequest = v,
                    0,
                    targetWidth,
                    Easing.CubicOut
                );
                animation.Commit(
                    owner: bar,
                    name: "BarWidth_" + bar.AutomationId,
                    length: 600U,
                    finished: (_, __) => tcs.TrySetResult(true)
                );
            });

            return tcs.Task;
        }

        private async void OnBackTapped(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}

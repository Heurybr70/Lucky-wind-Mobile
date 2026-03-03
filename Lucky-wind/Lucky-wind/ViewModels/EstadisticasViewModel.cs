using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Lucky_wind.Models;
using Lucky_wind.Services;
using Xamarin.Forms;

namespace Lucky_wind.ViewModels
{
    /// <summary>
    /// ViewModel de la pantalla Análisis Estadístico.
    /// Calcula métricas y datos de gráfico a partir de los sorteos del usuario.
    /// </summary>
    public class EstadisticasViewModel : BaseViewModel
    {
        private readonly RaffleService _raffleService;

        // ─── Métricas principales ─────────────────────────────────────────────────
        private int _totalRaffles;
        public int TotalRaffles
        {
            get => _totalRaffles;
            set => SetProperty(ref _totalRaffles, value);
        }

        private string _winRateText = "0%";
        public string WinRateText
        {
            get => _winRateText;
            set => SetProperty(ref _winRateText, value);
        }

        private double _winRateValue;
        public double WinRateValue
        {
            get => _winRateValue;
            set => SetProperty(ref _winRateValue, value);
        }

        private string _growthText = "+0%";
        public string GrowthText
        {
            get => _growthText;
            set => SetProperty(ref _growthText, value);
        }

        private string _thisMonthText = "+0 este mes";
        public string ThisMonthText
        {
            get => _thisMonthText;
            set => SetProperty(ref _thisMonthText, value);
        }

        // ─── Datos de gráfico de barras (últimos 6 meses) ─────────────────────────
        private List<MonthBarItem> _monthlyBars = new List<MonthBarItem>();
        public List<MonthBarItem> MonthlyBars
        {
            get => _monthlyBars;
            set => SetProperty(ref _monthlyBars, value);
        }

        // ─── Distribución de premios ──────────────────────────────────────────────
        private double _cashPercent    = 0.65;
        private double _techPercent    = 0.20;
        private double _otherPercent   = 0.15;

        public double CashPercent    { get => _cashPercent;  set => SetProperty(ref _cashPercent, value); }
        public double TechPercent    { get => _techPercent;  set => SetProperty(ref _techPercent, value); }
        public double OtherPercent   { get => _otherPercent; set => SetProperty(ref _otherPercent, value); }

        public string CashPercentText  => $"{(int)(_cashPercent  * 100)}%";
        public string TechPercentText  => $"{(int)(_techPercent  * 100)}%";
        public string OtherPercentText => $"{(int)(_otherPercent * 100)}%";

        // ─── Error ────────────────────────────────────────────────────────────────
        private string _errorMessage;
        public string ErrorMessage
        {
            get => _errorMessage;
            set { SetProperty(ref _errorMessage, value); OnPropertyChanged(nameof(HasError)); }
        }
        public bool HasError => !string.IsNullOrEmpty(_errorMessage);

        // ─── Comandos ─────────────────────────────────────────────────────────────
        public ICommand LoadCommand { get; }

        // ─── Constructor ─────────────────────────────────────────────────────────
        public EstadisticasViewModel()
        {
            _raffleService = new RaffleService();
            Title          = "Estadísticas";

            LoadCommand = new Command(async () =>
            {
                if (IsBusy) return;
                IsBusy       = true;
                ErrorMessage = null;

                try
                {
                    var (success, raffles, error) = await _raffleService.GetRafflesAsync();
                    if (!success)
                    {
                        ErrorMessage = error;
                        return;
                    }

                    // ── Métricas principales ──────────────────────────────────────
                    TotalRaffles = raffles.Count;

                    int finished = raffles.Count(r => r.Status == RaffleStatus.Finished);
                    double winRate = TotalRaffles > 0
                        ? Math.Round((double)finished / TotalRaffles * 100, 0)
                        : 0;
                    WinRateValue = winRate / 100.0;
                    WinRateText  = $"{(int)winRate}%";

                    // Sorteos del mes actual
                    int thisMonth = raffles.Count(r =>
                        r.CreatedAt.Year  == DateTime.Now.Year &&
                        r.CreatedAt.Month == DateTime.Now.Month);
                    ThisMonthText = $"+{thisMonth} este mes";

                    // Crecimiento: comparar mes actual vs mes anterior
                    int prevMonth = raffles.Count(r =>
                        r.CreatedAt.Year  == DateTime.Now.AddMonths(-1).Year &&
                        r.CreatedAt.Month == DateTime.Now.AddMonths(-1).Month);
                    if (prevMonth > 0)
                    {
                        int growth = (int)Math.Round((double)(thisMonth - prevMonth) / prevMonth * 100);
                        GrowthText = (growth >= 0 ? "+" : "") + growth + "%";
                    }
                    else
                    {
                        GrowthText = thisMonth > 0 ? "+100%" : "—";
                    }

                    // ── Gráfico: últimos 6 meses ──────────────────────────────────
                    var bars = new List<MonthBarItem>();
                    int maxCount = 1;

                    for (int i = 5; i >= 0; i--)
                    {
                        var month  = DateTime.Now.AddMonths(-i);
                        int cnt    = raffles.Count(r =>
                            r.CreatedAt.Year  == month.Year &&
                            r.CreatedAt.Month == month.Month);
                        if (cnt > maxCount) maxCount = cnt;
                        bars.Add(new MonthBarItem
                        {
                            Label = month.ToString("MMM", new System.Globalization.CultureInfo("es-ES")),
                            Count = cnt
                        });
                    }

                    // Normalizar alturas al máximo
                    foreach (var b in bars)
                        b.NormalizedHeight = maxCount > 0 ? (double)b.Count / maxCount : 0;

                    MonthlyBars = bars;

                    // ── Distribución de premios (proporcional por tipo de nombre) ─
                    ComputePrizeDistribution(raffles);
                }
                finally
                {
                    IsBusy = false;
                }
            });
        }

        /// <summary>
        /// Clasifica los premios por palabras clave en la descripción.
        /// Efectivo/dinero → cash, electrónicos/tech → tech, resto → other.
        /// </summary>
        private void ComputePrizeDistribution(List<RaffleModel> raffles)
        {
            if (raffles.Count == 0) return;

            string[] cashKeywords = { "efectivo", "dinero", "cash", "dólares", "pesos", "colones" };
            string[] techKeywords = { "celular", "teléfono", "laptop", "tablet", "computadora",
                                      "mac", "iphone", "samsung", "airpod", "watch", "tv", "electrónico" };

            int cashCount = 0, techCount = 0, otherCount = 0;

            foreach (var r in raffles)
            {
                string desc = (r.PrizeDescription + " " + r.Name).ToLowerInvariant();
                if (techKeywords.Any(k => desc.Contains(k)))       techCount++;
                else if (cashKeywords.Any(k => desc.Contains(k)))  cashCount++;
                else                                                otherCount++;
            }

            int total = cashCount + techCount + otherCount;
            if (total == 0) return;

            CashPercent  = Math.Round((double)cashCount  / total, 2);
            TechPercent  = Math.Round((double)techCount  / total, 2);
            OtherPercent = Math.Round((double)otherCount / total, 2);

            OnPropertyChanged(nameof(CashPercentText));
            OnPropertyChanged(nameof(TechPercentText));
            OnPropertyChanged(nameof(OtherPercentText));
        }
    }

    /// <summary>Elemento de la barra del gráfico mensual.</summary>
    public class MonthBarItem
    {
        public string Label           { get; set; }
        public int    Count           { get; set; }
        public double NormalizedHeight{ get; set; } // 0.0 – 1.0
    }
}

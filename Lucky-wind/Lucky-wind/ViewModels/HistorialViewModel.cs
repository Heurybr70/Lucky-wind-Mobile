using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Lucky_wind.Models;
using Lucky_wind.Services;
using Xamarin.Forms;

namespace Lucky_wind.ViewModels
{
    /// <summary>
    /// ViewModel de la pantalla Historial.
    /// Carga los sorteos del usuario y los separa en activos/finalizados.
    /// </summary>
    public class HistorialViewModel : BaseViewModel
    {
        private readonly RaffleService _raffleService;

        // ─── Colecciones ──────────────────────────────────────────────────────────
        public ObservableCollection<RaffleModel> ActiveRaffles   { get; } = new ObservableCollection<RaffleModel>();
        public ObservableCollection<RaffleModel> FinishedRaffles { get; } = new ObservableCollection<RaffleModel>();

        // ─── Tab seleccionada ─────────────────────────────────────────────────────
        private int _selectedTab = 1; // 0 = Activos, 1 = Finalizados
        public int SelectedTab
        {
            get => _selectedTab;
            set
            {
                SetProperty(ref _selectedTab, value);
                OnPropertyChanged(nameof(ShowActive));
                OnPropertyChanged(nameof(ShowFinished));
                OnPropertyChanged(nameof(IsActiveTabSelected));
                OnPropertyChanged(nameof(IsFinishedTabSelected));
            }
        }

        public bool ShowActive      => SelectedTab == 0;
        public bool ShowFinished    => SelectedTab == 1;
        public bool IsActiveTabSelected   => SelectedTab == 0;
        public bool IsFinishedTabSelected => SelectedTab == 1;

        // ─── Estado vacío ─────────────────────────────────────────────────────────
        private bool _isEmpty;
        public bool IsEmpty
        {
            get => _isEmpty;
            set => SetProperty(ref _isEmpty, value);
        }

        // ─── Error ────────────────────────────────────────────────────────────────
        private string _errorMessage;
        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                SetProperty(ref _errorMessage, value);
                OnPropertyChanged(nameof(HasError));
            }
        }
        public bool HasError => !string.IsNullOrEmpty(_errorMessage);

        // ─── Comandos ─────────────────────────────────────────────────────────────
        public ICommand LoadCommand          { get; }
        public ICommand SelectActiveCommand  { get; }
        public ICommand SelectFinishedCommand{ get; }

        // ─── Constructor ─────────────────────────────────────────────────────────
        public HistorialViewModel()
        {
            _raffleService = new RaffleService();
            Title          = "Historial";

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

                    ActiveRaffles.Clear();
                    FinishedRaffles.Clear();

                    // Ordenar: más recientes primero
                    var sorted = raffles.OrderByDescending(r => r.CreatedAt).ToList();

                    foreach (var r in sorted)
                    {
                        if (r.Status == RaffleStatus.Finished)
                            FinishedRaffles.Add(r);
                        else
                            ActiveRaffles.Add(r);
                    }

                    // Actualizar estado vacío según la tab visible
                    UpdateIsEmpty();
                }
                finally
                {
                    IsBusy = false;
                }
            });

            SelectActiveCommand   = new Command(() => { SelectedTab = 0; UpdateIsEmpty(); });
            SelectFinishedCommand = new Command(() => { SelectedTab = 1; UpdateIsEmpty(); });
        }

        private void UpdateIsEmpty()
        {
            IsEmpty = SelectedTab == 0
                ? ActiveRaffles.Count   == 0
                : FinishedRaffles.Count == 0;
        }
    }
}

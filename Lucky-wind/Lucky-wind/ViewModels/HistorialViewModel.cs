using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Lucky_wind.Models;
using Lucky_wind.Services;
using Lucky_wind.Views;
using Xamarin.Essentials;
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
        private readonly INavigation   _navigation;

        // ─── Colecciones ──────────────────────────────────────────────────────────
        public ObservableCollection<RaffleModel> ActiveRaffles   { get; } = new ObservableCollection<RaffleModel>();
        public ObservableCollection<RaffleModel> FinishedRaffles { get; } = new ObservableCollection<RaffleModel>();

        // ─── Tab seleccionada ─────────────────────────────────────────────────────
        private int _selectedTab = 0; // 0 = Activos, 1 = Finalizados
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

        public bool ShowActive            => SelectedTab == 0;
        public bool ShowFinished          => SelectedTab == 1;
        public bool IsActiveTabSelected   => SelectedTab == 0;
        public bool IsFinishedTabSelected => SelectedTab == 1;

        // ─── Estado vacío ─────────────────────────────────────────────────────────
        private bool _isEmpty;
        public bool IsEmpty
        {
            get => _isEmpty;
            set => SetProperty(ref _isEmpty, value);
        }

        // ─── Pull-to-Refresh ──────────────────────────────────────────────────────
        private bool _isRefreshing;
        public bool IsRefreshing
        {
            get => _isRefreshing;
            set => SetProperty(ref _isRefreshing, value);
        }

        // ─── Banner sin conexión ──────────────────────────────────────────────────
        private bool _hasNoConnection;
        public bool HasNoConnection
        {
            get => _hasNoConnection;
            set => SetProperty(ref _hasNoConnection, value);
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
        public ICommand LoadCommand           { get; }
        public ICommand SelectActiveCommand   { get; }
        public ICommand SelectFinishedCommand { get; }
        public ICommand SelectRaffleCommand   { get; }
        public ICommand DeleteCommand         { get; }

        // ─── Constructor ─────────────────────────────────────────────────────────
        public HistorialViewModel(INavigation navigation)
        {
            _raffleService = new RaffleService();
            _navigation    = navigation;
            Title          = "Historial";

            LoadCommand = new Command(async () =>
            {
                if (IsBusy) return;
                IsBusy       = true;
                IsRefreshing = false;
                ErrorMessage = null;

                // Verificar conectividad
                HasNoConnection = Connectivity.NetworkAccess != NetworkAccess.Internet;

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

                    var sorted = raffles.OrderByDescending(r => r.CreatedAt).ToList();

                    foreach (var r in sorted)
                    {
                        if (r.Status == RaffleStatus.Finished)
                            FinishedRaffles.Add(r);
                        else
                            ActiveRaffles.Add(r);
                    }

                    UpdateIsEmpty();
                }
                finally
                {
                    IsBusy       = false;
                    IsRefreshing = false;
                }
            });

            SelectActiveCommand   = new Command(() => { SelectedTab = 0; UpdateIsEmpty(); });
            SelectFinishedCommand = new Command(() => { SelectedTab = 1; UpdateIsEmpty(); });

            SelectRaffleCommand = new Command<RaffleModel>(async (raffle) =>
            {
                if (raffle == null) return;
                await _navigation.PushAsync(new DetalleSorteoPage(raffle));
            });

            DeleteCommand = new Command<RaffleModel>(async (raffle) =>
            {
                if (raffle == null) return;

                bool confirmed = await Application.Current.MainPage.DisplayAlert(
                    "Eliminar sorteo",
                    $"¿Deseas eliminar permanentemente el sorteo \"{raffle.Name}\"?",
                    "Eliminar", "Cancelar");

                if (!confirmed) return;

                IsBusy = true;
                try
                {
                    var (success, error) = await new RaffleService().DeleteRaffleAsync(raffle.Id);
                    if (success)
                    {
                        ActiveRaffles.Remove(raffle);
                        FinishedRaffles.Remove(raffle);
                        UpdateIsEmpty();
                    }
                    else
                    {
                        await Application.Current.MainPage
                            .DisplayAlert("Error", error ?? "No se pudo eliminar el sorteo.", "Ok");
                    }
                }
                finally
                {
                    IsBusy = false;
                }
            });
        }

        private void UpdateIsEmpty()
        {
            IsEmpty = SelectedTab == 0
                ? ActiveRaffles.Count   == 0
                : FinishedRaffles.Count == 0;
        }
    }
}

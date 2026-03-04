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
    /// ViewModel de la pantalla Detalle de Sorteo.
    /// Permite gestionar participantes de un sorteo activo y ver los resultados de un finalizado.
    /// </summary>
    public class DetalleSorteoViewModel : BaseViewModel
    {
        private readonly RaffleService _raffleService;
        private readonly INavigation   _navigation;

        // ─── Sorteo ───────────────────────────────────────────────────────────────
        private RaffleModel _raffle;
        public RaffleModel Raffle
        {
            get => _raffle;
            set
            {
                SetProperty(ref _raffle, value);
                OnPropertyChanged(nameof(IsActive));
                OnPropertyChanged(nameof(IsFinished));
                OnPropertyChanged(nameof(CanRealizarSorteo));
            }
        }

        public bool IsActive   => Raffle?.IsActive   ?? false;
        public bool IsFinished => Raffle?.Status    == RaffleStatus.Finished;

        // ─── Participantes ────────────────────────────────────────────────────────
        public ObservableCollection<string> Participants { get; } = new ObservableCollection<string>();

        /// <summary>Texto del campo para añadir un participante nuevo.</summary>
        private string _participantInput;
        public string ParticipantInput
        {
            get => _participantInput;
            set => SetProperty(ref _participantInput, value);
        }

        public bool CanRealizarSorteo => Participants.Count > 0 && IsActive;

        // ─── Estado de guardado ───────────────────────────────────────────────────
        private bool _isSaving;
        public bool IsSaving
        {
            get => _isSaving;
            set => SetProperty(ref _isSaving, value);
        }

        // ─── Comandos ─────────────────────────────────────────────────────────────
        public ICommand AddParticipantCommand    { get; }
        public ICommand RemoveParticipantCommand { get; }
        public ICommand RealizarSorteoCommand    { get; }
        public ICommand DeleteRaffleCommand      { get; }
        public ICommand ShareResultCommand       { get; }
        public ICommand BackCommand              { get; }

        // ─── Constructor ─────────────────────────────────────────────────────────
        public DetalleSorteoViewModel(RaffleModel raffle, INavigation navigation)
        {
            _raffleService = new RaffleService();
            _navigation    = navigation;
            Raffle         = raffle;
            Title          = raffle?.Name ?? "Detalle";

            // Poblar participantes desde el sorteo
            if (raffle?.Participants != null)
                foreach (var p in raffle.Participants)
                    Participants.Add(p);

            // Cuando cambie la colección, notificar CanRealizarSorteo
            Participants.CollectionChanged += (s, e) =>
                OnPropertyChanged(nameof(CanRealizarSorteo));

            // ─── AddParticipant ───────────────────────────────────────────────────
            AddParticipantCommand = new Command(async () =>
            {
                string name = ParticipantInput?.Trim();
                if (string.IsNullOrEmpty(name)) return;
                if (Participants.Contains(name))
                {
                    await Application.Current.MainPage
                        .DisplayAlert("Duplicado", $"\"{name}\" ya está en la lista.", "Ok");
                    return;
                }

                Participants.Add(name);
                ParticipantInput = string.Empty;

                // Sincronizar en Firestore
                await SyncParticipantsAsync();
            });

            // ─── RemoveParticipant ────────────────────────────────────────────────
            RemoveParticipantCommand = new Command<string>(async (name) =>
            {
                if (string.IsNullOrEmpty(name)) return;
                Participants.Remove(name);
                await SyncParticipantsAsync();
            });

            // ─── RealizarSorteo ───────────────────────────────────────────────────
            RealizarSorteoCommand = new Command(async () =>
            {
                if (!CanRealizarSorteo) return;
                // Actualizar el modelo con la lista actual y navegar
                Raffle.Participants    = Participants.ToList();
                Raffle.ParticipantsCount = Participants.Count;
                await _navigation.PushAsync(new RealizarSorteoPage(Raffle));
            });

            // ─── DeleteRaffle ─────────────────────────────────────────────────────
            DeleteRaffleCommand = new Command(async () =>
            {
                bool confirmed = await Application.Current.MainPage.DisplayAlert(
                    "Eliminar sorteo",
                    $"¿Deseas eliminar permanentemente \"{Raffle?.Name}\"? Esta acción no se puede deshacer.",
                    "Eliminar", "Cancelar");

                if (!confirmed) return;

                IsBusy = true;
                try
                {
                    var (success, error) = await _raffleService.DeleteRaffleAsync(Raffle.Id);
                    if (success)
                        await _navigation.PopToRootAsync();
                    else
                        await Application.Current.MainPage
                            .DisplayAlert("Error", error ?? "No se pudo eliminar.", "Ok");
                }
                finally { IsBusy = false; }
            });

            // ─── ShareResult ──────────────────────────────────────────────────────
            ShareResultCommand = new Command(async () =>
            {
                if (Raffle == null) return;
                string text = $"🏆 ¡El ganador del sorteo \"{Raffle.Name}\" es {Raffle.WinnerName}!\n" +
                              $"Premio: {Raffle.PrizeDescription}\n" +
                              $"Organizado con Participa y Gana 🎉";
                await Share.RequestAsync(new ShareTextRequest
                {
                    Text    = text,
                    Title   = "Compartir resultado del sorteo"
                });
            });

            // ─── Back ─────────────────────────────────────────────────────────────
            BackCommand = new Command(async () => await _navigation.PopAsync());
        }

        // ─── Helpers ─────────────────────────────────────────────────────────────
        private async System.Threading.Tasks.Task SyncParticipantsAsync()
        {
            if (string.IsNullOrEmpty(Raffle?.Id)) return;
            IsSaving = true;
            try
            {
                var list = Participants.ToList();
                await _raffleService.UpdateParticipantsAsync(Raffle.Id, list);
                Raffle.Participants     = list;
                Raffle.ParticipantsCount = list.Count;
            }
            finally { IsSaving = false; }
        }
    }
}

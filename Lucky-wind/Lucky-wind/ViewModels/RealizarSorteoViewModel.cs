using System;
using System.Collections.Generic;
using System.Windows.Input;
using Lucky_wind.Models;
using Lucky_wind.Services;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Lucky_wind.ViewModels
{
    /// <summary>
    /// ViewModel de la pantalla Realizar Sorteo.
    /// Controla el estado de la selección aleatoria y la confirmación en Firestore.
    /// </summary>
    public class RealizarSorteoViewModel : BaseViewModel
    {
        private readonly RaffleService _raffleService;
        private readonly INavigation   _navigation;

        // ─── Estado del sorteo ────────────────────────────────────────────────────
        public enum SpinStateEnum { Waiting, Spinning, Winner }

        private SpinStateEnum _spinState = SpinStateEnum.Waiting;
        public SpinStateEnum SpinState
        {
            get => _spinState;
            set
            {
                SetProperty(ref _spinState, value);
                OnPropertyChanged(nameof(IsWaiting));
                OnPropertyChanged(nameof(IsSpinning));
                OnPropertyChanged(nameof(HasWinner));
            }
        }

        public bool IsWaiting  => SpinState == SpinStateEnum.Waiting;
        public bool IsSpinning => SpinState == SpinStateEnum.Spinning;
        public bool HasWinner  => SpinState == SpinStateEnum.Winner;

        // ─── Sorteo y participantes ───────────────────────────────────────────────
        public RaffleModel Raffle { get; }

        private List<ParticipantDisplayItem> _participantItems;
        public List<ParticipantDisplayItem> ParticipantItems
        {
            get => _participantItems;
            set => SetProperty(ref _participantItems, value);
        }

        // ─── Pantalla de spin ─────────────────────────────────────────────────────
        /// <summary>Nombre que se muestra en el círculo durante la animación.</summary>
        private string _currentDisplay = "?";
        public string CurrentDisplay
        {
            get => _currentDisplay;
            set => SetProperty(ref _currentDisplay, value);
        }

        /// <summary>Ganador definitivo (set antes de la animación para que el code-behind lo conozca).</summary>
        public string PendingWinner { get; private set; }

        /// <summary>Nombre del ganador mostrado después de confirmar.</summary>
        private string _winnerName;
        public string WinnerName
        {
            get => _winnerName;
            set => SetProperty(ref _winnerName, value);
        }

        // ─── Comandos ─────────────────────────────────────────────────────────────
        public ICommand IniciarSorteoCommand  { get; }
        public ICommand ConfirmWinnerCommand  { get; }
        public ICommand CompartirCommand      { get; }
        public ICommand BackCommand           { get; }

        // ─── Paleta de avatares ───────────────────────────────────────────────────
        private static readonly string[] AvatarColors =
        {
            "#3211d4", "#7c3aed", "#ec4899", "#f59e0b", "#10b981", "#3b82f6"
        };

        // ─── Constructor ─────────────────────────────────────────────────────────
        public RealizarSorteoViewModel(RaffleModel raffle, INavigation navigation)
        {
            _raffleService = new RaffleService();
            _navigation    = navigation;
            Raffle         = raffle;
            Title          = "Realizar Sorteo";

            // Construir items de visualización
            var items = new List<ParticipantDisplayItem>();
            var participants = raffle?.Participants ?? new List<string>();
            for (int i = 0; i < participants.Count; i++)
            {
                string p = participants[i];
                items.Add(new ParticipantDisplayItem
                {
                    Name        = p,
                    Initial     = string.IsNullOrEmpty(p) ? "?" : p[0].ToString().ToUpper(),
                    AvatarColor = Color.FromHex(AvatarColors[i % AvatarColors.Length])
                });
            }
            ParticipantItems = items;

            // ─── IniciarSorteo ────────────────────────────────────────────────────
            IniciarSorteoCommand = new Command(() =>
            {
                if (participants.Count == 0 || SpinState != SpinStateEnum.Waiting) return;

                // Elegir ganador aleatoriamente y guardarlo en PendingWinner
                var rng   = new Random();
                PendingWinner = participants[rng.Next(participants.Count)];

                // Disparar animación (el code-behind observa IsSpinning)
                SpinState = SpinStateEnum.Spinning;
            },
            () => SpinState == SpinStateEnum.Waiting && participants.Count > 0);

            // ─── ConfirmWinner — llamado por code-behind al finalizar la animación ─
            ConfirmWinnerCommand = new Command(async () =>
            {
                string winner = PendingWinner;
                WinnerName    = winner;
                CurrentDisplay = winner;
                SpinState     = SpinStateEnum.Winner;

                // Persistir en Firestore
                if (!string.IsNullOrEmpty(Raffle?.Id))
                {
                    var (success, _) = await _raffleService.FinishRaffleAsync(Raffle.Id, winner);
                    if (success)
                    {
                        Raffle.WinnerName = winner;
                        Raffle.Status     = RaffleStatus.Finished;
                    }
                }
            });

            // ─── Compartir ────────────────────────────────────────────────────────
            CompartirCommand = new Command(async () =>
            {
                string text = $"🏆 ¡El ganador del sorteo \"{Raffle?.Name}\" es {WinnerName}!\n" +
                              $"Premio: {Raffle?.PrizeDescription}\n\n" +
                              $"Organizado con Participa y Gana 🎉";
                await Share.RequestAsync(new ShareTextRequest
                {
                    Text  = text,
                    Title = "Compartir resultado"
                });
            });

            // ─── Back ─────────────────────────────────────────────────────────────
            BackCommand = new Command(async () => await _navigation.PopAsync());
        }

        // ─── Clase auxiliar ───────────────────────────────────────────────────────
        public class ParticipantDisplayItem
        {
            public string Name        { get; set; }
            public string Initial     { get; set; }
            public Color  AvatarColor { get; set; }
        }
    }
}

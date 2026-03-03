using System;
using System.Windows.Input;
using Lucky_wind.Models;
using Lucky_wind.Services;
using Lucky_wind.Views;
using Xamarin.Forms;

namespace Lucky_wind.ViewModels
{
    /// <summary>
    /// ViewModel para la pantalla de registro de un nuevo sorteo.
    /// Contiene validaciones robustas antes de persistir en Firestore.
    /// </summary>
    public class RegisterSorteoViewModel : BaseViewModel
    {
        private readonly RaffleService  _raffleService;
        private readonly INavigation    _navigation;

        // ─── Propiedades del formulario ───────────────────────────────────────────
        private string _raffleName;
        public string RaffleName
        {
            get => _raffleName;
            set
            {
                SetProperty(ref _raffleName, value);
                OnPropertyChanged(nameof(NameError));
                OnPropertyChanged(nameof(HasNameError));
            }
        }

        private DateTime _raffleDate = DateTime.Today.AddDays(7);
        public DateTime RaffleDate
        {
            get => _raffleDate;
            set
            {
                SetProperty(ref _raffleDate, value);
                OnPropertyChanged(nameof(DateError));
                OnPropertyChanged(nameof(HasDateError));
            }
        }

        private string _participantsCountText;
        public string ParticipantsCountText
        {
            get => _participantsCountText;
            set
            {
                SetProperty(ref _participantsCountText, value);
                OnPropertyChanged(nameof(ParticipantsError));
                OnPropertyChanged(nameof(HasParticipantsError));
            }
        }

        private string _prizeDescription;
        public string PrizeDescription
        {
            get => _prizeDescription;
            set
            {
                SetProperty(ref _prizeDescription, value);
                OnPropertyChanged(nameof(PrizeError));
                OnPropertyChanged(nameof(HasPrizeError));
            }
        }

        // ─── Validaciones ─────────────────────────────────────────────────────────
        public string NameError =>
            string.IsNullOrWhiteSpace(RaffleName)          ? "El nombre del sorteo es obligatorio." :
            RaffleName.Trim().Length < 3                   ? "El nombre debe tener al menos 3 caracteres." :
            RaffleName.Trim().Length > 80                  ? "El nombre no puede superar los 80 caracteres." : null;

        public bool HasNameError => NameError != null;

        public string DateError =>
            RaffleDate.Date < DateTime.Today                ? "La fecha no puede ser anterior a hoy." : null;

        public bool HasDateError => DateError != null;

        public string ParticipantsError
        {
            get
            {
                if (string.IsNullOrWhiteSpace(ParticipantsCountText))
                    return "El número de participantes es obligatorio.";
                if (!int.TryParse(ParticipantsCountText, out int n))
                    return "Ingresa un número entero válido.";
                if (n < 2)
                    return "Se necesitan al menos 2 participantes.";
                if (n > 100000)
                    return "El máximo permitido es 100,000 participantes.";
                return null;
            }
        }

        public bool HasParticipantsError => ParticipantsError != null;

        public string PrizeError =>
            string.IsNullOrWhiteSpace(PrizeDescription)   ? "La descripción del premio es obligatoria." :
            PrizeDescription.Trim().Length < 5            ? "Describe el premio con más detalle." : null;

        public bool HasPrizeError => PrizeError != null;

        // ─── Resultado de la operación ────────────────────────────────────────────
        private bool _isSuccess;
        public bool IsSuccess
        {
            get => _isSuccess;
            set => SetProperty(ref _isSuccess, value);
        }

        // ─── Comandos ─────────────────────────────────────────────────────────────
        public ICommand CreateCommand  { get; }
        public ICommand CancelCommand  { get; }

        // ─── Constructor ─────────────────────────────────────────────────────────
        public RegisterSorteoViewModel(INavigation navigation)
        {
            _navigation    = navigation;
            _raffleService = new RaffleService();
            Title          = "Registrar Sorteo";

            CreateCommand = new Command(async () =>
            {
                // Forzar evaluación de todos los errores
                OnPropertyChanged(nameof(NameError));
                OnPropertyChanged(nameof(DateError));
                OnPropertyChanged(nameof(ParticipantsError));
                OnPropertyChanged(nameof(PrizeError));

                if (HasNameError || HasDateError || HasParticipantsError || HasPrizeError)
                {
                    await Application.Current.MainPage
                        .DisplayAlert("Formulario incompleto",
                                      "Corrige los errores marcados antes de continuar.",
                                      "Entendido");
                    return;
                }

                IsBusy = true;
                IsSuccess = false;

                try
                {
                    int.TryParse(ParticipantsCountText, out int count);

                    var raffle = new RaffleModel
                    {
                        Name              = RaffleName.Trim(),
                        PrizeDescription  = PrizeDescription.Trim(),
                        ParticipantsCount = count,
                        RaffleDate        = RaffleDate,
                        UserId            = AuthService.CurrentUser?.LocalId ?? "",
                        Status            = RaffleStatus.Active,
                        CreatedAt         = DateTime.UtcNow
                    };

                    var (success, _, error) = await _raffleService.CreateRaffleAsync(raffle);

                    if (success)
                    {
                        IsSuccess = true;
                        await Application.Current.MainPage
                            .DisplayAlert("¡Sorteo creado!",
                                          $"\"{raffle.Name}\" ha sido registrado correctamente.",
                                          "Aceptar");
                        await _navigation.PopAsync();
                    }
                    else
                    {
                        await Application.Current.MainPage
                            .DisplayAlert("Error al guardar", error, "Aceptar");
                    }
                }
                finally
                {
                    IsBusy = false;
                }
            });

            CancelCommand = new Command(async () =>
            {
                await _navigation.PopAsync();
            });
        }
    }
}

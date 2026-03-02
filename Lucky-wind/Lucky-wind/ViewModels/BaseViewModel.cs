using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Lucky_wind.ViewModels
{
    /// <summary>
    /// ViewModel base que implementa INotifyPropertyChanged
    /// y expone propiedades comunes a todos los ViewModels.
    /// </summary>
    public class BaseViewModel : INotifyPropertyChanged
    {
        // ─── INotifyPropertyChanged ──────────────────────────────────────────────
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Notifica a la UI que una propiedad ha cambiado.
        /// </summary>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Asigna el valor al campo de respaldo y notifica si realmente cambió.
        /// </summary>
        protected bool SetProperty<T>(ref T backingField, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(backingField, value))
                return false;

            backingField = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        // ─── IsBusy ──────────────────────────────────────────────────────────────
        private bool _isBusy;
        /// <summary>Indica si el ViewModel está procesando una operación asíncrona.</summary>
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        // ─── Title ───────────────────────────────────────────────────────────────
        private string _title = string.Empty;
        /// <summary>Título descriptivo de la pantalla actual.</summary>
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }
    }
}

using Lucky_wind.ViewModels;
using Xamarin.Forms;

namespace Lucky_wind.Views
{
    /// <summary>
    /// Menú principal de la aplicación.
    /// Solo asigna el BindingContext; toda la lógica vive en DashboardViewModel.
    /// </summary>
    public partial class DashboardPage : ContentPage
    {
        public DashboardPage()
        {
            InitializeComponent();
            BindingContext = new DashboardViewModel();
        }
    }
}

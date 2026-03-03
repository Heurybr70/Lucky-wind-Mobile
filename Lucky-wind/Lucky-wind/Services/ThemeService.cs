using Xamarin.Forms;

namespace Lucky_wind.Services
{
    /// <summary>
    /// Servicio para alternar entre tema claro y oscuro en toda la aplicación.
    /// </summary>
    public static class ThemeService
    {
        /// <summary>Indica si el tema actual es oscuro.</summary>
        public static bool IsDark =>
            Application.Current.UserAppTheme == OSAppTheme.Dark;

        /// <summary>Alterna entre tema claro y oscuro.</summary>
        public static void Toggle()
        {
            Application.Current.UserAppTheme = IsDark
                ? OSAppTheme.Light
                : OSAppTheme.Dark;
        }

        /// <summary>Aplica el tema del sistema operativo.</summary>
        public static void UseSystemTheme() =>
            Application.Current.UserAppTheme = OSAppTheme.Unspecified;
    }
}

using System.Threading.Tasks;

namespace Lucky_wind.Services
{
    /// <summary>
    /// Interfaz que define el contrato para el inicio de sesión con Google.
    /// Retorna (IdToken, null) en éxito, (null, mensajeError) en fallo,
    /// o (null, null) si el usuario canceló.
    /// </summary>
    public interface IGoogleAuthService
    {
        Task<(string Token, string Error)> GetGoogleIdTokenAsync();
    }
}

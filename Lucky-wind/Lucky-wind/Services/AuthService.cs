using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Lucky_wind.Models;
using Newtonsoft.Json;

namespace Lucky_wind.Services
{
    /// <summary>
    /// Servicio de autenticación que consume la REST API de Firebase Authentication.
    /// </summary>
    public class AuthService
    {
        // ──────────────────────────────────────────────────────────────────────────
        // Clave Web API del proyecto Firebase lucky-wind-5fdfb
        // ──────────────────────────────────────────────────────────────────────────
        private const string FirebaseApiKey = "AIzaSyC0HVIzBngcIFjexzLgLwYAvyS0i1nnwdU";

        private const string SignInUrl =
            "https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key=" + FirebaseApiKey;

        private const string SignUpUrl =
            "https://identitytoolkit.googleapis.com/v1/accounts:signUp?key=" + FirebaseApiKey;

        // Endpoint para autenticación con proveedor OAuth (Google, etc.)
        private const string SignInWithIdpUrl =
            "https://identitytoolkit.googleapis.com/v1/accounts:signInWithIdp?key=" + FirebaseApiKey;

        private static readonly HttpClient _httpClient = new HttpClient();

        // ─── Sesión simulada en memoria ─────────────────────────────────────────
        /// <summary>Usuario actualmente autenticado (null si no hay sesión activa).</summary>
        public static UserModel CurrentUser { get; private set; }

        // ─── Login ───────────────────────────────────────────────────────────────
        /// <summary>
        /// Autentica al usuario con email y contraseña contra Firebase.
        /// </summary>
        /// <returns>Tupla (éxito, mensajeDeError).</returns>
        public async Task<(bool Success, string Error)> LoginAsync(string email, string password)
        {
            try
            {
                var payload = new
                {
                    email,
                    password,
                    returnSecureToken = true
                };

                string json = JsonConvert.SerializeObject(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _httpClient.PostAsync(SignInUrl, content)
                                                                 .ConfigureAwait(false);
                string responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<FirebaseAuthResponse>(responseBody);
                    CurrentUser = new UserModel
                    {
                        Email        = result.email,
                        IdToken      = result.idToken,
                        LocalId      = result.localId,
                        RefreshToken = result.refreshToken
                    };
                    return (true, null);
                }

                // Parsear mensaje de error de Firebase
                var errorResponse = JsonConvert.DeserializeObject<FirebaseErrorWrapper>(responseBody);
                string errorMessage = TranslateFirebaseError(errorResponse?.error?.message);
                return (false, errorMessage);
            }
            catch (HttpRequestException)
            {
                return (false, "Sin conexión a Internet. Verifica tu red e intenta de nuevo.");
            }
            catch (Exception ex)
            {
                return (false, $"Error inesperado: {ex.Message}");
            }
        }

        // ─── Register ────────────────────────────────────────────────────────────
        /// <summary>
        /// Registra un nuevo usuario en Firebase Authentication.
        /// </summary>
        /// <returns>Tupla (éxito, mensajeDeError).</returns>
        public async Task<(bool Success, string Error)> RegisterAsync(string email, string password)
        {
            try
            {
                var payload = new
                {
                    email,
                    password,
                    returnSecureToken = true
                };

                string json = JsonConvert.SerializeObject(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _httpClient.PostAsync(SignUpUrl, content)
                                                                 .ConfigureAwait(false);
                string responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                if (response.IsSuccessStatusCode)
                {
                    return (true, null);
                }

                var errorResponse = JsonConvert.DeserializeObject<FirebaseErrorWrapper>(responseBody);
                string errorMessage = TranslateFirebaseError(errorResponse?.error?.message);
                return (false, errorMessage);
            }
            catch (HttpRequestException)
            {
                return (false, "Sin conexión a Internet. Verifica tu red e intenta de nuevo.");
            }
            catch (Exception ex)
            {
                return (false, $"Error inesperado: {ex.Message}");
            }
        }
        // ─── Google Sign-In ──────────────────────────────────────────────────────────────
        /// <summary>
        /// Intercambia el Id Token de Google por una sesión de Firebase.
        /// </summary>
        public async Task<(bool Success, string Error)> SignInWithGoogleAsync(string googleIdToken)
        {
            try
            {
                var payload = new
                {
                    postBody             = $"id_token={googleIdToken}&providerId=google.com",
                    requestUri           = "http://localhost",
                    returnIdpCredential  = true,
                    returnSecureToken    = true
                };

                string json    = JsonConvert.SerializeObject(payload);
                var    content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response =
                    await _httpClient.PostAsync(SignInWithIdpUrl, content).ConfigureAwait(false);
                string responseBody =
                    await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<FirebaseAuthResponse>(responseBody);
                    CurrentUser = new UserModel
                    {
                        Email        = result.email,
                        IdToken      = result.idToken,
                        LocalId      = result.localId,
                        RefreshToken = result.refreshToken
                    };
                    return (true, null);
                }

                var errorResponse = JsonConvert.DeserializeObject<FirebaseErrorWrapper>(responseBody);
                return (false, TranslateFirebaseError(errorResponse?.error?.message));
            }
            catch (HttpRequestException)
            {
                return (false, "Sin conexión a Internet. Verifica tu red e intenta de nuevo.");
            }
            catch (Exception ex)
            {
                return (false, $"Error inesperado: {ex.Message}");
            }
        }
        // ─── Refresh token ────────────────────────────────────────────────────────
        /// <summary>
        /// Renueva el IdToken usando el RefreshToken. Firebase IdTokens expiran en 1 hora.
        /// Llama a este método antes de cualquier request a Firestore.
        /// </summary>
        public static async Task<bool> RefreshTokenIfNeededAsync()
        {
            if (CurrentUser == null) return false;
            if (string.IsNullOrEmpty(CurrentUser.RefreshToken)) return false;

            try
            {
                const string url = "https://securetoken.googleapis.com/v1/token?key=" + FirebaseApiKey;
                var payload = new { grant_type = "refresh_token", refresh_token = CurrentUser.RefreshToken };
                string json = JsonConvert.SerializeObject(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _httpClient.PostAsync(url, content).ConfigureAwait(false);
                string body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                if (!response.IsSuccessStatusCode) return false;

                var result = JsonConvert.DeserializeObject<Newtonsoft.Json.Linq.JObject>(body);
                string newIdToken      = result["id_token"]?.ToString();
                string newRefreshToken = result["refresh_token"]?.ToString();

                if (string.IsNullOrEmpty(newIdToken)) return false;

                CurrentUser.IdToken      = newIdToken;
                CurrentUser.RefreshToken = newRefreshToken ?? CurrentUser.RefreshToken;
                return true;
            }
            catch { return false; }
        }

        // ─── Logout ──────────────────────────────────────────────────────────────
        /// <summary>Cierra la sesión activa del usuario.</summary>
        public void Logout()
        {
            CurrentUser = null;
        }

        // ─── Helpers ─────────────────────────────────────────────────────────────
        /// <summary>Indica si existe una sesión activa.</summary>
        public static bool IsLoggedIn => CurrentUser != null;

        /// <summary>Traduce los códigos de error de Firebase a mensajes en español.</summary>
        private static string TranslateFirebaseError(string firebaseErrorCode)
        {
            if (string.IsNullOrEmpty(firebaseErrorCode))
                return "Ha ocurrido un error desconocido.";

            switch (firebaseErrorCode)
            {
                case "EMAIL_NOT_FOUND":
                    return "No existe una cuenta con ese correo electrónico.";
                case "INVALID_PASSWORD":
                    return "Contraseña incorrecta. Inténtalo de nuevo.";
                case "USER_DISABLED":
                    return "Esta cuenta ha sido deshabilitada.";
                case "EMAIL_EXISTS":
                    return "Ya existe una cuenta con ese correo electrónico.";
                case "WEAK_PASSWORD : Password should be at least 6 characters":
                case "WEAK_PASSWORD":
                    return "La contraseña debe tener al menos 6 caracteres.";
                case "INVALID_EMAIL":
                    return "El formato del correo electrónico no es válido.";
                case "TOO_MANY_ATTEMPTS_TRY_LATER":
                    return "Demasiados intentos. Por favor espera antes de reintentar.";
                default:
                    return $"Error de autenticación: {firebaseErrorCode}";
            }
        }

        // ─── Modelos internos para deserialización ───────────────────────────────
        private class FirebaseAuthResponse
        {
            public string idToken      { get; set; }
            public string email        { get; set; }
            public string refreshToken { get; set; }
            public string expiresIn    { get; set; }
            public string localId      { get; set; }
        }

        private class FirebaseErrorWrapper
        {
            public FirebaseError error { get; set; }
        }

        private class FirebaseError
        {
            public int    code    { get; set; }
            public string message { get; set; }
        }
    }
}

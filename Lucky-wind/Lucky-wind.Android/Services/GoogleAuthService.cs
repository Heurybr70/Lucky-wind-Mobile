using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Gms.Auth.Api.SignIn;
using Android.Gms.Common.Apis;
using Lucky_wind.Droid.Services;
using Lucky_wind.Services;
using Xamarin.Forms;

// Registro del DependencyService para que el código compartido pueda resolver IGoogleAuthService
[assembly: Dependency(typeof(GoogleAuthService))]

namespace Lucky_wind.Droid.Services
{
    /// <summary>
    /// Implementación Android de IGoogleAuthService usando Google Sign-In SDK.
    /// </summary>
    public class GoogleAuthService : IGoogleAuthService
    {
        // Web Client ID del proyecto Firebase (tipo 3 en google-services.json)
        private const string WebClientId =
            "542081571847-f7ig9810dsjr0fssp1jeuutk5n0sdn0k.apps.googleusercontent.com";

        private const int RcSignIn = 9001;

        // TaskCompletionSource para puente entre OnActivityResult y async/await
        private static TaskCompletionSource<(string Token, string Error)> _tcs;

        /// <summary>
        /// Lanza la pantalla nativa de selección de cuenta Google.
        /// </summary>
        public Task<(string Token, string Error)> GetGoogleIdTokenAsync()
        {
            _tcs = new TaskCompletionSource<(string, string)>();

            var activity = MainActivity.Instance;

            var gso = new GoogleSignInOptions.Builder(GoogleSignInOptions.DefaultSignIn)
                .RequestIdToken(WebClientId)
                .RequestEmail()
                .Build();

            var client = GoogleSignIn.GetClient(activity, gso);

            // Cerrar sesión Google previa para forzar la selección de cuenta
            client.SignOut();

            var intent = client.SignInIntent;
            activity.StartActivityForResult(intent, RcSignIn);

            return _tcs.Task;
        }

        /// <summary>
        /// Llamado desde MainActivity.OnActivityResult cuando regresa el resultado.
        /// </summary>
        public static void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            if (requestCode != RcSignIn) return;

            try
            {
                var task = GoogleSignIn.GetSignedInAccountFromIntent(data);

                if (task.IsSuccessful)
                {
                    var account = task.Result as GoogleSignInAccount;

                    if (account?.IdToken != null)
                    {
                        _tcs?.TrySetResult((account.IdToken, null));
                    }
                    else
                    {
                        // IdToken null = SHA-1 del keystore no registrado en Firebase
                        _tcs?.TrySetResult((null,
                            "No se pudo obtener el token de Google. " +
                            "Verifica que el SHA-1 del keystore esté registrado en Firebase."));
                    }
                }
                else
                {
                    // Extraer el código de error de ApiException
                    var apiEx = task.Exception?.Cause as ApiException
                             ?? task.Exception as ApiException;

                    string msg = apiEx != null
                        ? $"Error Google Sign-In (código {apiEx.StatusCode}). " +
                          "Revisa la consola de Firebase y el SHA-1."
                        : "Error desconocido en Google Sign-In.";

                    _tcs?.TrySetResult((null, msg));
                }
            }
            catch (Exception ex)
            {
                _tcs?.TrySetResult((null, $"Error inesperado: {ex.Message}"));
            }
        }
    }
}

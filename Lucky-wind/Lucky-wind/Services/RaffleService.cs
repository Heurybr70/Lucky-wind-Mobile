using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Lucky_wind.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Lucky_wind.Services
{
    /// <summary>
    /// Servicio CRUD de sorteos usando la REST API de Cloud Firestore.
    /// Colección: users/{userId}/raffles
    /// Proyecto Firebase: lucky-wind-5fdfb
    /// </summary>
    public class RaffleService
    {
        // ──────────────────────────────────────────────────────────────────────────
        private const string ProjectId  = "lucky-wind-5fdfb";
        private const string FirebaseApiKey = "AIzaSyC0HVIzBngcIFjexzLgLwYAvyS0i1nnwdU";

        private static readonly HttpClient _http = new HttpClient();

        /// <summary>URL base de la colección de sorteos del usuario actual.</summary>
        private static string CollectionUrl(string userId) =>
            $"https://firestore.googleapis.com/v1/projects/{ProjectId}/databases/(default)/documents/users/{userId}/raffles";

        /// <summary>URL de un documento específico.</summary>
        private static string DocumentUrl(string userId, string docId) =>
            $"{CollectionUrl(userId)}/{docId}";

        // ─── Crear sorteo ─────────────────────────────────────────────────────────
        /// <summary>Persiste un nuevo sorteo en Firestore y retorna el modelo con Id poblado.</summary>
        public async Task<(bool Success, RaffleModel Raffle, string Error)> CreateRaffleAsync(RaffleModel raffle)
        {
            try
            {
                await AuthService.RefreshTokenIfNeededAsync().ConfigureAwait(false);
                string userId  = AuthService.CurrentUser?.LocalId;
                string idToken = AuthService.CurrentUser?.IdToken;
                if (string.IsNullOrEmpty(userId)) return (false, null, "No hay sesión activa.");

                string body    = SerializeRaffle(raffle);
                var    content = new StringContent(body, Encoding.UTF8, "application/json");

                var request = new HttpRequestMessage(HttpMethod.Post, $"{CollectionUrl(userId)}?key={FirebaseApiKey}")
                {
                    Content = content
                };
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", idToken);

                HttpResponseMessage response = await _http.SendAsync(request).ConfigureAwait(false);
                string responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                    return (false, null, ParseFirestoreError(responseBody));

                raffle.Id = ExtractDocumentId(responseBody);
                return (true, raffle, null);
            }
            catch (HttpRequestException)
            {
                return (false, null, "Sin conexión a Internet.");
            }
            catch (Exception ex)
            {
                return (false, null, $"Error inesperado: {ex.Message}");
            }
        }

        // ─── Obtener todos los sorteos ────────────────────────────────────────────
        /// <summary>Carga todos los sorteos del usuario autenticado.</summary>
        public async Task<(bool Success, List<RaffleModel> Raffles, string Error)> GetRafflesAsync()
        {
            try
            {
                await AuthService.RefreshTokenIfNeededAsync().ConfigureAwait(false);
                string userId  = AuthService.CurrentUser?.LocalId;
                string idToken = AuthService.CurrentUser?.IdToken;
                if (string.IsNullOrEmpty(userId)) return (false, null, "No hay sesión activa.");

                var request = new HttpRequestMessage(
                    HttpMethod.Get,
                    $"{CollectionUrl(userId)}?key={FirebaseApiKey}&orderBy=createdAt&pageSize=200");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", idToken);

                HttpResponseMessage response = await _http.SendAsync(request).ConfigureAwait(false);
                string responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                    return (false, null, ParseFirestoreError(responseBody));

                var raffles = DeserializeRaffleList(responseBody);
                return (true, raffles, null);
            }
            catch (HttpRequestException)
            {
                return (false, null, "Sin conexión a Internet.");
            }
            catch (Exception ex)
            {
                return (false, null, $"Error inesperado: {ex.Message}");
            }
        }

        // ─── Actualizar estado (finalizar sorteo) ─────────────────────────────────
        /// <summary>Marca un sorteo como finalizado y registra al ganador.</summary>
        public async Task<(bool Success, string Error)> FinishRaffleAsync(string raffleId, string winnerName)
        {
            try
            {
                await AuthService.RefreshTokenIfNeededAsync().ConfigureAwait(false);
                string userId  = AuthService.CurrentUser?.LocalId;
                string idToken = AuthService.CurrentUser?.IdToken;

                var patchBody = new
                {
                    fields = new
                    {
                        status     = new { stringValue  = RaffleStatus.Finished },
                        winnerName = new { stringValue  = winnerName ?? "" }
                    }
                };

                string body    = JsonConvert.SerializeObject(patchBody);
                var    content = new StringContent(body, Encoding.UTF8, "application/json");

                // PATCH con updateMask para no sobreescribir otros campos
                string url = $"{DocumentUrl(userId, raffleId)}?key={FirebaseApiKey}" +
                             "&updateMask.fieldPaths=status&updateMask.fieldPaths=winnerName";

                var request = new HttpRequestMessage(new HttpMethod("PATCH"), url) { Content = content };
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", idToken);

                HttpResponseMessage response = await _http.SendAsync(request).ConfigureAwait(false);
                string responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                return response.IsSuccessStatusCode
                    ? (true, null)
                    : (false, ParseFirestoreError(responseBody));
            }
            catch (Exception ex)
            {
                return (false, $"Error inesperado: {ex.Message}");
            }
        }

        // ─── Eliminar sorteo ──────────────────────────────────────────────────────
        /// <summary>Elimina permanentemente un sorteo de Firestore.</summary>
        public async Task<(bool Success, string Error)> DeleteRaffleAsync(string raffleId)
        {
            try
            {
                await AuthService.RefreshTokenIfNeededAsync().ConfigureAwait(false);
                string userId  = AuthService.CurrentUser?.LocalId;
                string idToken = AuthService.CurrentUser?.IdToken;
                if (string.IsNullOrEmpty(userId)) return (false, "No hay sesión activa.");

                var request = new HttpRequestMessage(
                    HttpMethod.Delete,
                    $"{DocumentUrl(userId, raffleId)}?key={FirebaseApiKey}");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", idToken);

                HttpResponseMessage response = await _http.SendAsync(request).ConfigureAwait(false);
                string responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                return response.IsSuccessStatusCode
                    ? (true, null)
                    : (false, ParseFirestoreError(responseBody));
            }
            catch (HttpRequestException)
            {
                return (false, "Sin conexión a Internet.");
            }
            catch (Exception ex)
            {
                return (false, $"Error inesperado: {ex.Message}");
            }
        }

        // ─── Actualizar participantes ──────────────────────────────────────────────
        /// <summary>Reemplaza la lista de participantes del sorteo en Firestore (PATCH).</summary>
        public async Task<(bool Success, string Error)> UpdateParticipantsAsync(string raffleId, List<string> participants)
        {
            try
            {
                await AuthService.RefreshTokenIfNeededAsync().ConfigureAwait(false);
                string userId  = AuthService.CurrentUser?.LocalId;
                string idToken = AuthService.CurrentUser?.IdToken;
                if (string.IsNullOrEmpty(userId)) return (false, "No hay sesión activa.");

                // Construir arrayValue de Firestore
                var values = new JArray();
                foreach (var p in participants ?? new List<string>())
                    values.Add(JObject.FromObject(new { stringValue = p }));

                var patchBody = new JObject
                {
                    ["fields"] = new JObject
                    {
                        ["participants"] = new JObject
                        {
                            ["arrayValue"] = new JObject { ["values"] = values }
                        },
                        ["participantsCount"] = new JObject
                        {
                            ["integerValue"] = (participants?.Count ?? 0).ToString()
                        }
                    }
                };

                var content = new StringContent(
                    patchBody.ToString(Formatting.None), Encoding.UTF8, "application/json");

                string url = $"{DocumentUrl(userId, raffleId)}?key={FirebaseApiKey}" +
                             "&updateMask.fieldPaths=participants&updateMask.fieldPaths=participantsCount";

                var request = new HttpRequestMessage(new HttpMethod("PATCH"), url) { Content = content };
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", idToken);

                HttpResponseMessage response = await _http.SendAsync(request).ConfigureAwait(false);
                string responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                return response.IsSuccessStatusCode
                    ? (true, null)
                    : (false, ParseFirestoreError(responseBody));
            }
            catch (HttpRequestException)
            {
                return (false, "Sin conexión a Internet.");
            }
            catch (Exception ex)
            {
                return (false, $"Error inesperado: {ex.Message}");
            }
        }

        // ─── Serialización / Deserialización Firestore ────────────────────────────

        /// <summary>Convierte un RaffleModel al formato de documento Firestore.</summary>
        private static string SerializeRaffle(RaffleModel r)
        {
            // Construir la lista de participantes como arrayValue
            var participantValues = new JArray();
            if (r.Participants != null)
                foreach (var p in r.Participants)
                    participantValues.Add(JObject.FromObject(new { stringValue = p }));

            var doc = new JObject
            {
                ["fields"] = new JObject
                {
                    ["name"]              = new JObject { ["stringValue"]  = r.Name              ?? "" },
                    ["prizeDescription"]  = new JObject { ["stringValue"]  = r.PrizeDescription  ?? "" },
                    ["participantsCount"] = new JObject { ["integerValue"] = r.ParticipantsCount.ToString() },
                    ["raffleDate"]        = new JObject { ["timestampValue"] = r.RaffleDate.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'") },
                    ["status"]            = new JObject { ["stringValue"]  = r.Status            ?? RaffleStatus.Active },
                    ["winnerName"]        = new JObject { ["stringValue"]  = r.WinnerName        ?? "" },
                    ["userId"]            = new JObject { ["stringValue"]  = r.UserId            ?? "" },
                    ["createdAt"]         = new JObject { ["timestampValue"] = r.CreatedAt.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'") },
                    ["participants"]      = new JObject { ["arrayValue"] = new JObject { ["values"] = participantValues } }
                }
            };
            return doc.ToString(Formatting.None);
        }

        /// <summary>Extrae el ID del documento de la respuesta de creación.</summary>
        private static string ExtractDocumentId(string json)
        {
            try
            {
                var obj  = JObject.Parse(json);
                string name = obj["name"]?.ToString() ?? "";
                // name = "projects/.../documents/users/{uid}/raffles/{docId}"
                var parts = name.Split('/');
                return parts.Length > 0 ? parts[parts.Length - 1] : Guid.NewGuid().ToString();
            }
            catch { return Guid.NewGuid().ToString(); }
        }

        /// <summary>Deserializa la lista de documentos Firestore a una lista de RaffleModel.</summary>
        private static List<RaffleModel> DeserializeRaffleList(string json)
        {
            var result = new List<RaffleModel>();
            try
            {
                var root = JObject.Parse(json);
                var docs = root["documents"] as JArray;
                if (docs == null) return result;

                foreach (var doc in docs)
                {
                    var fields = doc["fields"] as JObject;
                    if (fields == null) continue;

                    string docName = doc["name"]?.ToString() ?? "";
                    var    parts   = docName.Split('/');
                    string docId   = parts.Length > 0 ? parts[parts.Length - 1] : "";

                    var raffle = new RaffleModel
                    {
                        Id                = docId,
                        Name              = fields["name"]?["stringValue"]?.ToString()              ?? "",
                        PrizeDescription  = fields["prizeDescription"]?["stringValue"]?.ToString()  ?? "",
                        WinnerName        = fields["winnerName"]?["stringValue"]?.ToString()        ?? "",
                        Status            = fields["status"]?["stringValue"]?.ToString()            ?? RaffleStatus.Active,
                        UserId            = fields["userId"]?["stringValue"]?.ToString()            ?? "",
                    };

                    // participantsCount
                    if (int.TryParse(fields["participantsCount"]?["integerValue"]?.ToString(), out int count))
                        raffle.ParticipantsCount = count;

                    // raffleDate
                    if (DateTime.TryParse(fields["raffleDate"]?["timestampValue"]?.ToString(), out DateTime raffleDate))
                        raffle.RaffleDate = raffleDate;

                    // createdAt
                    if (DateTime.TryParse(fields["createdAt"]?["timestampValue"]?.ToString(), out DateTime createdAt))
                        raffle.CreatedAt = createdAt;

                    // participants (arrayValue)
                    var pArr = fields["participants"]?["arrayValue"]?["values"] as JArray;
                    if (pArr != null)
                        foreach (var pItem in pArr)
                        {
                            string pName = pItem["stringValue"]?.ToString();
                            if (!string.IsNullOrWhiteSpace(pName))
                                raffle.Participants.Add(pName);
                        }

                    result.Add(raffle);
                }
            }
            catch { /* devuelve lo que se pudo deserializar */ }
            return result;
        }

        /// <summary>Extrae el mensaje de error de una respuesta Firestore.</summary>
        private static string ParseFirestoreError(string json)
        {
            try
            {
                var obj    = JObject.Parse(json);
                string msg = obj["error"]?["message"]?.ToString();
                return string.IsNullOrEmpty(msg) ? "Error desconocido de Firestore." : msg;
            }
            catch { return "Error al procesar la respuesta del servidor."; }
        }
    }
}

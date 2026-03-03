using System;

namespace Lucky_wind.Models
{
    /// <summary>
    /// Representa un sorteo registrado por el usuario.
    /// Se persiste en Firebase Firestore bajo la colección "raffles/{userId}/items".
    /// </summary>
    public class RaffleModel
    {
        // ─── Identidad ────────────────────────────────────────────────────────────
        /// <summary>ID del documento en Firestore (asignado al crear).</summary>
        public string Id { get; set; }

        /// <summary>ID del usuario dueño del sorteo.</summary>
        public string UserId { get; set; }

        // ─── Datos principales ───────────────────────────────────────────────────
        /// <summary>Nombre descriptivo del sorteo.</summary>
        public string Name { get; set; }

        /// <summary>Descripción de los premios.</summary>
        public string PrizeDescription { get; set; }

        /// <summary>Número total de participantes.</summary>
        public int ParticipantsCount { get; set; }

        /// <summary>Fecha programada del sorteo.</summary>
        public DateTime RaffleDate { get; set; }

        // ─── Estado ───────────────────────────────────────────────────────────────
        /// <summary>Estado del sorteo: "active" o "finished".</summary>
        public string Status { get; set; } = RaffleStatus.Active;

        /// <summary>Nombre del ganador (se completa al finalizar).</summary>
        public string WinnerName { get; set; }

        // ─── Auditoría ────────────────────────────────────────────────────────────
        /// <summary>Fecha/hora de creación en UTC.</summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // ─── Calculados ───────────────────────────────────────────────────────────
        /// <summary>Indica si el sorteo está activo.</summary>
        public bool IsActive => Status == RaffleStatus.Active;

        /// <summary>Fecha formateada para mostrar en la UI.</summary>
        public string FormattedDate => RaffleDate.ToString("dd 'de' MMMM, yyyy",
            new System.Globalization.CultureInfo("es-ES"));
    }

    /// <summary>Constantes de estado para RaffleModel.</summary>
    public static class RaffleStatus
    {
        public const string Active   = "active";
        public const string Finished = "finished";
    }
}

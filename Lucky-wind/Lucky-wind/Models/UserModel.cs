namespace Lucky_wind.Models
{
    /// <summary>
    /// Representa al usuario autenticado con sus tokens de Firebase.
    /// </summary>
    public class UserModel
    {
        /// <summary>Correo electrónico del usuario.</summary>
        public string Email { get; set; }

        /// <summary>Token de identidad retornado por Firebase.</summary>
        public string IdToken { get; set; }

        /// <summary>Identificador único del usuario en Firebase.</summary>
        public string LocalId { get; set; }

        /// <summary>Token de refresco para mantener la sesión.</summary>
        public string RefreshToken { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace proyect.domain.Entidades
{
    public class RefreshToken
    {
        public int Id_RefreshToken { get; set; }
        public int IdUsuario { get; set; }
        public string TokenHash { get; set; } = default!;
        public DateTime FechaCreacion { get; set; }
        public DateTime FechaExpiracion { get; set; }
        public DateTime? FechaRevocacion { get; set; }
        public string? ReemplazadoPorTokenHash { get; set; }
        public string? IpCreacion { get; set; }

        public ApplicationUser Usuario { get; set; } = default!;

        public bool EstaActivo => FechaRevocacion is null && DateTime.UtcNow < FechaExpiracion;
    }
}

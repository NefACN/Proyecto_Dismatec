using System;
using System.Collections.Generic;
using System.Text;

namespace proyect.domain.Entidades
{
    public class RolUsuario
    {
        public long IdRolUsuario { get; set; }
        public int IdUsuario { get; set; }
        public int IdRol { get; set; }
        public DateTime FechaInicio { get; set; } = DateTime.UtcNow;
        public DateTime? FechaFin {  get; set; }
        public bool Activo { get; set; } = true;
        public int? AsignadoPor { get; set; }
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        public DateTime? FechaModificacion { get; set; }
        public ApplicationUser Usuario { get; set; } = null!;
        public Rol Rol { get; set; } = null!;
        public ApplicationUser? AsignadoPorUsuario { get; set; }

    }
}

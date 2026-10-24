using System;
using System.Collections.Generic;
using System.Text;

namespace proyect.domain.Entidades
{
    public class Rol
    {
        public int IdRol { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public bool Activo { get; set; } = true;
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        public DateTime? FechaModificacion { get; set; }

        //para navegacion
        public ICollection<RolUsuario> UsuariosAsignados { get; set; } = new List<RolUsuario>();
    }
}

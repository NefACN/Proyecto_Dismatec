using System;
using System.Collections.Generic;
using System.Text;

namespace proyect.domain.Entidades
{
    public class RegistroAuditoria
    {
        public long Id_Auditoria { get; set; }
        public int? IdUsuario { get; set; }
        public string? NombreUsuario { get; set; } 
        public string Accion { get; set; } = default!; 
        public string? Entidad { get; set; }
        public string? IdEntidadAfectada { get; set; }
        public string? Detalles { get; set; } 
        public string? IpOrigen { get; set; }
        public bool Exitoso { get; set; }
        public DateTime FechaHora { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.AspNetCore.Identity;

namespace proyect.domain.Entidades
{
    public class ApplicationUser : IdentityUser<int>
    {
        public string Nombre { get; set; } = string.Empty;
        public string Apellido {  get; set; } = string.Empty;
        public bool Activo { get; set; } = true;
        public bool DebeCambiarPassword { get; set; } = false;
        public DateTime FechaCreacion {  get; set; } = DateTime.UtcNow;
        public DateTime? FechaModificacion { get; set; }

        // para navegacion de los propios roles y no los del Indentity
        public ICollection<RolUsuario> RolesAsignados { get; set; } = new List<RolUsuario>();

        public int? Id_Sucursal { get; set; }
        public int? Id_Area {  get; set; }
        public decimal ScoreRiesgo { get; set; } = 0.00m;

        //nav
        public Sucursal? Sucursal { get; set; }
        public AreaOperativa? AreaOperativa { get; set; }

        //del score de concientizacion
        public int CyberScore { get; set; } = 0;
        public ICollection<LogroUsuario> LogrosObtenidos { get; set; } = new List<LogroUsuario>();

    }
}

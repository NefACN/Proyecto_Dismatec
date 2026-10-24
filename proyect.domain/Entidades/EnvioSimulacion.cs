using System;
using System.Collections.Generic;
using System.Text;

namespace proyect.domain.Entidades
{
    public class EnvioSimulacion
    {
        public int Id_Envio { get; set; }
        public int Id_Campania { get; set; }
        public int Id_Usuario { get; set; }

        public Guid TokenUuid { get; set; }

        public bool FueAbierto { get; set; } = false;
        public bool HizoClick { get; set; } = false;
        public DateTime? FechaVulneracion { get; set; }

        public CampaniaSimulacion Campania { get; set; } = null!;
        public ApplicationUser Usuario { get; set; } = null!;

        //concientizacion
        public bool CapacitacionCompleta { get; set; } = false;

        public DateTime? FechaCapacitacion { get; set; }
        public int PuntosRecuperados { get; set; } = 0;
    }
}

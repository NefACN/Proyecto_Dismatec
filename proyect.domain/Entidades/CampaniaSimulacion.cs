using System;
using System.Collections.Generic;
using System.Text;

namespace proyect.domain.Entidades
{
    public class CampaniaSimulacion
    {
        public int Id_Campania { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public int Id_Plantilla { get; set; }
        public int Id_Estado { get; set; }
        public int CreadoPor { get; set; }
        public DateTime? FechaInicioProgramada { get; set; }
        public DateTime? FechaFinProgramada { get; set; }
        public PlantillaPhishing Plantilla { get; set; } = null!;
        public EstadoFlujo Estado { get; set; } = null!;
        public ApplicationUser Creador { get; set; } = null!;

        public ICollection<SegmentacionCampaniArea> Segmentaciones { get; set; } = new List<SegmentacionCampaniArea>();
        public ICollection<EnvioSimulacion> Envios { get; set; } = new List<EnvioSimulacion>();

        //concientizacion
        public int NivelDificultad { get; set;  }

    }
}

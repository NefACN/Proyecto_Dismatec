using System;
using System.Collections.Generic;
using System.Text;

namespace proyect.domain.Entidades
{
    public class SegmentacionCampaniArea
    {
        public int Id_Segmentacion { get; set; }
        public int Id_Campania { get; set; }
        public int Id_Area { get; set; }
        public CampaniaSimulacion Campania { get; set; } = null!;
        public AreaOperativa AreaOperativa { get; set; } = null!;
    }
}

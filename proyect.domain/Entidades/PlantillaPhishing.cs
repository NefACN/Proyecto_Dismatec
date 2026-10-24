using System;
using System.Collections.Generic;
using System.Text;

namespace proyect.domain.Entidades
{
    public class PlantillaPhishing
    {
        public int Id_Plantilla { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string AsuntoCorreo { get; set; } = string.Empty;
        public string CuerpoHtml { get; set; } = string.Empty;
        public int Id_Criticidad { get; set; }

        public NivelCriticidad Criticidad { get; set; } = null!;

        //concientizacion
        public int Id_CategoriaPegunta {  get; set; }
        public CategoriaPregunta CategoriaPregunta { get; set; } = null!;
    }
}

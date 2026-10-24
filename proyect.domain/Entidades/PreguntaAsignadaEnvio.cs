using System;
using System.Collections.Generic;
using System.Text;

namespace proyect.domain.Entidades
{
    public class PreguntaAsignadaEnvio
    {
        public int Id_Asignacion {  get; set; }
        public int Id_Envio { get; set; }
        public int Id_Pregunta { get; set; }
        public EnvioSimulacion Envio { get; set; } = null!;
        public PreguntaConcientizacion Pregunta { get; set; } = null!;
    }
}

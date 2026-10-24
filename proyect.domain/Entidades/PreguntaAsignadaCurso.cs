using System;
using System.Collections.Generic;
using System.Text;

namespace proyect.domain.Entidades
{
    public class PreguntaAsignadaCurso
    {
        public int Id_Asignacion {  get; set; }
        public int Id_Intento { get; set; }
        public int Id_Pregunta {  get; set; }

        public IntentoCursoVoluntario Intento { get; set; } = null!;
        public PreguntaConcientizacion Pregunta { get; set; } = null!;
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace proyect.domain.Entidades
{
    public class IntentoCursoVoluntario
    {
        public int Id_Intento {  get; set; }
        public int Id_Usuario {  get; set; }
        public int Id_Categoria {  get; set; }

        public DateTime FechaInicio { get; set; }
        public DateTime? FechaCompletado { get; set; }
        public bool? Exitoso { get; set; }
        public int PuntosObtenidos { get; set; }

        public ApplicationUser Usuario { get; set; } = null!;
        public CategoriaPregunta Categoria { get; set; } = null!;

        public List<PreguntaAsignadaCurso> PreguntasAsignadas { get; set; } = new();
        
        
    }
}

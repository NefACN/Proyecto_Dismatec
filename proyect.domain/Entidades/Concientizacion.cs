using System;
using System.Collections.Generic;
using System.Text;

namespace proyect.domain.Entidades
{
    public class CategoriaPregunta
    {
        public int IdCategoria {  get; set; }
        public string Nombre { get; set; } = string.Empty;
        public ICollection<PreguntaConcientizacion> Preguntas { get; set; } = new List<PreguntaConcientizacion>();

    }

    public class PreguntaConcientizacion
    {
        public int Id_Pregunta { get; set; }
        public string TextoPregunta { get; set; } = string.Empty;
        public string ExplicacionEducativa { get; set; } = string.Empty; //"Teachable Moment"
        public int Id_Categoria { get; set; }

        public CategoriaPregunta Categoria { get; set; } = null!;
        public ICollection<OpcionRespuesta> Opciones { get; set; } = new List<OpcionRespuesta>();
    }

    public class OpcionRespuesta
    {
        public int Id_Opcion { get; set; }
        public string TextoOpcion { get; set; } = string.Empty;
        public bool EsCorrecta { get; set; }
        public int Id_Pregunta { get; set; }
        
        public PreguntaConcientizacion Pregunta { get; set; } = null!;
    }

    //"gamificacion" jsjs
    public class Insignia
    {
        public int Id_Insignia { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string IconoSvg { get; set; } = string.Empty;
        public int PuntosRequeridos { get; set; }
    }

    public class LogroUsuario
    {
        public int Id_Logro { get; set; }
        public int Id_Usuario { get; set; }
        public int Id_Insignia { get; set; }
        public DateTime FechaObtencion { get; set; }

        public ApplicationUser Usuario { get; set; } = null!;
        public Insignia Insignia { get; set; } = null!;
    }
}

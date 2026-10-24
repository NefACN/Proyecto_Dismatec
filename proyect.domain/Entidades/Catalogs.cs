using System;
using System.Collections.Generic;
using System.Text;

namespace proyect.domain.Entidades
{
    public class Ciudad
    {
        public int Id_Ciudad { get; set; }
        public string Nombre { get; set; } = string.Empty;
    }
    public class NivelCriticidad
    {
        public int Id_NivelCriticidad { get; set; }
        public string Nivel { get; set; } = string.Empty;
    }
    public class Prioridad
    {
        public int Id_Prioridad { get; set; }
        public string Nombre { get; set; } = string.Empty;
    }
    public class EstadoFlujo
    {
        public int Id_Estado { get; set; }
        public string NombreEstado { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
    }
}

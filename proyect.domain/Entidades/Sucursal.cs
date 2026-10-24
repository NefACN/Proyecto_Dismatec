using System;
using System.Collections.Generic;
using System.Text;

namespace proyect.domain.Entidades
{
    public class Sucursal
    {
        public int Id_Sucursal { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int Id_Ciudad { get; set; }
        public string? Direccion { get; set; }
        public int? Id_GerenteResponsable { get; set; }
        public bool Activo { get; set; } = true;

        public Ciudad Ciudad { get; set; } = null!;
        public ApplicationUser? GerenteResponsable { get; set;} 
    }
}

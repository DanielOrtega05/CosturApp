using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosturApp.Modelo
{
    public class Orden
    {
        public int Id { get; set; }
        public string NumeroOrden { get; set; }  // String por que puede ser "958", "Encargos", etc.
        public int TotalCamisetas { get; set; }
        public int AnexoId { get; set; }
    }

}

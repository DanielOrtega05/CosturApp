using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosturApp.Modelo
{
    public class Anexo
    {
        [Key]
        public int Id { get; set; }
        public string Titulo { get; set; }
        public DateTime FechaCreacion { get; set; }
    }
}

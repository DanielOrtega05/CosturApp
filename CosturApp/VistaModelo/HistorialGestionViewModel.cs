using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CosturApp.Modelo;
using CosturApp.Servicio;

namespace CosturApp.VistaModelo
{
    public class HistorialGestionViewModel
    {
        private HistorialService _historialService;

        public HistorialGestionViewModel()
        {
            _historialService = new HistorialService();
            ListaHistorial = new ObservableCollection<Historial>(_historialService.ObtenerHistorialCompleto());
        }

        public ObservableCollection<Historial> ListaHistorial { get; set; }
    }
}

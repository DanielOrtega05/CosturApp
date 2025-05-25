using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using CosturApp.Modelo;
using CosturApp.Servicio;

namespace CosturApp.VistaModelo
{
    public class HistorialGestionViewModel
    {
        private HistorialService _historialService;
        public ObservableCollection<Historial> ListaHistorial { get; set; }
        public ICommand EliminarTodoHistorialCommand { get; }

        private RelayCommand _eliminarCommand;

        public HistorialGestionViewModel()
        {
            _historialService = new HistorialService();
            ListaHistorial = new ObservableCollection<Historial>(_historialService.ObtenerHistorialCompleto());

            // Actualiza el estado del comando para que se habilite el boton de eliminar historial si hay contenido
            ListaHistorial.CollectionChanged += (s, e) =>
            {
                _eliminarCommand.RaiseCanExecuteChanged();
            };

            HistorialService.HistorialAgregado += historial =>
            {
                // Aseguro que se actualice desde el hilo de la interfaz
                Application.Current.Dispatcher.Invoke(() =>
                {
                    ListaHistorial.Insert(0, historial); // Insertar al inicio de la lista el mas reciente
                });
            };

            _eliminarCommand = new RelayCommand(EliminarTodoHistorial, () => ListaHistorial.Any());
            EliminarTodoHistorialCommand = _eliminarCommand;
        }

        public void AgregarHistorial(Historial historial)
        {
            _historialService.AgregarHistorial(historial);

        }

        private void EliminarTodoHistorial()
        {
            var resultado = MessageBox.Show("¿Estás seguro que deseas eliminar todo el historial?", "Confirmar eliminación", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (resultado == MessageBoxResult.Yes)
            {
                _historialService.EliminarTodoElHistorial();
                ListaHistorial.Clear();
            }
        }


    }
}

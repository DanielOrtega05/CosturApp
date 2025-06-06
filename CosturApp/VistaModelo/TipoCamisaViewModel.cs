using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using CosturApp.Modelo;
using CosturApp.Servicio;
using System.Windows.Input;
using System.Windows;
using CosturApp.Vista.VentanasSecundarias;

namespace CosturApp.VistaModelo
{
    public class TipoCamisaViewModel : INotifyPropertyChanged
    {
        private TipoCamisaService _servicio;
        private RelayCommand _eliminarTipoCommand;
        private RelayCommand _agregarTipoCommand;

        private TipoCamisa _tipoCamisaSeleccionado;
        private HistorialService _historialService = new HistorialService();
        public ObservableCollection<TipoCamisa> ListaTiposCamisa { get; set; }

        public TipoCamisa TipoCamisaSeleccionado
        {
            get => _tipoCamisaSeleccionado;
            set
            {
                _tipoCamisaSeleccionado = value;
                OnPropertyChanged();
                _eliminarTipoCommand.RaiseCanExecuteChanged();
            }
        }

        public ICommand AgregarTipoCamisaCommand => _agregarTipoCommand;
        public ICommand EliminarTipoCamisaCommand => _eliminarTipoCommand;

        public TipoCamisaViewModel()
        {
            _servicio = new TipoCamisaService();

            ListaTiposCamisa = new ObservableCollection<TipoCamisa>(_servicio.ObtenerTodos());

            _agregarTipoCommand = new RelayCommand(AgregarTipoCamisa);
            _eliminarTipoCommand = new RelayCommand(EliminarTipoCamisa, () => TipoCamisaSeleccionado != null);
        }

        private void AgregarTipoCamisa()
        {
            var ventana = new TipoCamisaCrearWindow(); // Asegúrate de tener esta ventana implementada

            if (ventana.ShowDialog() == true)
            {
                string nombre = ventana.NombreIngresado?.Trim();
                if (!string.IsNullOrWhiteSpace(nombre))
                {
                    _servicio.AgregarSiNoExiste(nombre);
                    RecargarLista();

                    _historialService.AgregarHistorial(new Historial
                    {
                        Titulo = "Tipo de camisa agregado",
                        Descripcion = $"Se agregó el tipo de camisa '{nombre}'.",
                        FechaHistorial = DateTime.Now
                    });

                    MessageBox.Show($"Tipo de camisa '{nombre}' agregado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void EliminarTipoCamisa()
        {
            if (TipoCamisaSeleccionado == null)
                return;

            var result = MessageBox.Show(
                $"¿Deseas eliminar el tipo de camisa '{TipoCamisaSeleccionado.Nombre}'?",
                "Confirmación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning
            );

            if (result == MessageBoxResult.Yes)
            {
                string nombreEliminado = TipoCamisaSeleccionado.Nombre;

                _servicio.EliminarPorId(TipoCamisaSeleccionado.Id);
                ListaTiposCamisa.Remove(TipoCamisaSeleccionado);

                _historialService.AgregarHistorial(new Historial
                {
                    Titulo = "Tipo de camisa eliminado",
                    Descripcion = $"Se eliminó el tipo de camisa '{nombreEliminado}'.",
                    FechaHistorial = DateTime.Now
                });

                MessageBox.Show("Tipo de camisa eliminado correctamente.", "Eliminado", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        public void RecargarLista()
        {
            ListaTiposCamisa.Clear();
            foreach (var tipo in _servicio.ObtenerTodos())
                ListaTiposCamisa.Add(tipo);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string nombre = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nombre));
        }
    }
}

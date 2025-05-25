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
using CosturApp.Vista.VentanasSecundarias;
using System.Windows;

namespace CosturApp.VistaModelo
{
    public class AnexoViewModel : INotifyPropertyChanged
    {
        private Anexo _anexoSeleccionado;
        private RelayCommand _editarAnexoCommand;
        private RelayCommand _eliminarAnexoCommand;
        private AnexoService _servicio;
        private HistorialService _historialService = new HistorialService();
        public ObservableCollection<Anexo> ListaAnexos { get; set; }

        public Anexo AnexoSeleccionado
        {
            get => _anexoSeleccionado;
            set
            {
                _anexoSeleccionado = value;
                OnPropertyChanged();
                _editarAnexoCommand.RaiseCanExecuteChanged(); // Comprobar los cambios 
                _eliminarAnexoCommand.RaiseCanExecuteChanged();
            }
        }

        // Se les llama en los bindings
        public ICommand CrearAnexoCommand { get; }
        public ICommand EditarAnexoCommand => _editarAnexoCommand;
        public ICommand EliminarAnexoCommand => _eliminarAnexoCommand;

        public AnexoViewModel()
        {
            // inicio bd anexo
            _servicio = new AnexoService();
            ListaAnexos = new ObservableCollection<Anexo>(_servicio.ObtenerAnexos());

            CrearAnexoCommand = new RelayCommand(CrearAnexo);
            _editarAnexoCommand = new RelayCommand(EditarAnexo, () => AnexoSeleccionado != null);
            _eliminarAnexoCommand = new RelayCommand(EliminarAnexo, () => AnexoSeleccionado != null);
        }

        private void CrearAnexo()
        {
            var ventana = new AnexoCrearWindow();

            if (ventana.ShowDialog() == true)
            {
                var nuevo = new Anexo
                {
                    Titulo = ventana.TituloIngresado,
                    FechaCreacion = DateTime.Now
                };

                var anexoGuardado = _servicio.AgregarAnexo(nuevo);
                ListaAnexos.Add(anexoGuardado);

                _historialService.AgregarHistorial(new Historial
                {
                    Titulo = "Anexo creado",
                    Descripcion = $"Se creó el anexo '{nuevo.Titulo}'.",
                    FechaHistorial = DateTime.Now
                });
            }
        }


        private void EditarAnexo()
        {
            if (AnexoSeleccionado != null)
            {
                string tituloAntes = AnexoSeleccionado.Titulo;

                var ventana = new AnexoDetalleWindow(AnexoSeleccionado);
                ventana.ShowDialog();

                if (tituloAntes != AnexoSeleccionado.Titulo)
                {
                    _historialService.AgregarHistorial(new Historial
                    {
                        Titulo = "Anexo editado",
                        Descripcion = $"Se cambió el título del anexo:\n- Antes: '{tituloAntes}'\n- Después: '{AnexoSeleccionado.Titulo}'",
                        FechaHistorial = DateTime.Now
                    });
                }
            }
        }

        private void EliminarAnexo()
        {

            if (AnexoSeleccionado != null)
            {
                var resultado = MessageBox.Show("¿Estás seguro de que deseas eliminar este elemento?",
                                "Advertencia",
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Warning);
                if (resultado == MessageBoxResult.Yes)
                {
                    string tituloEliminado = AnexoSeleccionado.Titulo;

                    MessageBox.Show("Se ha eliminado el Anexo con titulo: " + AnexoSeleccionado.Titulo, "Eliminado Exitosamente", MessageBoxButton.OK, MessageBoxImage.Information);

                    _servicio.EliminarAnexo(AnexoSeleccionado.Id);

                    ListaAnexos.Remove(AnexoSeleccionado);

                    _historialService.AgregarHistorial(new Historial
                    {
                        Titulo = "Anexo eliminado",
                        Descripcion = $"Se eliminó el anexo '{tituloEliminado}'.",
                        FechaHistorial = DateTime.Now
                    });

                }
            }
        }

        // Notifica los cambios de una propiedad a la vista xaml 

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}

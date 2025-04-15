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

        public ObservableCollection<Anexo> ListaAnexos { get; set; }

        public Anexo AnexoSeleccionado
        {
            get => _anexoSeleccionado;
            set
            {
                _anexoSeleccionado = value;
                OnPropertyChanged();
                _editarAnexoCommand.RaiseCanExecuteChanged();
                _eliminarAnexoCommand.RaiseCanExecuteChanged();
            }
        }

        // Se les llama en los bindings
        public ICommand CrearAnexoCommand { get; }
        public ICommand EditarAnexoCommand => _editarAnexoCommand;
        public ICommand EliminarAnexoCommand => _eliminarAnexoCommand;

        public AnexoViewModel()
        {
            ListaAnexos = new ObservableCollection<Anexo>();
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

                ListaAnexos.Add(nuevo);
                AnexoSeleccionado = nuevo;
            }
        }


        private void EditarAnexo()
        {
            if (AnexoSeleccionado != null)
            {
                // Por ahora no hace nada
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
                    MessageBox.Show("Se ha eliminado el Anexo con titulo: " + AnexoSeleccionado.Titulo, "Eliminado Exitosamente", MessageBoxButton.OK, MessageBoxImage.Information);
                    ListaAnexos.Remove(AnexoSeleccionado);
                    AnexoSeleccionado = null;
                    
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

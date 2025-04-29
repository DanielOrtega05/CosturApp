using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using CosturApp.Modelo;
using CosturApp.Servicio;
using CosturApp.Vista.VentanasSecundarias;
using MaterialDesignColors;

namespace CosturApp.VistaModelo
{
    public class AnexoDetalleViewModel : INotifyPropertyChanged
    {
        private Anexo _anexo;
        private ObservableCollection<Orden> _ordenes;
        private Orden _ordenSeleccionada;

        private RelayCommand _agregarOrdenCommand;
        private RelayCommand _editarOrdenCommand;
        private RelayCommand _eliminarOrdenCommand;
        private RelayCommand _editarTituloAnexoCommand;

        private OrdenService _ordenService;

        public AnexoDetalleViewModel(Anexo anexo)
        {
            _anexo = anexo;
            _ordenService = new OrdenService();
            Ordenes = new ObservableCollection<Orden>(_ordenService.ObtenerOrdenesPorAnexo(anexo.Id));

            _agregarOrdenCommand = new RelayCommand(AgregarOrden);
            _editarOrdenCommand = new RelayCommand(EditarOrden, () => OrdenSeleccionada != null);
            _eliminarOrdenCommand = new RelayCommand(EliminarOrden, () => OrdenSeleccionada != null);
            _editarTituloAnexoCommand = new RelayCommand(EditarTituloAnexo);
        }

        public Anexo Anexo => _anexo;

        // Solo notifica si se añade o elimina objeto, no si se edita
        public ObservableCollection<Orden> Ordenes
        {
            get => _ordenes;
            set
            {
                _ordenes = value;
                OnPropertyChanged();
            }
        }

        public Orden OrdenSeleccionada
        {
            get => _ordenSeleccionada;
            set
            {
                _ordenSeleccionada = value;
                OnPropertyChanged();
                _editarOrdenCommand.RaiseCanExecuteChanged();
                _eliminarOrdenCommand.RaiseCanExecuteChanged();
            }
        }

        public ICommand EditarTituloAnexoCommand => _editarTituloAnexoCommand;
        public ICommand AgregarOrdenCommand => _agregarOrdenCommand;
        public ICommand EditarOrdenCommand => _editarOrdenCommand;
        public ICommand EliminarOrdenCommand => _eliminarOrdenCommand;

        private void AgregarOrden()
        {
            var ventana = new OrdenCrearWindow();

            if (ventana.ShowDialog() == true)
            {
                var nuevaOrden = new Orden
                {
                    NumeroOrden = ventana.txbNumeroOrden.Text,
                    TotalCamisetas = int.TryParse(ventana.txbCantidad.Text, out var cantidad) ? cantidad : 0,
                    TipoCamisa = ventana.cmbTipoCamisa.Text,
                    AnexoId = _anexo.Id
                };

                _ordenService.AgregarOrden(nuevaOrden);
                Ordenes.Add(nuevaOrden);
            }

        }

        private void EditarTituloAnexo()
        {
            var ventana = new AnexoCrearWindow();

            ventana.TituloTextBox.Text = _anexo.Titulo;

            if (ventana.ShowDialog() == true)
            {
                string nuevoTitulo = ventana.TituloIngresado;

                if (!string.IsNullOrWhiteSpace(nuevoTitulo) && nuevoTitulo != _anexo.Titulo)
                {
                    _anexo.Titulo = nuevoTitulo;

                    var anexoService = new AnexoService();
                    anexoService.EditarTituloAnexo(Anexo);

                    OnPropertyChanged(nameof(Anexo));
                }
            }
        }


        private void EditarOrden()
        {
            if (OrdenSeleccionada != null)
            {
                var ventana = new OrdenCrearWindow(OrdenSeleccionada); // Pasamos la orden seleccionada a la ventana

                if (ventana.ShowDialog() == true)
                {
                    // Si la ventana devuelve true, se ha actualizado la orden
                    _ordenService.EditarOrden(OrdenSeleccionada); // Actualizamos la orden en la base de datos
                }
            }
        }

        private void EliminarOrden()
        {
            if (OrdenSeleccionada != null)
            {
                var confirmar = MessageBox.Show("¿Estás seguro de que deseas eliminar la orden " + OrdenSeleccionada.NumeroOrden + "?",
                                "Advertencia",
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Warning);
                if (confirmar == MessageBoxResult.Yes)
                {
                    _ordenService.EliminarOrden(OrdenSeleccionada.Id);
                    Ordenes.Remove(OrdenSeleccionada);
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string nombre = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nombre));
        }
    }
}

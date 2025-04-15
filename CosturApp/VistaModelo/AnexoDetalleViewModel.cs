using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using CosturApp.Modelo;
using CosturApp.Servicio;

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

        private OrdenService _ordenService;

        public AnexoDetalleViewModel(Anexo anexo)
        {
            _anexo = anexo;
            _ordenService = new OrdenService();
            Ordenes = new ObservableCollection<Orden>(_ordenService.ObtenerOrdenesPorAnexo(anexo.Id));

            _agregarOrdenCommand = new RelayCommand(AgregarOrden);
            _editarOrdenCommand = new RelayCommand(EditarOrden, () => OrdenSeleccionada != null);
            _eliminarOrdenCommand = new RelayCommand(EliminarOrden, () => OrdenSeleccionada != null);
        }

        public Anexo Anexo => _anexo;

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

        public ICommand AgregarOrdenCommand => _agregarOrdenCommand;
        public ICommand EditarOrdenCommand => _editarOrdenCommand;
        public ICommand EliminarOrdenCommand => _eliminarOrdenCommand;

        private void AgregarOrden()
        {
            // Codigo ejemplo
            var nueva = new Orden
            {
                AnexoId = _anexo.Id,
                NumeroOrden = "Nuevo",
                TotalCamisetas = 0
            };

            _ordenService.AgregarOrden(nueva);
            Ordenes.Add(nueva);
        }

        private void EditarOrden()
        {
            if (OrdenSeleccionada != null)
            {
                // Codigo Ejemplo
                MessageBox.Show($"Editar orden: {OrdenSeleccionada.NumeroOrden}", "Editar", MessageBoxButton.OK);
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

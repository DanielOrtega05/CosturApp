using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
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
        private HistorialService _historialService = new HistorialService();

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
                if (_ordenes != null)
                    _ordenes.CollectionChanged -= Ordenes_CollectionChanged;

                _ordenes = value;
                OnPropertyChanged();

                if (_ordenes != null)
                    _ordenes.CollectionChanged += Ordenes_CollectionChanged;

                OnPropertyChanged(nameof(TotalCamisetasMes));

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

        public int TotalCamisetasMes => Ordenes?.Sum(o => o.TotalCamisetas) ?? 0;

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

                _historialService.AgregarHistorial(new Historial
                {
                    Titulo = "Orden creada",
                    Descripcion = $"Se creó la orden {nuevaOrden.NumeroOrden} con {nuevaOrden.TotalCamisetas} camisetas de tipo {nuevaOrden.TipoCamisa}.",
                    FechaHistorial = DateTime.Now
                });

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
                var ordenAntes = new Orden
                {
                    NumeroOrden = OrdenSeleccionada.NumeroOrden,
                    TotalCamisetas = OrdenSeleccionada.TotalCamisetas,
                    TipoCamisa = OrdenSeleccionada.TipoCamisa
                };

                var ventana = new OrdenCrearWindow(OrdenSeleccionada); // Pasamos la orden seleccionada a la ventana

                if (ventana.ShowDialog() == true)
                {
                    // Si la ventana devuelve true, se ha actualizado la orden
                    _ordenService.EditarOrden(OrdenSeleccionada); // Actualizamos la orden en la base de datos

                    _historialService.AgregarHistorial(new Historial
                    {
                        Titulo = "Orden editada",
                        Descripcion = $"Orden '{ordenAntes.NumeroOrden}' del anexo '{_anexo.Titulo}' fue editada:\n" +
                      $"- Número de orden: {ordenAntes.NumeroOrden} → {OrdenSeleccionada.NumeroOrden}\n" +
                      $"- Total camisetas: {ordenAntes.TotalCamisetas} → {OrdenSeleccionada.TotalCamisetas}\n" +
                      $"- Tipo camisa: {ordenAntes.TipoCamisa} → {OrdenSeleccionada.TipoCamisa}",
                        FechaHistorial = DateTime.Now
                    });

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
                    string numeroOrdenEliminada = OrdenSeleccionada.NumeroOrden;
                    _ordenService.EliminarOrden(OrdenSeleccionada.Id);
                    Ordenes.Remove(OrdenSeleccionada);

                    _historialService.AgregarHistorial(new Historial
                    {
                        Titulo = "Orden eliminada",
                        Descripcion = $"Se eliminó la orden {numeroOrdenEliminada}.",
                        FechaHistorial = DateTime.Now
                    });

                }
            }
        }

        // Se suscribe a cada objeto orden dentro de la coleccion para saber si se ha modificado alguna orden. Es decir que cada vez que hay un cambio
        // este metodo se ejecutara.
        private void Orden_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Si la propiedad que ha cambiado en la orden es total camisetas, entonces total camisetas mes se actualiza con el nuevo valor
            if (e.PropertyName == nameof(Orden.TotalCamisetas))
            {
                OnPropertyChanged(nameof(TotalCamisetasMes));
            }
        }

        private void Ordenes_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // Si se elimina una orden tambien se actualiza el total de camisetas del mes
            if (e.OldItems != null)
            {
                foreach (Orden oldOrden in e.OldItems)
                {
                    oldOrden.PropertyChanged -= Orden_PropertyChanged;
                }
            }

            // Al igual que si se añade una orden nueva
            if (e.NewItems != null)
            {
                foreach (Orden newOrden in e.NewItems)
                {
                    newOrden.PropertyChanged += Orden_PropertyChanged;
                }
            }

            OnPropertyChanged(nameof(TotalCamisetasMes));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string nombre = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nombre));
        }
    }
}

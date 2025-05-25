using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using CosturApp.Modelo;
using CosturApp.Servicio;
using CosturApp.Vista.VentanasSecundarias;
using iTextSharp.text.pdf;
using iTextSharp.text;
using MaterialDesignColors;
using Microsoft.Win32;

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
        private RelayCommand _exportarPdfCommand;

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

            _exportarPdfCommand = new RelayCommand(ExportarPdf);
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
        public ICommand ExportarPdfCommand => _exportarPdfCommand;

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

        private void ExportarPdf()
        {
            // Esto abre una ventana para que el usuario elija donde guardar el PDF
            SaveFileDialog saveDialog = new SaveFileDialog
            {
                Filter = "PDF files (*.pdf)|*.pdf", // Solo deja guardar archivos .pdf
                FileName = $"{_anexo.Titulo}_{DateTime.Now:yyyyMMdd}.pdf" // Nombre por defecto con titulo y fecha
            };

            // Si el usuario cancela, no hacemos nada y salimos
            if (saveDialog.ShowDialog() != true)
                return;

            try
            {
                // Aqui se crea el archivo para escribir el PDF
                using (FileStream fs = new FileStream(saveDialog.FileName, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    // Creamos el documento con tamaño A4 y margenes
                    Document doc = new Document(PageSize.A4, 40, 40, 40, 40);
                    PdfWriter writer = PdfWriter.GetInstance(doc, fs);
                    doc.Open(); // Abrimos el documento para agregar cosas

                    // Defino dos fuentes, una para titulo grande y azul y otra normal para texto normal
                    var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16, BaseColor.Blue);
                    var normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 12);

                    // Aqui pongo el titulo del PDF centrado y con un poco de espacio abajo
                    Paragraph title = new Paragraph($"Anexo: {_anexo.Titulo}", titleFont);
                    title.Alignment = Element.ALIGN_CENTER;
                    title.SpacingAfter = 15;
                    doc.Add(title);

                    // Agrego la fecha de creacion del anexo
                    Paragraph fecha = new Paragraph($"Fecha de creacion: {_anexo.FechaCreacion:dd/MM/yyyy}", normalFont);
                    fecha.SpacingAfter = 10;
                    doc.Add(fecha);

                    // Ahora agrupo las ordenes por tipo de camisa para listarlas juntas
                    var grupos = _ordenes
                        .GroupBy(o => o.TipoCamisa)
                        .OrderBy(g => g.Key); // Ordeno alfabeticamente los tipos

                    foreach (var grupo in grupos)
                    {
                        // Pongo un subtitulo con el tipo de camisa
                        Paragraph subtitulo = new Paragraph($"Tipo: {grupo.Key}", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14, BaseColor.Black));
                        subtitulo.SpacingBefore = 15;
                        subtitulo.SpacingAfter = 10;
                        doc.Add(subtitulo);

                        // Creo una tabla de 2 columnas, una para el numero de orden y otra para camisetas
                        PdfPTable tabla = new PdfPTable(2);
                        tabla.WidthPercentage = 100;
                        tabla.SetWidths(new float[] { 2, 1 }); // columnas con diferente tamaño

                        // Agrego los encabezados de la tabla
                        tabla.AddCell(Celda("Numero de Orden", true));
                        tabla.AddCell(Celda("Cantidades", true));

                        // Ordeno las ordenes dentro del grupo de esta manera:
                        // primero las que tienen numero, luego por numero ascendente, y si no tienen numero por nombre alfabetico
                        var ordenesOrdenadas = grupo
                            .Select(o => new
                            {
                                Orden = o,
                                NumeroExtraido = ExtraerNumero(o.NumeroOrden)
                            })
                            .OrderBy(x => x.NumeroExtraido.HasValue) // los que tienen numero primero
                            .ThenBy(x => x.NumeroExtraido)           // por numero ascendente
                            .ThenBy(x => x.Orden.NumeroOrden)        // por nombre alfabetico si no hay numero
                            .Select(x => x.Orden);

                        // Aqui agrego cada orden en la tabla con su numero y cantidad de camisetas
                        foreach (var orden in ordenesOrdenadas)
                        {
                            tabla.AddCell(Celda(orden.NumeroOrden));
                            tabla.AddCell(Celda(orden.TotalCamisetas.ToString()));
                        }

                        // Al final de la tabla pongo un subtotal con la suma de camisetas de ese grupo
                        PdfPCell subtotalCell = new PdfPCell(new Phrase($"Subtotal: {grupo.Sum(o => o.TotalCamisetas)} camisetas", normalFont));
                        subtotalCell.Colspan = 2; // ocupa dos columnas
                        subtotalCell.HorizontalAlignment = Element.ALIGN_RIGHT; // alineado a la derecha
                        subtotalCell.PaddingTop = 8;
                        subtotalCell.Border = Rectangle.TOP_BORDER; // separacion arriba
                        tabla.AddCell(subtotalCell);

                        doc.Add(tabla); // agrego la tabla al documento
                    }

                    // Al final pongo un total general con todas las camisetas sumadas
                    Paragraph totalGeneral = new Paragraph($"Total general: {_ordenes.Sum(o => o.TotalCamisetas)} camisetas", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12));
                    totalGeneral.Alignment = Element.ALIGN_RIGHT;
                    totalGeneral.SpacingBefore = 15;
                    doc.Add(totalGeneral);

                    // Agrego un pie de pagina con info de la app y fecha/hora actual
                    Paragraph footer = new Paragraph($"Generado por CosturApp - {DateTime.Now:dd/MM/yyyy HH:mm}", FontFactory.GetFont(FontFactory.HELVETICA_OBLIQUE, 10, BaseColor.Gray));
                    footer.SpacingBefore = 20;
                    footer.Alignment = Element.ALIGN_CENTER;
                    doc.Add(footer);

                    doc.Close(); // cierro el doc para guardar todo
                    writer.Close();
                }

                MessageBox.Show("PDF generado correctamente", "Exito", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al generar el PDF: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            // Metodo auxiliar para crear celdas con texto, si es encabezado les pone estilo especial
            PdfPCell Celda(string texto, bool esEncabezado = false)
            {
                var font = esEncabezado ? FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12) : FontFactory.GetFont(FontFactory.HELVETICA, 12);
                var cell = new PdfPCell(new Phrase(texto, font));
                cell.Padding = 5;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                if (esEncabezado)
                {
                    cell.BackgroundColor = BaseColor.LightGray; // fondo gris para encabezados
                }
                return cell;
            }
        }

        private int? ExtraerNumero(string texto)
        {
            // Busco el primer numero que encuentre en el texto y lo devuelvo como int
            var match = System.Text.RegularExpressions.Regex.Match(texto, @"\d+");
            if (match.Success && int.TryParse(match.Value, out int numero))
                return numero;

            // Si no encuentra numero devuelve null
            return null;
        }


    }
}

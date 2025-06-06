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
    // Esta clase se encarga de manejar la logica y datos para la vista detalle del anexo
    public class AnexoDetalleViewModel : INotifyPropertyChanged
    {
        private Anexo _anexo;
        private ObservableCollection<Orden> _ordenes;
        private Orden _ordenSeleccionada;

        // Comandos que se usan para enlazar con botones y asi ejecutar una serie de funciones
        private RelayCommand _agregarOrdenCommand;
        private RelayCommand _editarOrdenCommand;
        private RelayCommand _eliminarOrdenCommand;
        private RelayCommand _editarTituloAnexoCommand;
        private RelayCommand _exportarPdfCommand;

        private OrdenService _ordenService;
        private TipoCamisaService _tipoCamisaService;
        private HistorialService _historialService = new HistorialService();

        public AnexoDetalleViewModel(Anexo anexo)
        {
            _anexo = anexo;
            _ordenService = new OrdenService();
            _tipoCamisaService = new TipoCamisaService();

            // Aqui se obtienen las ordenes del anexo y se guardan en una coleccion observable
            var ordenes = _ordenService.ObtenerOrdenesPorAnexo(anexo.Id);
            foreach (var orden in ordenes)
            {
                orden.TipoCamisa = _tipoCamisaService.ObtenerPorId(orden.TipoCamisaId);
            }

            Ordenes = new ObservableCollection<Orden>(ordenes);

            // Se crean los comandos asignandoles las funciones que se ejecutan al activarlos
            _agregarOrdenCommand = new RelayCommand(AgregarOrden);
            _editarOrdenCommand = new RelayCommand(EditarOrden, () => OrdenSeleccionada != null);
            _eliminarOrdenCommand = new RelayCommand(EliminarOrden, () => OrdenSeleccionada != null);
            _editarTituloAnexoCommand = new RelayCommand(EditarTituloAnexo);

            _exportarPdfCommand = new RelayCommand(ExportarPdf);
        }

        public Anexo Anexo => _anexo;

        // Propiedad que devuelve o asigna la coleccion de ordenes
        // Se suscribe y desuscribe de eventos para controlar cambios en la coleccion
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

                // Cuando cambia la coleccion se actualiza el total de camisetas del mes
                OnPropertyChanged(nameof(TotalCamisetasMes));
            }
        }

        // Propiedad que guarda la orden que esta seleccionada en la UI
        public Orden OrdenSeleccionada
        {
            get => _ordenSeleccionada;
            set
            {
                _ordenSeleccionada = value;
                OnPropertyChanged();
                // Cuando cambia la orden seleccionada, se activan o desactivan los botones editar y eliminar
                _editarOrdenCommand.RaiseCanExecuteChanged();
                _eliminarOrdenCommand.RaiseCanExecuteChanged();
            }
        }

        // Propiedades publicas que exponen los comandos para la UI
        public ICommand EditarTituloAnexoCommand => _editarTituloAnexoCommand;
        public ICommand AgregarOrdenCommand => _agregarOrdenCommand;
        public ICommand EditarOrdenCommand => _editarOrdenCommand;
        public ICommand EliminarOrdenCommand => _eliminarOrdenCommand;
        public ICommand ExportarPdfCommand => _exportarPdfCommand;

        // Calcula el total de camisetas sumando todas las ordenes
        public int TotalCamisetasMes => Ordenes?.Sum(o => o.TotalCamisetas) ?? 0;

        // Metodo para agregar una nueva orden
        private void AgregarOrden()
        {
            var ventana = new OrdenCrearWindow();

            // Si se confirma la ventana, se crea la orden y se añade a la coleccion y a la BD
            if (ventana.ShowDialog() == true)
            {
                string numeroOrden = ventana.txbNumeroOrden.Text.Trim();
                int tipoCamisaId = ((TipoCamisa)ventana.cmbTipoCamisa.SelectedItem)?.Id ?? 0;

                // Validar si ya existe una orden con el mismo numero y tipo de camisa
                string numeroOrdenNormalizado = NormalizarNumeroOrden(numeroOrden);

                bool ordenExistente = Ordenes.Any(o =>
                    NormalizarNumeroOrden(o.NumeroOrden) == numeroOrdenNormalizado &&
                    o.TipoCamisaId == tipoCamisaId);

                if (ordenExistente)
                {
                    MessageBox.Show(
                        $"Ya existe una orden con el número '{numeroOrden}' para este tipo de camisa.",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }

                var nuevaOrden = new Orden
                {
                    NumeroOrden = ventana.txbNumeroOrden.Text,
                    TotalCamisetas = int.TryParse(ventana.txbCantidad.Text, out var cantidad) ? cantidad : 0,
                    TipoCamisaId = ((TipoCamisa)ventana.cmbTipoCamisa.SelectedItem)?.Id ?? 0,
                    AnexoId = _anexo.Id
                };

                nuevaOrden.TipoCamisa = _tipoCamisaService.ObtenerPorId(tipoCamisaId);

                _ordenService.AgregarOrden(nuevaOrden);
                Ordenes.Add(nuevaOrden);

                // Se añade registro al historial de cambios
                _historialService.AgregarHistorial(new Historial
                {
                    Titulo = $"Orden creada en el anexo {_anexo.Titulo}",
                    Descripcion = $"Se creo la orden {nuevaOrden.NumeroOrden} con {nuevaOrden.TotalCamisetas} camisetas de tipo {nuevaOrden.TipoCamisa.Nombre}.",
                    FechaHistorial = DateTime.Now
                });

            }

        }

        // Metodo para editar el titulo del anexo
        private void EditarTituloAnexo()
        {
            var ventana = new AnexoCrearWindow();

            ventana.TituloTextBox.Text = _anexo.Titulo;

            if (ventana.ShowDialog() == true)
            {
                string nuevoTitulo = ventana.TituloIngresado;

                // Si el titulo no esta vacio y es diferente al actual se actualiza
                if (!string.IsNullOrWhiteSpace(nuevoTitulo) && nuevoTitulo != _anexo.Titulo)
                {
                    _anexo.Titulo = nuevoTitulo;

                    var anexoService = new AnexoService();
                    anexoService.EditarTituloAnexo(Anexo);

                    // Notifica que el anexo cambio para que se refresque la UI
                    OnPropertyChanged(nameof(Anexo));
                }
            }
        }


        // Metodo para editar la orden seleccionada
        private void EditarOrden()
        {
            if (OrdenSeleccionada != null)
            {
                // Se guarda la orden antes de editar para comparar luego
                var ordenAntes = new Orden
                {
                    NumeroOrden = OrdenSeleccionada.NumeroOrden,
                    TotalCamisetas = OrdenSeleccionada.TotalCamisetas,
                    TipoCamisaId = OrdenSeleccionada.TipoCamisaId

                };

                var ventana = new OrdenCrearWindow(OrdenSeleccionada);

                if (ventana.ShowDialog() == true)
                {
                    string nuevoNumero = ventana.txbNumeroOrden.Text.Trim();
                    int nuevoTipoId = ((TipoCamisa)ventana.cmbTipoCamisa.SelectedItem)?.Id ?? 0;
                    int nuevaCantidad = int.TryParse(ventana.txbCantidad.Text, out var cantidad) ? cantidad : 0;

                    string numeroNormalizado = NormalizarNumeroOrden(nuevoNumero);

                    // Validar si ya existe otra orden (excluyendo la actual) con el mismo numero y tipo
                    bool ordenExistente = Ordenes.Any(o =>
                        o.Id != OrdenSeleccionada.Id &&
                        NormalizarNumeroOrden(o.NumeroOrden) == numeroNormalizado &&
                        o.TipoCamisaId == nuevoTipoId);

                    if (ordenExistente)
                    {
                        MessageBox.Show(
                            $"Ya existe otra orden con el número '{nuevoNumero}' para este tipo de camisa.",
                            "Error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);

                        // Revertir los cambios visuales en la orden seleccionada
                        OrdenSeleccionada.NumeroOrden = ordenAntes.NumeroOrden;
                        OrdenSeleccionada.TotalCamisetas = ordenAntes.TotalCamisetas;
                        OrdenSeleccionada.TipoCamisaId = ordenAntes.TipoCamisaId;
                        OrdenSeleccionada.TipoCamisa = _tipoCamisaService.ObtenerPorId(ordenAntes.TipoCamisaId);

                        return;
                    }

                    // Aplicar los nuevos valores solo si pasa la validacion
                    OrdenSeleccionada.NumeroOrden = nuevoNumero;
                    OrdenSeleccionada.TotalCamisetas = nuevaCantidad;
                    OrdenSeleccionada.TipoCamisaId = nuevoTipoId;
                    OrdenSeleccionada.TipoCamisa = _tipoCamisaService.ObtenerPorId(nuevoTipoId);

                    // Si se confirma, se actualiza la orden en la BD
                    _ordenService.EditarOrden(OrdenSeleccionada);

                    // Se obtiene el nombre del tipo de camisa antes y despues de editar para el historial
                    TipoCamisaService tipoCamisaService = new TipoCamisaService();
                    var tipoAntes = tipoCamisaService.ObtenerPorId(ordenAntes.TipoCamisaId)?.Nombre ?? "Desconocido";
                    var tipoDespues = tipoCamisaService.ObtenerPorId(OrdenSeleccionada.TipoCamisaId)?.Nombre ?? "Desconocido";

                    // Se añade registro en el historial con los cambios realizados
                    _historialService.AgregarHistorial(new Historial
                    {
                        Titulo = "Orden editada",
                        Descripcion = $"Orden '{ordenAntes.NumeroOrden}' del anexo '{_anexo.Titulo}' fue editada:\n" +
                      $"- Numero de orden: {ordenAntes.NumeroOrden} -> {OrdenSeleccionada.NumeroOrden}\n" +
                      $"- Total camisetas: {ordenAntes.TotalCamisetas} -> {OrdenSeleccionada.TotalCamisetas}\n" +
                      $"- Tipo camisa: {tipoAntes} -> {tipoDespues}",
                        FechaHistorial = DateTime.Now
                    });

                }
            }
        }

        // Metodo para eliminar la orden seleccionada
        private void EliminarOrden()
        {
            if (OrdenSeleccionada != null)
            {
                // Se muestra mensaje para confirmar eliminacion
                var confirmar = MessageBox.Show("¿Estas seguro de que deseas eliminar la orden " + OrdenSeleccionada.NumeroOrden + "?",
                                "Advertencia",
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Warning);
                if (confirmar == MessageBoxResult.Yes)
                {
                    string numeroOrdenEliminada = OrdenSeleccionada.NumeroOrden;
                    int numeroCamisasEliminada = OrdenSeleccionada.TotalCamisetas;
                    TipoCamisa tipoCamisaEliminada = _tipoCamisaService.ObtenerPorId(OrdenSeleccionada.TipoCamisaId);
                    _ordenService.EliminarOrden(OrdenSeleccionada.Id);
                    Ordenes.Remove(OrdenSeleccionada);

                    // Se añade registro al historial de eliminacion
                    _historialService.AgregarHistorial(new Historial
                    {
                        Titulo = $"Orden eliminada del anexo {_anexo.Titulo}",
                        Descripcion = $"Se elimino la orden {numeroOrdenEliminada}, contenia {numeroCamisasEliminada} camisetas, del tipo {tipoCamisaEliminada.Nombre}.",
                        FechaHistorial = DateTime.Now
                    });

                }
            }
        }

        // Este metodo se llama cuando cambia alguna propiedad de una orden individual en la coleccion
        private void Orden_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Si cambio el total de camisetas, se actualiza el total general
            if (e.PropertyName == nameof(Orden.TotalCamisetas))
            {
                OnPropertyChanged(nameof(TotalCamisetasMes));
            }
        }

        // Este metodo se ejecuta cuando se añade o elimina una orden en la coleccion
        private void Ordenes_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // Si se quita alguna orden, se desuscribe de su evento PropertyChanged
            if (e.OldItems != null)
            {
                foreach (Orden oldOrden in e.OldItems)
                {
                    oldOrden.PropertyChanged -= Orden_PropertyChanged;
                }
            }

            // Si se añade alguna orden, se suscribe al evento PropertyChanged para detectar cambios
            if (e.NewItems != null)
            {
                foreach (Orden newOrden in e.NewItems)
                {
                    newOrden.PropertyChanged += Orden_PropertyChanged;
                }
            }

            // Cuando cambia la coleccion, actualiza el total de camisetas
            OnPropertyChanged(nameof(TotalCamisetasMes));
        }

        // Evento que notifica cuando una propiedad cambia para actualizar la interfaz
        public event PropertyChangedEventHandler PropertyChanged;

        // Metodo que lanza el evento PropertyChanged
        protected void OnPropertyChanged([CallerMemberName] string nombre = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nombre));
        }

        // Metodo para exportar el contenido a PDF
        private void ExportarPdf()
        {
            // Se abre un dialogo para que el usuario elija donde guardar el archivo PDF
            SaveFileDialog saveDialog = new SaveFileDialog
            {
                Filter = "PDF files (*.pdf)|*.pdf",
                FileName = $"{_anexo.Titulo}_{DateTime.Now:yyyyMMdd}.pdf"
            };

            if (saveDialog.ShowDialog() != true)
                return;

            try
            {
                using (FileStream fs = new FileStream(saveDialog.FileName, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    Document doc = new Document(PageSize.A4, 40, 40, 40, 40);
                    PdfWriter writer = PdfWriter.GetInstance(doc, fs);
                    doc.Open();

                    // Se definen fuentes para usar en el PDF
                    var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16, BaseColor.Blue);
                    var normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 12);

                    // Se añade titulo centrado al PDF
                    Paragraph title = new Paragraph($"Anexo: {_anexo.Titulo}", titleFont);
                    title.Alignment = Element.ALIGN_CENTER;
                    title.SpacingAfter = 15;
                    doc.Add(title);

                    // Se añade la fecha de creacion del anexo
                    Paragraph fecha = new Paragraph($"Fecha de creacion: {_anexo.FechaCreacion:dd/MM/yyyy}", normalFont);
                    fecha.SpacingAfter = 10;
                    doc.Add(fecha);

                    // Se obtienen todos los tipos de camisa para agrupar las ordenes por tipo
                    TipoCamisaService tipoCamisaService = new TipoCamisaService();
                    var tiposCamisa = tipoCamisaService.ObtenerTodos();

                    // Se agrupan las ordenes por TipoCamisaId y se ordenan alfabeticamente
                    var grupos = _ordenes
                        .GroupBy(o => o.TipoCamisaId)
                        .Select(g => new
                        {
                            TipoCamisa = tiposCamisa.FirstOrDefault(t => t.Id == g.Key)?.Nombre ?? "Sin tipo",
                            Ordenes = g.ToList()
                        })
                        .OrderBy(g => g.TipoCamisa);

                    // Se genera el contenido del PDF por cada grupo
                    foreach (var grupo in grupos)
                    {
                        // Se añade subtitulo con el tipo de camisa
                        Paragraph subtitulo = new Paragraph($"Tipo: {grupo.TipoCamisa}",
                            FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14, BaseColor.Black));
                        subtitulo.SpacingBefore = 15;
                        subtitulo.SpacingAfter = 10;
                        doc.Add(subtitulo);

                        // Se crea tabla con dos columnas para mostrar numero y cantidad de ordenes
                        PdfPTable tabla = new PdfPTable(2);
                        tabla.WidthPercentage = 100;
                        tabla.SetWidths(new float[] { 2, 1 });

                        // Encabezados de la tabla
                        tabla.AddCell(Celda("Numero de Orden", true));
                        tabla.AddCell(Celda("Cantidad", true));

                        // Se ordenan las ordenes primero por numero que se extrae del texto para orden natural
                        var ordenesOrdenadas = grupo.Ordenes
                            .Select(o => new
                            {
                                Orden = o,
                                NumeroExtraido = ExtraerNumero(o.NumeroOrden)
                            })
                            .OrderBy(x => x.NumeroExtraido.HasValue)
                            .ThenBy(x => x.NumeroExtraido)
                            .ThenBy(x => x.Orden.NumeroOrden)
                            .Select(x => x.Orden);

                        // Se agregan filas con los datos de las ordenes
                        foreach (var orden in ordenesOrdenadas)
                        {
                            tabla.AddCell(Celda(orden.NumeroOrden));
                            tabla.AddCell(Celda(orden.TotalCamisetas.ToString()));
                        }

                        // Se añade una fila con el subtotal por tipo de camisa
                        PdfPCell subtotalCell = new PdfPCell(new Phrase(
                            $"Subtotal: {grupo.Ordenes.Sum(o => o.TotalCamisetas)} camisetas",
                            normalFont));
                        subtotalCell.Colspan = 2;
                        subtotalCell.HorizontalAlignment = Element.ALIGN_RIGHT;
                        subtotalCell.PaddingTop = 8;
                        subtotalCell.Border = Rectangle.TOP_BORDER;
                        tabla.AddCell(subtotalCell);

                        doc.Add(tabla);
                    }

                    // Se añade total general de camisetas al final
                    Paragraph totalGeneral = new Paragraph(
                        $"Total general: {_ordenes.Sum(o => o.TotalCamisetas)} camisetas",
                        FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12));
                    totalGeneral.Alignment = Element.ALIGN_RIGHT;
                    totalGeneral.SpacingBefore = 15;
                    doc.Add(totalGeneral);

                    // Pie de pagina con info de la app y fecha/hora
                    Paragraph footer = new Paragraph(
                        $"Generado por CosturApp - {DateTime.Now:dd/MM/yyyy HH:mm}",
                        FontFactory.GetFont(FontFactory.HELVETICA_OBLIQUE, 10, BaseColor.Gray));
                    footer.SpacingBefore = 20;
                    footer.Alignment = Element.ALIGN_CENTER;
                    doc.Add(footer);

                    doc.Close();
                }

                MessageBox.Show("PDF generado correctamente", "Exito", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al generar el PDF: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            // Metodo auxiliar para crear celdas para la tabla del PDF, si es encabezado le pone fondo gris
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

        // Metodo que busca y extrae el primer numero que encuentra en un texto, devuelve null si no hay numero
        private int? ExtraerNumero(string texto)
        {
            var match = System.Text.RegularExpressions.Regex.Match(texto, @"\d+");
            if (match.Success && int.TryParse(match.Value, out int numero))
                return numero;

            return null;
        }

        private string NormalizarNumeroOrden(string numero)
        {
            return new string(numero
                .Where(c => !char.IsWhiteSpace(c) && c != '-') 
                .ToArray())
                .ToUpperInvariant(); 
        }

    }

}

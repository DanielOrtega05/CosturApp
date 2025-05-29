using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using CosturApp.Modelo;
using CosturApp.Servicio;

namespace CosturApp.Vista.VentanasSecundarias
{
    public partial class OrdenCrearWindow : Window
    {
        public bool IsEditMode { get; set; }
        public Orden Orden { get; set; }
        private List<TipoCamisa> _tiposCamisa = new List<TipoCamisa>();
        private TipoCamisaService _tipoCamisaService = new TipoCamisaService();
        public OrdenCrearWindow()
        {
            InitializeComponent();
            CargarComboBox();
        }

        // Constructor para edición, donde se pasa una orden existente
        public OrdenCrearWindow(Orden orden)
        {
            InitializeComponent();
            CargarComboBox();
            
            IsEditMode = true;
            Orden = orden;

            // Cargar los datos de la orden en los controles
            txbNumeroOrden.Text = orden.NumeroOrden;
            txbCantidad.Text = orden.TotalCamisetas.ToString();
            cmbTipoCamisa.SelectedItem = _tiposCamisa.FirstOrDefault(t => t.Id == orden.TipoCamisaId);

        }

        private void CargarComboBox()
        {
            _tiposCamisa = _tipoCamisaService.ObtenerTodos(); // <-- Obtiene de la DB
            cmbTipoCamisa.ItemsSource = _tiposCamisa;
            cmbTipoCamisa.DisplayMemberPath = "Nombre"; // Mostrar el nombre en el ComboBox
            cmbTipoCamisa.SelectedValuePath = "Id";     // Usar el Id como valor seleccionado
        }

        private void Aceptar_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txbNumeroOrden.Text) ||
                string.IsNullOrWhiteSpace(txbCantidad.Text) ||
                cmbTipoCamisa.SelectedItem == null)
            {
                MessageBox.Show("Por favor, completa todos los campos.", "Campos incompletos", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Validar que la cantidad sea un numero entero positivo
            if (!int.TryParse(txbCantidad.Text, out int cantidad) || cantidad <= 0) // El try parse intenta convertir el texto a entero y si puede lo guarda en la variable cantidad
            {
                MessageBox.Show("Por favor, introduce una cantidad válida (número entero mayor que 0).", "Cantidad inválida", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (IsEditMode)
            {
                Orden.NumeroOrden = txbNumeroOrden.Text;
                Orden.TotalCamisetas = cantidad;

                var tipoSeleccionado = cmbTipoCamisa.SelectedItem as TipoCamisa;
                Orden.TipoCamisa = tipoSeleccionado;
                Orden.TipoCamisaId = tipoSeleccionado?.Id ?? 0;
            }
            else
            {
                var tipoSeleccionado = cmbTipoCamisa.SelectedItem as TipoCamisa;
                Orden = new Orden
                {
                    NumeroOrden = txbNumeroOrden.Text,
                    TotalCamisetas = cantidad,
                    TipoCamisa = tipoSeleccionado,
                    TipoCamisaId = tipoSeleccionado?.Id ?? 0
                };
            }

            DialogResult = true; // Esto hará que ShowDialog() devuelva true para que se pueda recuperar la informacion desde la ventana anterior
            Close();
        }

        private void Cancelar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

    }
}

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

namespace CosturApp.Vista.VentanasSecundarias
{
    public partial class OrdenCrearWindow : Window
    {
        public bool IsEditMode { get; set; }
        public Orden Orden { get; set; }
        private List<TipoCamisa> _tiposCamisa = new List<TipoCamisa>();
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
            cmbTipoCamisa.SelectedItem = _tiposCamisa.FirstOrDefault(t => t.Nombre == orden.TipoCamisa);
        }

        private void CargarComboBox()
        {
            // Crear una lista de tipos de camisa hecho asi, pero en un futuro puede venir de bd
            _tiposCamisa = new List<TipoCamisa>
            {
                new TipoCamisa { Id = 1, Nombre = "Vestir" },
                new TipoCamisa { Id = 2, Nombre = "Laboral" },
                new TipoCamisa { Id = 3, Nombre = "Guayavera" },
                new TipoCamisa { Id = 4, Nombre = "Arreglos" },
                new TipoCamisa { Id = 5, Nombre = "Encargos" },
                new TipoCamisa { Id = 6, Nombre = "Muestras" },
                new TipoCamisa { Id = 5, Nombre = "Otras" }
            };

            cmbTipoCamisa.ItemsSource = _tiposCamisa;
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

            if (IsEditMode)
            {
                // Si esta en modo edicion, actualizamos los valores de la orden
                Orden.NumeroOrden = txbNumeroOrden.Text;
                Orden.TotalCamisetas = int.TryParse(txbCantidad.Text, out var cantidad) ? cantidad : 0;
                Orden.TipoCamisa = (cmbTipoCamisa.SelectedItem as TipoCamisa)?.Nombre;
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

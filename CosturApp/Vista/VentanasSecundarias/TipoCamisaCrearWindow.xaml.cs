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

namespace CosturApp.Vista.VentanasSecundarias
{
    /// <summary>
    /// Lógica de interacción para TipoCamisaCrearWindow.xaml
    /// </summary>
    public partial class TipoCamisaCrearWindow : Window
    {
        public string NombreIngresado { get; private set; }

        public TipoCamisaCrearWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            NombreTextBox.Focus();
        }

        private void Aceptar_Click(object sender, RoutedEventArgs e)
        {
            NombreIngresado = NombreTextBox.Text.Trim();
            if (!string.IsNullOrWhiteSpace(NombreIngresado))
            {
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("Debes ingresar un nombre válido.", "Campo requerido", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void Cancelar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}

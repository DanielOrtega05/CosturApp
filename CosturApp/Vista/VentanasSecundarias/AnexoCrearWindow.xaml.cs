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
    /// Lógica de interacción para AnexoCrearWindow.xaml
    /// </summary>
    public partial class AnexoCrearWindow : Window
    {
        public string TituloIngresado { get; private set; }

        public AnexoCrearWindow()
        {
            InitializeComponent();
        }
        private void Aceptar_Click(object sender, RoutedEventArgs e)
        {
            TituloIngresado = TituloTextBox.Text.Trim();
            if (!string.IsNullOrEmpty(TituloIngresado))
            {
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("Por favor, ingresa un título válido.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void Cancelar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            TituloTextBox.Focus();
        }

    }
}

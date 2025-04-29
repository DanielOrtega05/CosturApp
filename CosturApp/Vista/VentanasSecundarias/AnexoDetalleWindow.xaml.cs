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
using CosturApp.VistaModelo;

namespace CosturApp.Vista.VentanasSecundarias
{
    /// <summary>
    /// Lógica de interacción para AnexoDetalleWindow.xaml
    /// </summary>
    public partial class AnexoDetalleWindow : Window
    {
        public AnexoDetalleWindow(Anexo anexo)
        {
            InitializeComponent();
            AnexoDetalleViewModel viewModel = new AnexoDetalleViewModel(anexo); // Asigna el anexo
            DataContext = viewModel; // Vincula la propiedad Anexo con los controles en el XAML
        }
    }
}

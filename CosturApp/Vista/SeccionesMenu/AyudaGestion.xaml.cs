using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CosturApp.Vista.SeccionesMenu
{
    /// <summary>
    /// Lógica de interacción para AyudaGestion.xaml
    /// </summary>
    public partial class AyudaGestion : UserControl
    {
        public AyudaGestion()
        {
            InitializeComponent();
        }

        private void BtnAbrirManual_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Asegúrate de que la ruta sea correcta y el PDF exista
                string rutaManual = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Documentacion", "Manual CosturApp - Primera Version.pdf");

                if (System.IO.File.Exists(rutaManual))
                {
                    Process.Start(new ProcessStartInfo(rutaManual) { UseShellExecute = true });
                }
                else
                {
                    MessageBox.Show("El manual de usuario no se encontró en la ubicación esperada.",
                                  "Archivo no encontrado",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Warning);
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Error al abrir el manual: {ex.Message}",
                              "Error",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
        }
    }
}

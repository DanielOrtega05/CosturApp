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

namespace CosturApp.Vista
{
    /// <summary>
    /// Lógica de interacción para GestionPrincipalWindow.xaml
    /// </summary>
    public partial class GestionPrincipalWindow : Window
    {
        public GestionPrincipalWindow()
        {
            InitializeComponent();
            this.PreviewKeyDown += new KeyEventHandler(HandleTeclas);
        }

        private void HandleTeclas(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.WindowState = WindowState.Normal;
                this.WindowStyle = WindowStyle.SingleBorderWindow;
            }

            if (e.Key == Key.F11)
            {
                this.WindowState = WindowState.Maximized;
            }
        }
    }
}

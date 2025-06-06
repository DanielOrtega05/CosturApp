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
using System.Windows.Navigation;
using System.Windows.Shapes;
using CosturApp.VistaModelo;

namespace CosturApp.Vista.SeccionesMenu
{
    /// <summary>
    /// Lógica de interacción para TipoCamisaGestion.xaml
    /// </summary>
    public partial class TipoCamisaGestion : UserControl
    {
        public TipoCamisaGestion()
        {
            InitializeComponent();
            DataContext = new TipoCamisaViewModel();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is TipoCamisaViewModel vm)
            {
                vm.RecargarLista();
            }
        }
    }
}

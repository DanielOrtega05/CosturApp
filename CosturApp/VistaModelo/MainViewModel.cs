using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using CosturApp.Servicio;

namespace CosturApp.VistaModelo
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public ICommand ComenzarCommand { get; }

        public MainViewModel()
        {
            ComenzarCommand = new RelayCommand(Comenzar);
        }

        private void Comenzar()
        {
            // Crea la nueva ventana y la muestra
            var gestionWindow = new Vista.GestionPrincipalWindow();
            gestionWindow.Show();

            // Cierra la ventana abierta actual
            App.Current.Windows[0]?.Close();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

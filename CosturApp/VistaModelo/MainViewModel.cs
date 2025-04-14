using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;

namespace CosturApp.VistaModelo
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public string FechaActual => DateTime.Now.ToString("dddd, dd MMMM yyyy - HH:mm");

        public MainViewModel()
        {
            var timer = new DispatcherTimer { Interval = TimeSpan.FromMinutes(1) };
            timer.Tick += (s, e) => OnPropertyChanged(nameof(FechaActual));
            timer.Start();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

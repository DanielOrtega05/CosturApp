using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CosturApp.Modelo
{
    public class Historial : INotifyPropertyChanged
    {
        private int _id;
        private string _titulo;
        private string _descripcion;
        private DateTime _fechaHistorial;

        [Key]
        public int Id
        {
            get => _id;
            set
            {
                _id = value;
                OnPropertyChanged();
            }
        }

        public string Titulo
        {
            get => _titulo;
            set
            {
                if (_titulo != value)
                {
                    _titulo = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Descripcion
        {
            get => _descripcion;
            set
            {
                if (_descripcion != value)
                {
                    _descripcion = value;
                    OnPropertyChanged();
                }
            }
        }

        public DateTime FechaHistorial
        {
            get => _fechaHistorial;
            set
            {
                if (_fechaHistorial != value)
                {
                    _fechaHistorial = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propiedad = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propiedad));
        }
    }
}

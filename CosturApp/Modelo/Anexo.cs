using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace CosturApp.Modelo
{
    // Notifica el cambio a la UI cuando alguna propiedad cambie su valor (Actualiza los bindings)
    public class Anexo : INotifyPropertyChanged
    {
        private int _id;
        private string _titulo;
        private DateTime _fechaCreacion;

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

        public DateTime FechaCreacion
        {
            get => _fechaCreacion;
            set
            {
                if (_fechaCreacion != value)
                {
                    _fechaCreacion = value;
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

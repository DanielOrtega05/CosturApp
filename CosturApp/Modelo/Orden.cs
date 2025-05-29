using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CosturApp.Modelo
{
    // Notifica el cambio a la UI cuando alguna propiedad cambie su valor (Actualiza los bindings)
    public class Orden : INotifyPropertyChanged
    {
        private string _numeroOrden;
        public string NumeroOrden // String por que puede ser "958", "Encargos", etc.
        {
            get => _numeroOrden;
            set
            {
                _numeroOrden = value;
                OnPropertyChanged();
            }
        }

        private int _totalCamisetas;
        public int TotalCamisetas
        {
            get => _totalCamisetas;
            set
            {
                _totalCamisetas = value;
                OnPropertyChanged();
            }
        }

        private int _tipoCamisaId;
        public int TipoCamisaId
        {
            get => _tipoCamisaId;
            set
            {
                _tipoCamisaId = value;
                OnPropertyChanged();
            }
        }

        private TipoCamisa _tipoCamisa { get; set; }
        public TipoCamisa TipoCamisa
        {
            get => _tipoCamisa;
            set
            {
                _tipoCamisa = value;
                OnPropertyChanged();

                // Opcional: actualizar también el ID automáticamente si se cambia el objeto
                if (_tipoCamisa != null)
                    TipoCamisaId = _tipoCamisa.Id;
            }
        }

        public int Id { get; set; }
        public int AnexoId { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string nombre = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nombre));
        }
    }


}

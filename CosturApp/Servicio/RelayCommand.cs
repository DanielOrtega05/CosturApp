 using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CosturApp.Servicio
{
    // Esta clase implementa la interfaz ICommand y se utiliza para crear comandos reutilizables,
    // especialmente es util en patrones como el que estoy usando MVVM (Modelo-Vista-VistaModelo)
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;

        private readonly Func<bool> _canExecute;

        public event EventHandler CanExecuteChanged;

        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        // Metodo que determina si el comando puede ejecutarse
        // Si no se proporciona una funcion _canExecute, devuelve true por defecto (el comando siempre se puede ejecutar)
        public bool CanExecute(object parameter) => _canExecute?.Invoke() ?? true;

        // Metodo que se llama cuando se ejecuta el comando.
        public void Execute(object parameter) => _execute();
    }
}

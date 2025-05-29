using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using CosturApp.Modelo;
using CosturApp.Vista;

namespace CosturApp.VistaModelo
{
    public class GestionPrincipalViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<MenuItemModel> MenuItems { get; set; }

        private MenuItemModel _selectedMenuItem;
        public MenuItemModel SelectedMenuItem
        {
            get => _selectedMenuItem;
            set
            {
                if (_selectedMenuItem != value)
                {
                    _selectedMenuItem = value;
                    OnPropertyChanged(nameof(SelectedMenuItem));

                    CurrentView = _selectedMenuItem?.View;
                }
            }
        }

        private object _currentView;
        public object CurrentView
        {
            get => _currentView;
            set { _currentView = value; OnPropertyChanged(nameof(CurrentView)); }
        }

        public GestionPrincipalViewModel()
        {
            MenuItems = new ObservableCollection<MenuItemModel>
        {
            new MenuItemModel { Title = "Inicio", Icon = "Home", View = new Vista.SeccionesMenu.InicioGestion() },
            new MenuItemModel { Title = "Anexo", Icon = "File", View = new Vista.SeccionesMenu.AnexoGestion() },
            new MenuItemModel { Title = "Tipos Camisas", Icon = "TshirtCrew", View = new Vista.SeccionesMenu.TipoCamisaGestion() },
            new MenuItemModel { Title = "Historial", Icon = "History", View = new Vista.SeccionesMenu.HistorialGestion() },
            new MenuItemModel { Title = "Ayuda", Icon = "HelpCircle", View = null },
        };
            SelectedMenuItem = MenuItems.FirstOrDefault();
            CurrentView = SelectedMenuItem?.View;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

}

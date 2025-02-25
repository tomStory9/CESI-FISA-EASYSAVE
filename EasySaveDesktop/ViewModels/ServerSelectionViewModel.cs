using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EasySaveBusiness.Controllers;
using EasySaveClient.Controllers;
using EasySaveDesktop.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySaveDesktop.ViewModels
{
    public partial class ServerSelectionViewModel : ObservableValidator
    {
        [ObservableProperty]
        [Required]
        private string host = "127.0.0.1";

        [ObservableProperty]
        [Required]
        [Range(1, 65535)]
        private string port = "4201";

        public Action<string, int> OnServerSelected { get; set; }

        [RelayCommand]
        private void Connect()
        {
            ValidateAllProperties();

            if (HasErrors) return;

            OnServerSelected?.Invoke(Host, int.Parse(Port));

            if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var vm = new MainWindowViewModel();
                var controller = new RemoteEasySaveController(host, int.Parse(Port));

                vm.Controller = controller;
                controller.View = vm;

                var mainWindow = new MainWindow
                {
                    DataContext = vm
                };
                mainWindow.Show();
                /*desktop.MainWindow.Close();
                desktop.MainWindow = mainWindow;*/
            }
        }
    }
}

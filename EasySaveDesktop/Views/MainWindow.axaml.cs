using Avalonia.Controls;
using EasySaveDesktop.ViewModels;
using System.Threading.Tasks;
using DialogHostAvalonia;
using EasySaveDesktop.Models;

namespace EasySaveDesktop.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        DataContextChanged += (sender, e) =>
        {
            if (DataContext is MainWindowViewModel vm)
            {
                vm.Init();
            }
        };
    }

    private async void OpenErrorDialog(string message)
    {
        await DialogHost.Show(new ErrorDialog(message));
    }
}
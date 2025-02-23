using Avalonia.Controls;
using EasySaveDesktop.ViewModels;
using System.Threading.Tasks;

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
}
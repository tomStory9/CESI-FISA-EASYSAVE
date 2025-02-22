using Avalonia.Controls;
using EasySaveDesktop.ViewModels;

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
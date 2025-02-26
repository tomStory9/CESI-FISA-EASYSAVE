using Avalonia.Controls;
using EasySaveDesktop.ViewModels;
using System.Threading.Tasks;
using DialogHostAvalonia;
using EasySaveDesktop.Models;
using System;

namespace EasySaveDesktop.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        /* DataContextChanged += (sender, e) =>
        {
            if (DataContext is MainWindowViewModel vm)
            {
                vm.Init();
            }
        }; */
    }
}
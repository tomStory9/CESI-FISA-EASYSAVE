using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using EasySaveDesktop.ViewModels;
using System;

namespace EasySaveDesktop;

public partial class BackupConfigWizardWindow : Window
{
    public BackupConfigWizardWindow()
    {
        InitializeComponent();

        if (DataContext is BackupConfigWizardViewModel viewModel)
        {
            viewModel.BackupConfigCompleted += OnBackupConfigCompleted;
        }
    }

    private void OnBackupConfigCompleted(object? sender, EventArgs e)
    {
        Close();
    }
}
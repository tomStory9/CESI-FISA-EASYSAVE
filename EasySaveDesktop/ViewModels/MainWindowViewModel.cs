using EasySaveBusiness.Models;
using EasySaveBusiness.Views;
using EasySaveBusiness.Controllers;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Linq;
using System;
using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using DialogHostAvalonia;
using EasySaveDesktop.Models;
using Tmds.DBus.Protocol;

namespace EasySaveDesktop.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase, IView
    {
        public IEasySaveController Controller { get; set; }

        public string Greeting { get; } = "Welcome to Avalonia!";

        [ObservableProperty]
        private List<BackupConfig> backupConfigs = [];

        [ObservableProperty]
        private ObservableCollection<BackupJobViewModel> backupJobs = [];

        public bool ShowMassActionButtons => BackupJobs.Any(job => job.IsChecked);

        public MainWindowViewModel()
        {
            BackupJobs.CollectionChanged += BackupJobs_CollectionChanged;
        }

        public void Init()
        {
            Controller.Init();
        }

        private void BackupJobs_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs? e)
        {
            if (e?.NewItems != null)
            {
                foreach (BackupJobViewModel item in e.NewItems)
                {
                    item.PropertyChanged += BackupJobs_PropertyChanged;
                }
            }

            if (e?.OldItems != null)
            {
                foreach (BackupJobViewModel item in e.OldItems)
                {
                    item.PropertyChanged -= BackupJobs_PropertyChanged;
                }
            }
        }

        private void BackupJobs_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs? e)
        {
            OnPropertyChanged(nameof(ShowMassActionButtons));
        }

        [RelayCommand]
        private void StartAll()
        {
            Console.WriteLine("Start all");
        }

        [RelayCommand]
        private async void OpenCreateBackupConfigWindow()
        {
            var createBackupConfigViewModel = new CreateBackupConfigViewModel();
            var createBackupConfigWindow = new CreateBackupConfigWindow
            {
                DataContext = createBackupConfigViewModel
            };

            if (Avalonia.Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                await createBackupConfigWindow.ShowDialog<CreateBackupConfigViewModel>(desktop.MainWindow);
            }

            /* createBackupConfigViewModel.BackupConfigCreated += (backupConfig) =>
            {
                Controller.AddBackupConfig(backupConfig);
                createBackupConfigWindow.Close();
            }; */

        }

        public void DisplayError(string errorMessage)
        {
            Console.WriteLine(errorMessage);
            DialogHost.Show(new ErrorDialog(errorMessage));
        }

        public void DisplayMessage(string message)
        {
            throw new System.NotImplementedException();
        }

        public void RefreshBackupConfigs(List<BackupConfig> backupConfigs)
        {
            BackupConfigs = backupConfigs;
        }

        public void RefreshBackupJobFullStates(List<BackupJobFullState> backupJobFullState)
        {
            BackupJobs.Clear();
            foreach (var job in backupJobFullState)
            {
                BackupJobs.Add(new BackupJobViewModel(Controller, job));
            }
        }
    }
}

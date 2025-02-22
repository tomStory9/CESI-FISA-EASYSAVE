using EasySaveBusiness.Models;
using EasySaveBusiness.Views;
using EasySaveBusiness.Controllers;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EasySaveDesktop.Models;
using System.Linq;
using System;
using System.Collections.ObjectModel;

namespace EasySaveDesktop.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase, IView
    {
        public EasySaveController Controller { private get; set; }

        public string Greeting { get; } = "Welcome to Avalonia!";

        [ObservableProperty]
        private List<BackupConfig> backupConfigs = [];

        [ObservableProperty]
        private ObservableCollection<BackupJob> backupJobs = [];

        public MainWindowViewModel()
        {
            
        }

        public void Init()
        {
            Controller.Init();
        }

        [RelayCommand]
        private void Test()
        {
            Controller.StartBackupJob(0);
        }

        public void DisplayError(string errorMessage)
        {
            throw new System.NotImplementedException();
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
            BackupJobs = [.. backupJobFullState.Select(BackupJob.FromBackupJobFullState)];
        }

        partial void OnBackupJobsChanged(ObservableCollection<BackupJob> value)
        {
            Console.WriteLine("Backup jobs changed");
            Controller.OverrideBackupConfigs(value.Select(job => (BackupConfig) job).ToList());
        }
    }
}

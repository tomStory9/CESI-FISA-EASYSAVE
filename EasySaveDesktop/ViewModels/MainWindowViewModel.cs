﻿using EasySaveBusiness.Models;
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
using System.Threading.Tasks;

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

        public async Task Init()
        {
            await Controller.Init();
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
        private async Task OpenCreateBackupConfigWindow()
        {
            var createBackupConfigViewModel = new BackupConfigWizardViewModel(Controller);
            var createBackupConfigWindow = new BackupConfigWizardWindow
            {
                DataContext = createBackupConfigViewModel
            };

            if (Avalonia.Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                await createBackupConfigWindow.ShowDialog<BackupConfigWizardViewModel>(desktop.MainWindow);
            }

            /* createBackupConfigViewModel.BackupConfigCreated += (backupConfig) =>
            {
                Controller.AddBackupConfig(backupConfig);
                createBackupConfigWindow.Close();
            }; */

        }

        public async Task DisplayError(string errorMessage)
        {
            Console.WriteLine(errorMessage);
            await DialogHost.Show(new ErrorDialog(errorMessage));
        }

        public async Task DisplayMessage(string message)
        {
            await Task.CompletedTask;
            throw new System.NotImplementedException();
        }

        public async Task RefreshBackupConfigs(List<BackupConfig> backupConfigs)
        {
            BackupConfigs = backupConfigs;
            await Task.CompletedTask;
        }

        public async Task RefreshBackupJobFullStates(List<BackupJobFullState> backupJobFullState)
        {
            BackupJobs.Clear();
            foreach (var job in backupJobFullState)
            {
                BackupJobs.Add(new BackupJobViewModel(Controller, job));
            }
            await Task.CompletedTask;
        }
    }
}

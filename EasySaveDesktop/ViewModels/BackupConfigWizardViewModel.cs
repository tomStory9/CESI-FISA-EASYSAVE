using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EasySaveBusiness.Controllers;
using EasySaveBusiness.Models;
using EasySaveBusiness.Services;
using EasySaveClient.Controllers;
using EasySaveDesktop.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace EasySaveDesktop.ViewModels
{
    public partial class BackupConfigWizardViewModel : ViewModelBase
    {
        public event EventHandler? BackupConfigCompleted;

        private readonly IEasySaveController _controller;

        [ObservableProperty]
        private string name;

        [ObservableProperty]
        private string sourceDirectory;

        [ObservableProperty]
        private string targetDirectory;

        [ObservableProperty]
        private BackupType type;

        [ObservableProperty]
        private bool encrypted;

        [ObservableProperty]
        private int currentStep = 1;

        [ObservableProperty]
        private bool isPreviousEnabled;

        [ObservableProperty]
        private bool isNextEnabled;

        [ObservableProperty]
        private bool isFinishEnabled;

        public ObservableCollection<BackupType> BackupTypes { get; } = new ObservableCollection<BackupType>
        {
            BackupType.Full,
            BackupType.Differential
        };

        public BackupConfigWizardViewModel(IEasySaveController controller)
        {
            _controller = controller;
            UpdateButtonStates();
        }

        [RelayCommand]
        private void Next()
        {
            if (CurrentStep < 3)
            {
                CurrentStep++;
            }
            UpdateButtonStates();
        }

        [RelayCommand]
        private void Previous()
        {
            if (CurrentStep > 1)
            {
                CurrentStep--;
            }
            UpdateButtonStates();
        }

        [RelayCommand]
        private async Task Finish()
        {
            var config = new BackupConfig
            {
                Name = Name,
                SourceDirectory = SourceDirectory,
                TargetDirectory = TargetDirectory,
                Type = Type,
                Encrypted = Encrypted
            };

            await _controller.AddBackupConfig(config);

            BackupConfigCompleted?.Invoke(this, EventArgs.Empty);
        }
        private void UpdateButtonStates()
        {
            IsPreviousEnabled = CurrentStep > 1;
            IsNextEnabled = CurrentStep < 3;
            IsFinishEnabled = CurrentStep == 3;
        }
    }
}

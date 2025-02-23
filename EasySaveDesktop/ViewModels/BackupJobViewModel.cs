using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EasySaveBusiness.Controllers;
using EasySaveBusiness.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySaveDesktop.ViewModels
{
    public partial class BackupJobViewModel : ViewModelBase
    {
        private readonly IEasySaveController _controller;

        public int Id { get; }

        [ObservableProperty]
        private string name;

        [ObservableProperty]
        private string sourceDirectory;

        [ObservableProperty]
        private string targetDirectory;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsReadOnly))]
        private BackupType type;

        [ObservableProperty]
        private string? sourceFilePath;

        [ObservableProperty]
        private string? targetFilePath;

        [ObservableProperty]
        private BackupJobState state;

        [ObservableProperty]
        private long totalFilesToCopy;

        [ObservableProperty]
        private long totalFilesSize;

        [ObservableProperty]
        private long nbFilesLeftToDo;

        [ObservableProperty]
        private int progression;

        [ObservableProperty]
        private bool isChecked;

        private bool IsReadOnly => State != BackupJobState.STOPPED;

        public BackupJobViewModel(IEasySaveController controller, BackupJobFullState backupJobFullState)
        {
            _controller = controller;

            Id = backupJobFullState.Id;
            name = backupJobFullState.Name;
            sourceDirectory = backupJobFullState.SourceDirectory;
            targetDirectory = backupJobFullState.TargetDirectory;
            type = backupJobFullState.Type;
            sourceFilePath = backupJobFullState.SourceFilePath;
            targetFilePath = backupJobFullState.TargetFilePath;
            state = backupJobFullState.State;
            totalFilesToCopy = backupJobFullState.TotalFilesToCopy;
            totalFilesSize = backupJobFullState.TotalFilesSize;
            nbFilesLeftToDo = backupJobFullState.NbFilesLeftToDo;
            progression = backupJobFullState.Progression;
            isChecked = false;
        }

        partial void OnNameChanged(string value)
        {
            SaveBackupConfigChanges();
        }

        partial void OnSourceDirectoryChanged(string value)
        {
            SaveBackupConfigChanges();
        }

        partial void OnTargetDirectoryChanged(string value)
        {
            SaveBackupConfigChanges();
        }

        partial void OnTypeChanged(BackupType value)
        {
            SaveBackupConfigChanges();
        }

        private void SaveBackupConfigChanges()
        {
            _controller.EditBackupConfig(new BackupConfig
            {
                Id = Id,
                Name = Name,
                SourceDirectory = SourceDirectory,
                TargetDirectory = TargetDirectory,
                Type = Type
            });
        }

        [RelayCommand]
        private void Start()
        {
            Console.WriteLine("BackupJobViewModel.Start");
        }
    }
}

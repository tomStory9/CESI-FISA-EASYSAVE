using EasySaveBusiness.Controllers;
using EasySaveBusiness.Models;
using EasySaveBusiness.Views;
using NeoSmart.PrettySize;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySaveConsole.Views
{
    public class ConsoleView : IView
    {
        public IEasySaveController Controller { get; set; }

        private List<BackupJobFullState>? _backupJobFullStates { get; set; }

        private EasySaveConfig? _easySaveConfig { get; set; }

        // The view is fully initialized when the server sent all the configs and states
        private bool _isFullyInitialized = false;
        public EventHandler? FullyInitialized { get; set; }

        private EventHandler<List<BackupJobFullState>>? NewBackupJobFullStates { get; set; }

        public async Task Init()
        {
            Console.WriteLine("Welcome to EasySave! Press CTRL+C to exit");

            Console.WriteLine("Connecting to the EasySave server...");

            await Controller.Init();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Connected to the EasySave server.");
            Console.ResetColor();
        }

        private void CheckInitialization()
        {
            if (!_isFullyInitialized && _backupJobFullStates != null && _easySaveConfig != null)
            {
                FullyInitialized?.Invoke(this, EventArgs.Empty);
            }
        }

        public async Task DisplayError(string errorMessage)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error: {errorMessage}");
            Console.ResetColor();
            await Task.CompletedTask;
        }

        public async Task DisplayMessage(string message)
        {
            Console.WriteLine(message);
        }

        public async Task RefreshBackupJobFullStates(List<BackupJobFullState> backupJobFullStates)
        {
            _backupJobFullStates = backupJobFullStates;

            NewBackupJobFullStates?.Invoke(this, _backupJobFullStates);

            CheckInitialization();
        }

        public async Task RefreshEasySaveConfig(EasySaveConfig easySaveConfig)
        {
            _easySaveConfig = easySaveConfig;

            CheckInitialization();
        }

        public async Task ExecuteParsedArguments(UserChoice userChoice)
        {
            switch (userChoice)
            {
                case UserChoice.ListBackupConfigs:
                    ListBackupConfigs();
                    break;
                case UserChoice.AddBackupConfig addBackupConfig:
                    await Controller.AddBackupConfig(addBackupConfig.Config);
                    break;
                case UserChoice.RemoveBackupConfig removeBackupConfig:
                    await Controller.RemoveBackupConfig(removeBackupConfig.Id);
                    break;
                case UserChoice.ExecuteBackups executeBackups:
                    executeBackups.Ids.ToList().ForEach(async id => await Controller.StartBackupJob(id));
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        public static void ShowHelp()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine($"  ./easysave create <name> <sourceDirectory> <targetDirectory> <type> <encrypted> - {Resources.Resources.HelpCreate}");
            Console.WriteLine($"  ./easysave remove <id> - {Resources.Resources.HelpRemove}");
            Console.WriteLine($"  ./easysave <id1;id2;id3> - {Resources.Resources.HelpExecuteSpecific}");
            Console.WriteLine($"  ./easysave <id1-id3> - {Resources.Resources.HelpExecuteRange}");
            Console.WriteLine($"  ./easysave help - {Resources.Resources.HelpHelp}");
        }

        private void ListBackupConfigs()
        {
            Console.WriteLine("Backup job configs:");
            foreach (var backupConfig in _easySaveConfig.BackupConfigs)
            {
                Console.WriteLine($"{Resources.Resources.Name}: {backupConfig.Name}");
                Console.WriteLine($"{Resources.Resources.SourceDirectory}: {backupConfig.SourceDirectory}");
                Console.WriteLine($"{Resources.Resources.TargetDirectory}: {backupConfig.TargetDirectory}");
                Console.WriteLine($"{Resources.Resources.Type}: {backupConfig.Type}");
                Console.WriteLine($"{Resources.Resources.Encrypted}: {backupConfig.Encrypted}");
                Console.WriteLine();
            }
        }

        private void ListBackupJobFullStates()
        {
            Console.WriteLine("Backup job states:");
            foreach (var backupJobFullState in _backupJobFullStates!)
            {
                Console.WriteLine($"{Resources.Resources.Id}: {backupJobFullState.Id}");
                Console.WriteLine($"{Resources.Resources.Name}: {backupJobFullState.Name}");
                Console.WriteLine($"{Resources.Resources.Type}: {backupJobFullState.Type}");
                Console.WriteLine($"{Resources.Resources.Encrypted}: {backupJobFullState.Encrypted}");
                Console.WriteLine($"{Resources.Resources.State}: {backupJobFullState.State}");
                Console.WriteLine($"{Resources.Resources.SourceDirectory}: {backupJobFullState.SourceFilePath}");
                Console.WriteLine($"{Resources.Resources.TargetDirectory}: {backupJobFullState.TargetFilePath}");
                Console.WriteLine($"{Resources.Resources.TotalFilesToCopy}: {backupJobFullState.TotalFilesToCopy}");
                Console.WriteLine($"{Resources.Resources.TotalFilesSize}: {new PrettySize(backupJobFullState.TotalFilesSize)}");
                Console.WriteLine($"{Resources.Resources.NbFilesLeftToDo}: {backupJobFullState.NbFilesLeftToDo}");
                Console.WriteLine($"{Resources.Resources.Progression}: {backupJobFullState.Progression}");
                Console.WriteLine();
            }
        }
    }
}

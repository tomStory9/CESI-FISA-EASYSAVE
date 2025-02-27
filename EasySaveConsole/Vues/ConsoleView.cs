using EasySaveBusiness.Models;
using EasySaveBusiness.ViewModels;
using EasySaveBusiness.Vues;
using EasySaveConsole.Resources;
using EasySaveConsole.Services;
using System;
using System.Collections.Generic;
using System.Threading;
using NeoSmart.PrettySize;

namespace EasySaveConsole.Vues
{
    public class ConsoleView(ArgParserService argParserService) : IView
    {
        private ArgParserService _argParserService { get; } = argParserService;
        public EasySaveViewModel? ViewModel { private get; set; }

        public void Init()
        {

        }

        public UserChoice GetUserChoice()
        {
            try
            {
                return _argParserService.ParseArguments();
            }
            catch
            {
                ShowHelp();
                System.Environment.Exit(1);
                return null;
            }
        }

        private void ShowHelp()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine($"  ./easysave create <name> <sourceDirectory> <targetDirectory> <type> - {Resources.Resources.HelpCreate}");
            Console.WriteLine($"  ./easysave remove <id> - {Resources.Resources.HelpRemove}");
            Console.WriteLine($"  ./easysave <id1;id2;id3> - {Resources.Resources.HelpExecuteSpecific}");
            Console.WriteLine($"  ./easysave <id1-id3> - {Resources.Resources.HelpExecuteRange}");
            Console.WriteLine($"  ./easysave help - {Resources.Resources.HelpHelp}");
        }

        public void DisplayBackupConfigs(Dictionary<int, BackupConfig> backupConfigs)
        {
            foreach (var config in backupConfigs)
            {
                Console.WriteLine($"{Resources.Resources.Id}: {config.Key}");
                Console.WriteLine($"{Resources.Resources.Name}: {config.Value.Name}");
                Console.WriteLine($"{Resources.Resources.SourceDirectory}: {config.Value.SourceDirectory}");
                Console.WriteLine($"{Resources.Resources.TargetDirectory}: {config.Value.TargetDirectory}");
                Console.WriteLine($"{Resources.Resources.Type}: {config.Value.Type}");
                Console.WriteLine();
            }
        }

        public void DisplayBackupJobFullState(BackupJobFullState backupJobFullState)
        {
            Console.WriteLine($"{Resources.Resources.Name}: {backupJobFullState.Name}");
            Console.WriteLine($"{Resources.Resources.SourceDirectory}: {backupJobFullState.SourceFilePath}");
            Console.WriteLine($"{Resources.Resources.TargetDirectory}: {backupJobFullState.TargetFilePath}");
            Console.WriteLine($"{Resources.Resources.State}: {backupJobFullState.State}");
            Console.WriteLine($"{Resources.Resources.TotalFilesToCopy}: {new PrettySize(backupJobFullState.TotalFilesToCopy)}");
            Console.WriteLine($"{Resources.Resources.TotalFilesSize}: {backupJobFullState.TotalFilesSize}");
            Console.WriteLine($"{Resources.Resources.NbFilesLeftToDo} : {backupJobFullState.NbFilesLeftToDo}");
            Console.WriteLine($"{Resources.Resources.Progression} :  {backupJobFullState.Progression}");

            DrawProgressBar(backupJobFullState.Progression, 50);

            Console.WriteLine();
            Console.WriteLine();
        }

        private void DrawProgressBar(int progress, int totalWidth)
        {
            int width = (progress * totalWidth) / 100;
            Console.Write("[");
            Console.Write(new string('=', width));
            Console.Write(new string(' ', totalWidth - width));
            Console.Write($"] {progress}%");
        }

        public void DisplayError(string errorMessage)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Erreur: {errorMessage}");
            Console.ResetColor();
        }

        public void DisplayMessage(string message)
        {
            Console.WriteLine(message);
        }
    }
}

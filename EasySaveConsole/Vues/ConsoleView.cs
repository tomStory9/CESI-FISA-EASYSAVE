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
            Console.WriteLine("  ./easysave create <name> <sourceDirectory> <targetDirectory> <type> - Create a new backup job");
            Console.WriteLine("  ./easysave remove <id> - Remove a backup job by ID");
            Console.WriteLine("  ./easysave <id1;id2;id3> - Execute specific backup jobs");
            Console.WriteLine("  ./easysave <id1-id3> - Execute a range of backup jobs");
            Console.WriteLine("  ./easysave /help - Show this help message");
        }

        public void DisplayBackupConfigs(Dictionary<int, BackupConfig> backupConfigs)
        {
            foreach (var config in backupConfigs)
            {
                Console.WriteLine($"ID: {config.Key}");
                Console.WriteLine($"Nom: {config.Value.Name}");
                Console.WriteLine($"Répertoire source: {config.Value.SourceDirectory}");
                Console.WriteLine($"Répertoire cible: {config.Value.TargetDirectory}");
                Console.WriteLine($"Type: {config.Value.Type}");
                Console.WriteLine();
            }
        }

        public void DisplayBackupJobFullState(BackupJobFullState backupJobFullState)
        {
            Console.WriteLine($"Nom: {backupJobFullState.Name}");
            Console.WriteLine($"Chemin source: {backupJobFullState.SourceFilePath}");
            Console.WriteLine($"Chemin cible: {backupJobFullState.TargetFilePath}");
            Console.WriteLine($"État: {backupJobFullState.State}");
            Console.WriteLine($"Total de fichiers à copier: {new PrettySize(backupJobFullState.TotalFilesToCopy)}");
            Console.WriteLine($"Taille totale des fichiers: {backupJobFullState.TotalFilesSize}");
            Console.WriteLine($"Nombre de fichiers restants: {backupJobFullState.NbFilesLeftToDo}");
            Console.WriteLine($"Progression: {backupJobFullState.Progression}%");

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

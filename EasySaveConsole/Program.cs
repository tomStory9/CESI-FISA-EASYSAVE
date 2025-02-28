using EasySaveBusiness.Models;
using EasySaveBusiness.Services;
using EasySaveClient.Controllers;
using EasySaveConsole.Services;
using EasySaveConsole.Views;
using System.Globalization;

class Program
{
    static async Task Main(string[] args)
    {
        try
        {
            SetCurrentCulture();

            string host = string.Empty;
            int port = 0;
            UserChoice userChoice = new UserChoice.ListBackupConfigs();

            try
            {
                var argParser = new ArgParserService(args);
                (host, port, userChoice) = argParser.ParseArguments();
            }
            catch (ArgumentException ex)
            {
                ConsoleView.ShowHelp();
                return;
            }

            var controller = new RemoteEasySaveController(host, port);
            var view = new ConsoleView();

            controller.View = view;
            view.Controller = controller;

            await view.Init();

            //TODO: wait for the FullyInitialized event to trigger
            var initialisationCompletionSource = new TaskCompletionSource();
            view.FullyInitialized += (sender, e) => initialisationCompletionSource.SetResult();
            await initialisationCompletionSource.Task;

            await view.ExecuteParsedArguments(userChoice);

            // Wait indefinitely
            await Task.Delay(-1);
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Erreur critique: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
            Console.ResetColor();
        }
    }

    static void SetCurrentCulture()
    {
        var culture = CultureInfo.CurrentUICulture;
        Thread.CurrentThread.CurrentCulture = culture;
    }
}

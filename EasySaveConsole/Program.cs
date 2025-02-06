using EasySaveBusiness.Models;
using EasySaveBusiness.Services;
using EasySaveBusiness.ViewModels;
using EasySaveConsole.Services;
using EasySaveConsole.Vues;
using LoggerDLL.Services;
using System.Globalization;
using System.Reflection;

class Program
{
    static void Main(string[] args)
    {
        SetCurrentCulture();

        var logPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "", "logs");

        var loggerService = new LoggerService(logPath);
        var backupConfigService = new BackupConfigService();
        var backupService = new BackupJobService(loggerService);
        var argParserService = new ArgParserService(args);

        var viewModel = new EasySaveViewModel(backupConfigService, backupService, loggerService);
        var view = new ConsoleView(argParserService);
        viewModel.View = view;
        view.ViewModel = viewModel;

        viewModel.Init();
    }

    static void SetCurrentCulture()
    {
        var culture = CultureInfo.CurrentUICulture;
        Thread.CurrentThread.CurrentCulture = culture;
    }
}
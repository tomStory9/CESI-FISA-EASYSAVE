using EasySaveBusiness.Services;
using EasySaveBusiness.ViewModels;
using EasySaveConsole.Services;
using EasySaveConsole.Vues;
using LoggerDLL.Services;
using System.Globalization;
using System.Reflection;

class Program
{
    static async Task Main(string[] args)
    {
        SetCurrentCulture();

        var logPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "", "logs");
        var fullStatePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "", "state.json");

        var loggerService = new LoggerService(logPath);
        var backupConfigService = new BackupConfigService();
        var backupFullStateLogger = new BackupFullStateLogger(fullStatePath);
        var backupService = new BackupJobService(loggerService);
        var argParserService = new ArgParserService(args);

        var viewModel = new EasySaveViewModel(
            backupConfigService,
            backupService,
            loggerService,
            backupFullStateLogger
        );
        var view = new ConsoleView(argParserService);
        viewModel.View = view;
        view.ViewModel = viewModel;

        await viewModel.InitAsync();
    }

    static void SetCurrentCulture()
    {
        var culture = CultureInfo.CurrentUICulture;
        Thread.CurrentThread.CurrentCulture = culture;
    }
}

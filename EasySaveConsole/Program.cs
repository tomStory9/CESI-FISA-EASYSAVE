using EasySaveBusiness.Models;
using EasySaveBusiness.Services;
using EasySaveBusiness.ViewModels;
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

        var backupConfigService = new BackupConfigService();
        var backupService = new BackupJobService();
        var loggerService = new LoggerService(logPath);

        var viewModel = new EasySaveViewModel(backupConfigService, backupService, loggerService);
        var view = new ConsoleVue();
    }

    static void SetCurrentCulture()
    {
        var culture = CultureInfo.CurrentUICulture;
        Thread.CurrentThread.CurrentCulture = culture;
    }
}
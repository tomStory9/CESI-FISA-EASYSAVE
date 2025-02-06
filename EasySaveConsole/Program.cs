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

        var loggerService = new LoggerService(logPath);
        var backupConfigService = new BackupConfigService();
        var backupService = new BackupJobService(loggerService)

        var viewModel = new EasySaveViewModel(backupConfigService, backupService, loggerService);
        var view = new ConsoleVue();

        var backupJobsToRun = ParseArguments(args);
        if (backupJobsToRun != null)
        {
            viewModel.ExecuteBackups(jobId);
        }
    }

    static void SetCurrentCulture()
    {
        var culture = CultureInfo.CurrentUICulture;
        Thread.CurrentThread.CurrentCulture = culture;
    }

    static List<int>? ParseArguments(string[] args)
    {
        if (args.Length == 0)
        {
            return null;
        }

        var backupJobsToRun = new List<int>();
        foreach (var arg in args)
        {
            if (arg.Contains('-'))
            {
                var range = arg.Split('-');
                if (range.Length == 2 && int.TryParse(range[0], out int start) && int.TryParse(range[1], out int end))
                {
                    for (int i = start; i <= end; i++)
                    {
                        backupJobsToRun.Add(i);
                    }
                }
            }
            else if (arg.Contains(';'))
            {
                var jobs = arg.Split(';');
                foreach (var job in jobs)
                {
                    if (int.TryParse(job, out int jobId))
                    {
                        backupJobsToRun.Add(jobId);
                    }
                }
            }
            else if (int.TryParse(arg, out int singleJobId))
            {
                backupJobsToRun.Add(singleJobId);
            }
        }

        return backupJobsToRun;
    }
}
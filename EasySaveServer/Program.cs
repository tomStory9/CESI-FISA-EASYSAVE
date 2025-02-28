using EasySaveServer.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;


using System.Reflection;
var appName = Assembly.GetEntryAssembly().GetName().Name;
var notAlreadyRunning = true;
using (var mutex = new Mutex(true, appName + "Singleton", out notAlreadyRunning))
{
    if (notAlreadyRunning)
    {

        var host = Host.CreateDefaultBuilder(args)
    .UseWindowsService()
    .ConfigureServices(services =>
    {
        services.AddCommonServices();
        services.AddHostedService<SocketServer>();
    })
    .ConfigureLogging(logging =>
    {
        logging.ClearProviders();
        logging.AddConsole();
        logging.AddEventLog();
    })
    .Build();

        await host.RunAsync();

    }
    else
        Console.Error.WriteLine(appName + " is already running.");
}

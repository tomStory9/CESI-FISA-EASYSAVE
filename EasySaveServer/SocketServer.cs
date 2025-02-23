using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using EasySaveBusiness.Controllers;
using EasySaveBusiness.Models;
using EasySaveServer.Views;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using static System.Runtime.InteropServices.JavaScript.JSType;

public class SocketServer : BackgroundService
{
    private readonly IEasySaveController _controller;
    private TcpListener _listener;
    private List<TcpClient> _clients = new();

    public SocketServer(IEasySaveController controller)
    {
        _controller = controller;
        _controller.View = new RemoteView(this);

        _listener = new TcpListener(IPAddress.Any, 5000);
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _listener.Start();
        Console.WriteLine("Serveur en attente de connexions...");

        while (!cancellationToken.IsCancellationRequested)
        {
            var client = await _listener.AcceptTcpClientAsync();
            _clients.Add(client);
            _ = HandleClientAsync(client, cancellationToken);
        }
    }

    private async Task HandleClientAsync(TcpClient client, CancellationToken cancellationToken)
    {
        using var stream = client.GetStream();
        var buffer = new byte[1024];

        while (client.Connected && !cancellationToken.IsCancellationRequested)
        {
            int bytesRead = await stream.ReadAsync(buffer, cancellationToken);
            if (bytesRead == 0) break;

            string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            HandleClientRequest(message, client);
        }

        _clients.Remove(client);
        client.Close();
    }

    private void HandleClientRequest(string message, TcpClient client)
    {
        var request = JsonSerializer.Deserialize<Dictionary<string, object>>(message);

        if (request == null || !request.ContainsKey("command"))
            return;

        string command = request["command"]?.ToString() ?? string.Empty;

        Console.WriteLine($"Command received: {command}");

        switch (command)
        {
            case "Init":
                _controller.Init();
                break;
            case "StartBackupJob":
                if (int.TryParse(request["payload"]?.ToString(), out int id))
                {
                    _controller.StartBackupJob(id);
                }
                break;
            case "AddBackupConfig":
                var config = JsonSerializer.Deserialize<BackupConfig>(request["payload"]?.ToString() ?? string.Empty);
                if (config != null)
                {
                    _controller.AddBackupConfig(config);
                }
                break;
        }
    }

    public void BroadcastEvent(string eventType, object payload)
    {
        object message = new {
            Event = eventType,
            Payload = payload
        };

        string json = JsonSerializer.Serialize(message);
        byte[] data = Encoding.UTF8.GetBytes(json);

        foreach (var client in _clients)
        {
            client.GetStream().Write(data);
        }
    }
}

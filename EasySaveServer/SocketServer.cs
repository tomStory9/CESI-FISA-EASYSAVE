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
    private readonly TimeSpan _clientCheckInterval = TimeSpan.FromSeconds(10);

    public SocketServer(IEasySaveController controller)
    {
        _controller = controller;
        _controller.View = new RemoteView(this);

        _listener = new TcpListener(IPAddress.Any, 4201);
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _listener.Start();
        Console.WriteLine("Serveur en attente de connexions...");

        _ = CheckClientsAsync(cancellationToken);

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

        Console.WriteLine("New client");

        while (client.Connected && !cancellationToken.IsCancellationRequested)
        {
            int bytesRead = await stream.ReadAsync(buffer, cancellationToken);
            if (bytesRead == 0) break;

            string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            HandleClientRequest(message, client);
        }

        Console.WriteLine("Client disconnected");

        _clients.Remove(client);
        client.Close();
    }

    private async Task CheckClientsAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(_clientCheckInterval, cancellationToken);

            for (int i = _clients.Count - 1; i >= 0; i--)
            {
                var client = _clients[i];
                if (!client.Connected)
                {
                    Console.WriteLine("Removing disconnected client");
                    _clients.RemoveAt(i);
                    client.Close();
                }
            }
        }
    }

    private void HandleClientRequest(string message, TcpClient client)
    {
        Dictionary<string, object>? request;
        try
        {
            request = JsonSerializer.Deserialize<Dictionary<string, object>>(message);
        }
        catch (JsonException ex)
        {
            Console.WriteLine(ex.Message);
            SendErrorMessage(client, "Invalid message format");
            return;
        }

        if (request == null || !request.ContainsKey("Command"))
        {
            Console.WriteLine("Invalid message format");
            SendErrorMessage(client, "Invalid message format");
            return;
        }

        string command = request["Command"]?.ToString() ?? string.Empty;

        Console.WriteLine($"Command received: {command}");

        try
        {
            switch (command)
            {
                case "Init":
                    _controller.Init();
                    break;
                case "StartBackupJob":
                    if (int.TryParse(request["Payload"]?.ToString(), out int id))
                    {
                        _controller.StartBackupJob(id);
                    }
                    break;
                case "PauseBackupJob":
                    if (int.TryParse(request["Payload"]?.ToString(), out int pauseId))
                    {
                        _controller.PauseBackupJob(pauseId);
                    }
                    break;
                case "StopBackupJob":
                    if (int.TryParse(request["Payload"]?.ToString(), out int stopId))
                    {
                        _controller.StopBackupJob(stopId);
                    }
                    break;
                case "AddBackupConfig":
                    var config = JsonSerializer.Deserialize<BackupConfig>(request["Payload"]?.ToString() ?? string.Empty);
                    if (config != null)
                    {
                        _controller.AddBackupConfig(config);
                    }
                    break;
                case "EditBackupConfig":
                    var editConfig = JsonSerializer.Deserialize<BackupConfig>(request["Payload"]?.ToString() ?? string.Empty);
                    if (editConfig != null)
                    {
                        _controller.EditBackupConfig(editConfig);
                    }
                    break;
                case "RemoveBackupConfig":
                    if (int.TryParse(request["Payload"]?.ToString(), out int removeId))
                    {
                        _controller.RemoveBackupConfig(removeId);
                    }
                    break;
                case "OverrideBackupConfigs":
                    var configs = JsonSerializer.Deserialize<List<BackupConfig>>(request["Payload"]?.ToString() ?? string.Empty);
                    if (configs != null)
                    {
                        _controller.OverrideBackupConfigs(configs);
                    }
                    break;
                case "OverrideEasySaveConfig":
                    var easySaveConfig = JsonSerializer.Deserialize<EasySaveConfig>(request["Payload"]?.ToString() ?? string.Empty);
                    if (easySaveConfig != null)
                    {
                        _controller.OverrideEasySaveConfig(easySaveConfig);
                    }
                    break;
                default:
                    SendErrorMessage(client, $"Unknown command '{command}'");
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur lors du traitement de la commande '{command}': {ex.Message}");
            SendErrorMessage(client, $"Error processing command '{command}'");
        }
    }

    public void BroadcastEvent(string eventType, object payload)
    {
        Console.WriteLine($"Broadcasting event: {eventType}");
        object message = new
        {
            Event = eventType,
            Payload = payload
        };

        string json = JsonSerializer.Serialize(message);
        byte[] data = Encoding.UTF8.GetBytes(json);

        foreach (var client in _clients)
        {
            if (client.Connected)
            {
                client.GetStream().Write(data);
            }
        }
    }

    private void SendErrorMessage(TcpClient client, string errorMessage)
    {
        var errorResponse = new
        {
            Event = "DisplayError",
            Payload = errorMessage
        };

        string json = JsonSerializer.Serialize(errorResponse);
        byte[] data = Encoding.UTF8.GetBytes(json);

        client.GetStream().Write(data);
    }
}

using EasySaveBusiness.Models;
using EasySaveBusiness.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EasySaveClient.Services
{
    public class SocketClient
    {
        private TcpClient _client;
        private NetworkStream _stream;
        private IView _view;

        public SocketClient(IView view)
        {
            _client = new TcpClient();
            _view = view;
        }

        public async Task ConnectAsync(string host, int port)
        {
            await _client.ConnectAsync(host, port);
            Console.WriteLine("Connected to server");
            _stream = _client.GetStream();
            _ = ListenForServerMessages();
        }

        public async Task SendCommandAsync(string command, object payload)
        {
            var message = new { Command = command, Payload = payload };
            string json = JsonSerializer.Serialize(message);
            byte[] data = Encoding.UTF8.GetBytes(json);
            await _stream.WriteAsync(data);
        }

        private async Task ListenForServerMessages()
        {
            var buffer = new byte[1024];

            while (true)
            {
                int bytesRead = await _stream.ReadAsync(buffer);
                if (bytesRead == 0) break;

                string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                var response = JsonSerializer.Deserialize<Dictionary<string, object>>(message);

                if (response != null && response.ContainsKey("Event"))
                {
                    string eventType = response["Event"].ToString();

                    Console.WriteLine($"Event received: {eventType}");

                    switch (eventType)
                    {
                        case "RefreshBackupJobFullStates":
                            var backupJobFullStates = JsonSerializer.Deserialize<List<BackupJobFullState>>(response["Payload"].ToString());
                            _view.RefreshBackupJobFullStates(backupJobFullStates);
                            break;
                        case "DisplayMessage":
                            _view.DisplayMessage(response["Payload"].ToString());
                            break;
                        case "DisplayError":
                            _view.DisplayError(response["Payload"].ToString());
                            break;
                    }
                }
            }
        }
    }
}

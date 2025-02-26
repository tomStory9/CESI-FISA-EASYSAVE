using EasySaveBusiness.Controllers;
using EasySaveBusiness.Models;
using EasySaveBusiness.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace EasySaveServer.Views
{
    class RemoteView : IView
    {
        private readonly SocketServer _server;
        private readonly TcpClient _client;
        public IEasySaveController Controller { get; set; }

        public RemoteView(SocketServer server)
        {
            _server = server;
        }

        public async Task Init() {
            await Task.CompletedTask;
        }

        public async Task RefreshBackupJobFullStates(List<BackupJobFullState> backupJobFullState)
        {
            _server.BroadcastEvent("RefreshBackupJobFullStates", backupJobFullState);
            await Task.CompletedTask;
        }

        public async Task DisplayMessage(string message)
        {
            _server.BroadcastEvent("DisplayMessage", message);
            await Task.CompletedTask;
        }

        public async Task DisplayError(string errorMessage)
        {
            _server.BroadcastEvent("DisplayError", errorMessage);
            await Task.CompletedTask;
        }
    }
}

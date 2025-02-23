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

        public void Init() {}

        public void RefreshBackupConfigs(List<BackupConfig> backupConfigs)
        {
            _server.BroadcastEvent("RefreshBackupConfigs", backupConfigs);
        }

        public void RefreshBackupJobFullStates(List<BackupJobFullState> backupJobFullState)
        {
            Console.WriteLine("Brodcasted RefreshBackupJobFullStates");
            _server.BroadcastEvent("RefreshBackupJobFullStates", backupJobFullState);
        }

        public void DisplayMessage(string message)
        {
            _server.BroadcastEvent("DisplayMessage", message);
        }

        public void DisplayError(string errorMessage)
        {
            _server.BroadcastEvent("DisplayError", errorMessage);
        }
    }
}

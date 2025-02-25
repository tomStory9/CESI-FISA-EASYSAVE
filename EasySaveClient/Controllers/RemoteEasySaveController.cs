using EasySaveBusiness.Controllers;
using EasySaveBusiness.Models;
using EasySaveBusiness.Views;
using EasySaveClient.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySaveClient.Controllers
{
    public class RemoteEasySaveController : IEasySaveController
    {
        public IView View { get; set; }
        private SocketClient _socketClient;
        private string _host;
        private int _port;

        public RemoteEasySaveController(string host, int port)
        {
            _host = host;
            _port = port;
        }

        public void Init()
        {
            _socketClient = new SocketClient(View);
            _socketClient.ConnectAsync(_host, _port).Wait();
            _socketClient.SendCommandAsync("Init", null).Wait();
        }

        public void AddBackupConfig(BackupConfig config)
        {
            _socketClient.SendCommandAsync("AddBackupConfig", config).Wait();
        }

        public void EditBackupConfig(BackupConfig config)
        {
            _socketClient.SendCommandAsync("EditBackupConfig", config).Wait();
        }

        public void OverrideBackupConfigs(List<BackupConfig> configs)
        {
            _socketClient.SendCommandAsync("OverrideBackupConfigs", configs).Wait();
        }

        public void RemoveBackupConfig(int id)
        {
            _socketClient.SendCommandAsync("RemoveBackupConfig", id).Wait();
        }

        public void StartBackupJob(int id)
        {
            _socketClient.SendCommandAsync("StartBackupJob", id).Wait();
        }

        public void OnBackupJobFullStateChanged(object? sender, List<BackupJobFullState> backupJobFullStates)
        {
            View.RefreshBackupJobFullStates(backupJobFullStates);
        }
    }
}

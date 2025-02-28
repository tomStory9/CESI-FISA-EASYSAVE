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

        public async Task Init()
        {
            _socketClient = new SocketClient(View);
            await _socketClient.ConnectAsync(_host, _port);
            await _socketClient.SendCommandAsync("Init", null);
        }

        public async Task AddBackupConfig(BackupConfig config)
        {
            await _socketClient.SendCommandAsync("AddBackupConfig", config);
        }

        public async Task EditBackupConfig(BackupConfig config)
        {
            await _socketClient.SendCommandAsync("EditBackupConfig", config);
        }

        public async Task OverrideBackupConfigs(List<BackupConfig> configs)
        {
            await _socketClient.SendCommandAsync("OverrideBackupConfigs", configs);
        }

        public async Task OverrideEasySaveConfig(EasySaveConfig config)
        {
            await _socketClient.SendCommandAsync("OverrideEasySaveConfig", config);
        }

        public async Task RemoveBackupConfig(int id)
        {
            await _socketClient.SendCommandAsync("RemoveBackupConfig", id);
        }

        public async Task StartBackupJob(int id)
        {
            await _socketClient.SendCommandAsync("StartBackupJob", id);
        }
        public async Task RestoreBackupJob(int id)
        {
            await _socketClient.SendCommandAsync("RestoreBackupJob", id);
        }

        public async Task PauseBackupJob(int id)
        {
            await _socketClient.SendCommandAsync("PauseBackupJob", id);
        }

        public async Task StopBackupJob(int id)
        {
            await _socketClient.SendCommandAsync("StopBackupJob", id);
        }

        public void OnBackupJobFullStateChanged(object? sender, List<BackupJobFullState> backupJobFullStates)
        {
            View.RefreshBackupJobFullStates(backupJobFullStates);
        }
    }
}

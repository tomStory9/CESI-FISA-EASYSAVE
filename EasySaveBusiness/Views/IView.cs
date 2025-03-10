﻿using EasySaveBusiness.Controllers;
using EasySaveBusiness.Models;
using EasySaveBusiness.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySaveBusiness.Views
{
    public interface IView
    {
        public IEasySaveController Controller { protected get; set; }

        public Task Init();
        public Task RefreshEasySaveConfig(EasySaveConfig easySaveConfig);
        public Task RefreshBackupJobFullStates(List<BackupJobFullState> backupJobFullStates);
        public Task DisplayMessage(string message);
        public Task DisplayError(string errorMessage);
    }
}

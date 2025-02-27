using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EasySaveBusiness.Controllers;
using EasySaveBusiness.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySaveDesktop.ViewModels
{
    public partial class EasySaveConfigViewModel: ObservableValidator
    {
        private readonly IEasySaveController _controller;

        [ObservableProperty]
        [Required]
        private string workApp;

        [ObservableProperty]
        [Required]
        private string priorityFileExtensions;

        [ObservableProperty]
        [Required]
        private int networkKoLimit;

        [ObservableProperty]
        [Required]
        private string networkInterfaceName;

        [ObservableProperty]
        [Required]
        private LoggerDLL.Models.LogType.LogTypeEnum logType;

        [ObservableProperty]
        [Required]
        private int sizeLimit;

        [ObservableProperty]
        private string? key;

        public EasySaveConfigViewModel(IEasySaveController controller, EasySaveConfig easySaveConfig)
        {
            _controller = controller;
            workApp = easySaveConfig.WorkApp;
            priorityFileExtensions = string.Join(", ", easySaveConfig.PriorityFileExtension);
            networkKoLimit = easySaveConfig.NetworkKoLimit;
            networkInterfaceName = easySaveConfig.NetworkInterfaceName;
            logType = easySaveConfig.LogType;
            sizeLimit = easySaveConfig.SizeLimit;
            key = easySaveConfig.Key;
        }

        [RelayCommand]
        public async Task Save()
        {
            ValidateAllProperties();

            if (HasErrors) return;

            var priorityFileExtensionList = priorityFileExtensions.Split(',')
                .Select(ext => ext.Trim())
                .Where(ext => !string.IsNullOrWhiteSpace(ext))
                .ToList();

            var EasySaveConfig = new EasySaveConfig(
                [], // BackupConfigs are ignored
                WorkApp,
                priorityFileExtensionList,
                NetworkKoLimit,
                NetworkInterfaceName,
                LogType,
                SizeLimit,
                Key
            );

            await _controller.OverrideEasySaveConfig(EasySaveConfig);
        }
    }
}

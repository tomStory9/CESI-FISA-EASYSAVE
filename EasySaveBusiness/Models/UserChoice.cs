using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySaveBusiness.Models
{
    public abstract record UserChoice()
    {
        public record ListBackupConfigs : UserChoice;
        public record AddBackupConfig(BackupConfig Config) : UserChoice;
        public record RemoveBackupConfig(int Id) : UserChoice;
        public record ExecuteBackups(IEnumerable<int> Ids) : UserChoice;
    }
}

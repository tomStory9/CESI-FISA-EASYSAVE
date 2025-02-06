using EasySaveBusiness.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySaveBusiness.Services
{
    public class BackupJobService
    {
        public BackupJobFullState? BackupJobFullState { get; }

        public void ExecuteBackup(BackupConfig job)
        {
            Console.WriteLine($"Executing backup: {job.Name}");
        }
    }
}

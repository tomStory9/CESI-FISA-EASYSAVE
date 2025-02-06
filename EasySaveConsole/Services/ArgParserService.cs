using EasySaveBusiness.Models;
using System;
using System.Collections.Generic;

namespace EasySaveConsole.Services
{
    public class ArgParserService(string[] args)
    {
        private string[] args { get; } = args;

        public UserChoice ParseArguments()
        {
            var args = this.args;

            if (args.Length == 0)
            {
                return new UserChoice.ListBackupConfigs();
            }

            switch (args[0].ToLower())
            {
                case "create":
                    if (args.Length == 5 && Enum.TryParse(args[4], true, out BackupType type))
                    {
                        return new UserChoice.AddBackupConfig(new BackupConfig(args[1], args[2], args[3], type));
                    }
                    break;
                case "remove":
                    if (args.Length == 2 && int.TryParse(args[1], out int id))
                    {
                        return new UserChoice.RemoveBackupConfig(id);
                    }
                    break;
                default:
                    var backupJobsToRun = ParseBackupJobs(args);
                    if (backupJobsToRun.Count > 0)
                    {
                        return new UserChoice.ExecuteBackups(backupJobsToRun);
                    }
                    break;
            }

            throw new ArgumentException("Invalid arguments");
        }

        private List<int> ParseBackupJobs(string[] args)
        {
            var backupJobsToRun = new List<int>();
            foreach (var arg in args)
            {
                if (arg.Contains('-'))
                {
                    var range = arg.Split('-');
                    if (range.Length == 2 && int.TryParse(range[0], out int start) && int.TryParse(range[1], out int end))
                    {
                        for (int i = start; i <= end; i++)
                        {
                            backupJobsToRun.Add(i);
                        }
                    }
                }
                else if (arg.Contains(';'))
                {
                    var jobs = arg.Split(';');
                    foreach (var job in jobs)
                    {
                        if (int.TryParse(job, out int jobId))
                        {
                            backupJobsToRun.Add(jobId);
                        }
                    }
                }
                else if (int.TryParse(arg, out int singleJobId))
                {
                    backupJobsToRun.Add(singleJobId);
                }
            }

            return backupJobsToRun;
        }
    }
}

using EasySaveBusiness.Models;
using System;
using System.Collections.Generic;
using EasySaveConsole.Resources;

namespace EasySaveConsole.Services
{
    public class ArgParserService
    {
        private string[] Args { get; }

        public ArgParserService(string[] args)
        {
            Args = args ?? throw new ArgumentNullException(nameof(args));
        }

        public (string Host, int Port, UserChoice UserChoice) ParseArguments()
        {
            var args = Args;

            if (args.Length < 2)
            {
                throw new ArgumentException(Resources.Resources.InvalidArguments);
            }

            var serverInfo = args[0].Split(':');
            if (serverInfo.Length != 2 || !int.TryParse(serverInfo[1], out int port))
            {
                throw new ArgumentException(Resources.Resources.InvalidServerInfo);
            }

            string host = serverInfo[0];

            try
            {
                switch (args[1].ToLower())
                {
                    case "create":
                        return (host, port, ParseCreateArguments(args.Skip(2).ToArray()));
                    case "remove":
                        return (host, port, ParseRemoveArguments(args.Skip(2).ToArray()));
                    case "list":
                        return (host, port, new UserChoice.ListBackupConfigs());
                    default:
                        return (host, port, ParseExecuteArguments(args.Skip(2).ToArray()));
                }
            }
            catch (ArgumentException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"{Resources.Resources.Error}: {ex.Message}");
                Console.ResetColor();
                throw;
            }
        }

        private UserChoice ParseCreateArguments(string[] args)
        {
            if (args.Length != 5)
            {
                throw new ArgumentException(Resources.Resources.InvalidBackupArguments__);
            }

            if (!Enum.TryParse(args[3], true, out BackupType type))
            {
                var validTypes = string.Join(", ", Enum.GetNames(typeof(BackupType)));
                throw new ArgumentException(string.Format(Resources.Resources.InvalidBackupType, args[4], validTypes));
            }

            if (!bool.TryParse(args[4], out bool encrypted))
            {
                throw new ArgumentException(Resources.Resources.InvalidEncryptionFlag);
            }

            return new UserChoice.AddBackupConfig(new BackupConfig
            {
                Id = 0, // Id is not used when creating a new backup config
                Name = args[0],
                SourceDirectory = args[1],
                TargetDirectory = args[2],
                Type = type,
                Encrypted = encrypted
            });
        }

        private UserChoice ParseRemoveArguments(string[] args)
        {
            if (args.Length != 2 || !int.TryParse(args[1], out int id))
            {
                throw new ArgumentException(Resources.Resources.InvalidRemoveArguments);
            }

            return new UserChoice.RemoveBackupConfig(id);
        }

        private UserChoice ParseExecuteArguments(string[] args)
        {
            var backupJobsToRun = ParseBackupJobs(args);
            if (backupJobsToRun.Count == 0)
            {
                throw new ArgumentException(Resources.Resources.NoValidBackupJobs);
            }

            return new UserChoice.ExecuteBackups(backupJobsToRun);
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
                    else
                    {
                        throw new ArgumentException(string.Format(Resources.Resources.InvalidBackupRange, arg));
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
                        else
                        {
                            throw new ArgumentException(string.Format(Resources.Resources.InvalidBackupId, job));
                        }
                    }
                }
                else if (int.TryParse(arg, out int singleJobId))
                {
                    backupJobsToRun.Add(singleJobId);
                }
                else
                {
                    throw new ArgumentException(string.Format(Resources.Resources.InvalidBackupArguments__, arg));
                }
            }

            return backupJobsToRun;
        }
    }
}

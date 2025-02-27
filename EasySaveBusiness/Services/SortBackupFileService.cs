using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySaveBusiness.Services
{
    public class SortBackupFileService
    {
        public List<string> SortFile(List<string> priorityExtension, string[] Files)
        {
            var extensionPriority = new Dictionary<string, int>();
            for (int i = 0; i < priorityExtension.Count; i++)
            {
                extensionPriority[priorityExtension[i]] = i;
            }

            var sortedFiles = Files.OrderBy(file =>
            {
                var extension = System.IO.Path.GetExtension(file).TrimStart('.');
                return extensionPriority.ContainsKey(extension) ? extensionPriority[extension] : int.MaxValue;
            }).ToList();

            return sortedFiles;
        }
    }
}

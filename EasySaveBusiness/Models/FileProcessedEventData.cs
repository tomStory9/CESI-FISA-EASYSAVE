using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySaveBusiness.Models
{
    public record FileProcessedEventData
    {
        public long NbFilesLeftToDo { get; init; }
        public int Progression { get; init; }
        public string SourceFilePath { get; init; }
        public string TargetFilePath { get; init; }
    }
}

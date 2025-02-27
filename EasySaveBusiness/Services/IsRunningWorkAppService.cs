using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySaveBusiness.Services
{
    public class IsRunningWorkAppService
    {
        public bool IsRunning(string processName)
        {
            return Process.GetProcessesByName(processName).Length > 0;
        }
    }
}

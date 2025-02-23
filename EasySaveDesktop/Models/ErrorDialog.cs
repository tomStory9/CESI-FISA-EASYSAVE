using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySaveDesktop.Models
{
    class ErrorDialog(string message)
    {
        public string Message { get; set; } = message;
    }
}

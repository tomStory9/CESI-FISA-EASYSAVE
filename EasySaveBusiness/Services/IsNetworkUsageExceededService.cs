using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace EasySaveBusiness.Services
{
    public class IsNetworkUsageExceededService
    {
        public static bool IsBigFileProcessing = false;
        public bool IsNetworkUsageLimitExceeded(string networkInterfaceName, long networkUsageLimit)
        {
            NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

            foreach (NetworkInterface networkInterface in networkInterfaces)
            {
                if (networkInterface.Name.Equals(networkInterfaceName, StringComparison.OrdinalIgnoreCase))
                {
                    IPv4InterfaceStatistics statistics = networkInterface.GetIPv4Statistics();
                    long bytesSent = statistics.BytesSent;
                    long bytesReceived = statistics.BytesReceived;

                    // Check if the total bytes sent and received exceed the limit
                    if (bytesSent + bytesReceived > networkUsageLimit)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}

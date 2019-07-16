using System.Net;
using System.Net.Sockets;

namespace OneAppTcpCommunication
{
    public class CommunicationUtils
    {
        public static IPAddress GetMyIpAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip;
                }
            }

            return IPAddress.Any;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using Freeway.Logging;

namespace AntServiceStack.Common.Utils
{
    public static class HostUtility
    {
        private static ILog _logger = LogManager.GetLogger(typeof(HostUtility));

        public static string IPv4 { get; private set; }

        public static string Name { get; private set; }

        static HostUtility()
        {
          HostUtility.Name = HostUtility.GetHostName();
           HostUtility.IPv4 = HostUtility.GetIPAddressFromNetworkInterface() ?? HostUtility.GetIPAddressFromDns();
        }

        private static bool NetworkInterfaceHasKeyword(NetworkInterface networkInterface, string keyword)
        {
            if (!HostUtility.ContactsIgnoreCase(networkInterface.Name, keyword))
                return HostUtility.ContactsIgnoreCase(networkInterface.Description, keyword);
            return true;
        }

        private static bool ContactsIgnoreCase(string source, string target)
        {
            return source.IndexOf(target, StringComparison.CurrentCultureIgnoreCase) >= 0;
        }

        private static string GetIPAddressFromNetworkInterface()
        {
            try
            {
                NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
                Dictionary<int, IPAddress> source = new Dictionary<int, IPAddress>();
                foreach (NetworkInterface networkInterface in networkInterfaces)
                {
                    int key = 512;
                    if (networkInterface.NetworkInterfaceType == NetworkInterfaceType.Loopback)
                        key -= 256;
                    if (HostUtility.NetworkInterfaceHasKeyword(networkInterface, "Virtual"))
                        key -= 128;
                    if (HostUtility.NetworkInterfaceHasKeyword(networkInterface, "本地连接") || HostUtility.NetworkInterfaceHasKeyword(networkInterface, "Local Area Connection"))
                        key += 64;
                    if (networkInterface.OperationalStatus == OperationalStatus.Up)
                        key += 32;
                    if (!source.ContainsKey(key))
                    {
                        foreach (UnicastIPAddressInformation unicastAddress in networkInterface.GetIPProperties().UnicastAddresses)
                        {
                            if (unicastAddress.Address.AddressFamily == AddressFamily.InterNetwork)
                            {
                                source.Add(key, unicastAddress.Address);
                                break;
                            }
                        }
                    }
                }
                return source.OrderByDescending<KeyValuePair<int, IPAddress>, int>((Func<KeyValuePair<int, IPAddress>, int>)(item => item.Key)).FirstOrDefault<KeyValuePair<int, IPAddress>>().Value.ToString();
            }
            catch (Exception ex)
            {
                HostUtility._logger.Warn("GetIPAddressFromNetworkInterface Failed.", ex);
                return (string)null;
            }
        }

        private static string GetIPAddressFromDns()
        {
            try
            {
                return ((IEnumerable<IPAddress>)Dns.GetHostAddresses(HostUtility.Name)).Where<IPAddress>((Func<IPAddress, bool>)(c => c.AddressFamily == AddressFamily.InterNetwork)).Select<IPAddress, string>((Func<IPAddress, string>)(c => c.ToString())).FirstOrDefault<string>();
            }
            catch (Exception ex)
            {
                HostUtility._logger.Warn("GetIPAddressFromDns Failed.", ex);
                return (string)null;
            }
        }

        private static string GetHostName()
        {
            try
            {
                return IPGlobalProperties.GetIPGlobalProperties().HostName;
            }
            catch (Exception ex)
            {
                HostUtility._logger.Warn("GetHostName Failed.", ex);
                return (string)null;
            }
        }
    }
}

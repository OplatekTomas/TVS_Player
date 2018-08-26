using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TVS_Player
{
    class ServerFinder{

        /// <summary>
        /// For simplification returned IP will be object sender
        /// </summary>
        public event EventHandler ServerFound;

        public ServerFinder(EventHandler whenFound) {
            ServerFound += whenFound;
        }

        public async Task RunPortScanAsync() {
            var ip = (await GetMyIP()).ToString();
            var mask = GetSubnetMask(IPAddress.Parse(ip)).ToString();
            ip = ip.Substring(0, ip.LastIndexOf('.') + 1);
            var numberOfIps = GetNumberOfIps(mask);
            List<Task> tasks = new List<Task>();
            for (double i = 1; i < numberOfIps + 1; i++) {
                tasks.Add(CheckAddress(ip + i));
            }
            await Task.WhenAll(tasks);
        }


        private async Task CheckAddress(string address) {
            Ping ping = new Ping();
            var res = await ping.SendPingAsync(address);
            if (res.Status == IPStatus.Success) {
                using (TcpClient tcpClient = new TcpClient()) {
                    try {
                        await tcpClient.ConnectAsync(address, 5850);
                        ServerFound.Invoke(address + ":" + 5850, new EventArgs());
                    } catch { }
                }
            }

        }

        private double GetNumberOfIps(string mask) {
            int totalBits = 0;
            foreach (string octet in mask.Split('.')) {
                byte octetByte = byte.Parse(octet);
                while (octetByte != 0) {
                    totalBits += octetByte & 1;
                    octetByte >>= 1;
                }
            }
            return Math.Pow(2, 32 - totalBits) - 2;
        }

        private IPAddress GetSubnetMask(IPAddress address) {
            foreach (NetworkInterface adapter in NetworkInterface.GetAllNetworkInterfaces()) {
                foreach (UnicastIPAddressInformation unicastIPAddressInformation in adapter.GetIPProperties().UnicastAddresses) {
                    if (unicastIPAddressInformation.Address.AddressFamily == AddressFamily.InterNetwork) {
                        if (address.Equals(unicastIPAddressInformation.Address)) {
                            return unicastIPAddressInformation.IPv4Mask;
                        }
                    }
                }
            }
            return IPAddress.None;
        }

        private async Task<IPAddress> GetMyIP() {
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0)) {
                await socket.ConnectAsync("8.8.8.8", 65530);
                IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                return endPoint.Address;
            }
        }
    }


}


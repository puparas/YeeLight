using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace YeeLight
{
    class Locator
    {
        const int PORT = 1982;
        const string BROADCASTIP = "239.255.255.250";
        const string MSGSTRING = "M-SEARCH * HTTP/1.1\r\n\"HOST: 192.168.0.222:1982\r\n\"MAN: \"ssdp:discover\"\r\nST: wifi_bulb\r\n";
        private static IPAddress IPADDRESS = GetLocalIPAddress();
        public List<Dictionary<string, string>> devices = new List<Dictionary<string, string>>();
        private static UdpClient client = new UdpClient(PORT);

        public void Find()
        {
            SearchDevices();
            SendBroadcastMessage();
        }

        private async void SearchDevices()
        {
            try
            {

                var result = await client.ReceiveAsync();
                string dataString = Encoding.UTF8.GetString(result.Buffer);
                Dictionary<string, string> deviceParams = Device.GetParamsFormString(dataString);
                var item = devices.Where(i => i["Location"].Contains(deviceParams["Location"]))
                         .FirstOrDefault();
                if (item == null)
                {
                    new Device(deviceParams);
                    devices.Add(deviceParams);
                }
                SearchDevices();
            }
            catch (System.ObjectDisposedException e)
            {
                throw e;
            }

        }

        private void SendBroadcastMessage()
        {
            byte[] data = Encoding.UTF8.GetBytes(MSGSTRING);
            using (UdpClient sender = new UdpClient(new IPEndPoint(IPADDRESS, PORT)))
            {
                sender.SendAsync(data, data.Length, BROADCASTIP, PORT);
            }

        }

        private static IPAddress GetLocalIPAddress()
        {
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
            {
                socket.Connect("8.8.8.8", 65530);
                IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                return endPoint.Address;
            }
        }

    }

}

// |¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯|
// |2025   ---        -<CODE BY BLAYMS>-        ---   2025|
// |______________________________________________________|

using System.Net;
using System.Net.Sockets;

namespace TestStuffInCSharp
{
    /// <summary>
    /// A class that grabs time data directly from time.windows.com
    /// </summary>
    public static class MicrosoftTime
    {
        /// <summary>
        /// Gets time directly from the server and converts it to UTC+0 timezone
        /// </summary>
        public static DateTime GetUniversal()
        {
            return GetNetworkTime().ToUniversalTime();
        }
        /// <summary>
        /// Gets time directly from the server and converts it to your local timezone
        /// </summary>
        public static DateTime GetLocal()
        {
            return GetNetworkTime().ToLocalTime();
        }
        /// <summary>
        /// Gets time directly from the server without any conversion
        /// </summary>
        public static DateTime GetNetworkTime()
        {
            string ntpServer = "time.windows.com";
            const int ntpDataLength = 48;
            byte[] ntpData = new byte[ntpDataLength];

            ntpData[0] = 0x1B;

            IPEndPoint endPoint = new IPEndPoint(Dns.GetHostAddresses(ntpServer)[0], 123);
            using (UdpClient udpClient = new UdpClient())
            {
                udpClient.Connect(endPoint);
                udpClient.Send(ntpData, ntpData.Length);
                ntpData = udpClient.Receive(ref endPoint);
            }

            ulong intPart = BitConverter.ToUInt32(ntpData, 40);
            ulong fractPart = BitConverter.ToUInt32(ntpData, 44);

            intPart = SwapEndianness(intPart);
            fractPart = SwapEndianness(fractPart);

            ulong milliseconds = (intPart * 1000) + ((fractPart * 1000) / 0x100000000L);
            DateTime networkDateTime = new DateTime(1900, 1, 1).AddMilliseconds((long)milliseconds);

            return networkDateTime;
        }

        private static uint SwapEndianness(ulong x) => (uint)(
            ((x & 0x000000ff) << 24) |
            ((x & 0x0000ff00) << 8) |
            ((x & 0x00ff0000) >> 8) |
            ((x & 0xff000000) >> 24));
    }

}

using System;
using System.Net;
using System.Net.Sockets;

namespace ReliableUdp
{
    public sealed class UdpEndPoint
	{
		public string Host { get { return EndPoint.Address.ToString(); } }
		public int Port { get { return EndPoint.Port; } }

		public readonly IPEndPoint EndPoint;

		public UdpEndPoint(IPEndPoint ipEndPoint)
		{
			EndPoint = ipEndPoint;
		}

		public override bool Equals(object obj)
		{
			if (!(obj is UdpEndPoint))
			{
				return false;
			}
			return EndPoint.Equals(((UdpEndPoint)obj).EndPoint);
		}

		public override string ToString()
		{
			return EndPoint.ToString();
		}

		public override int GetHashCode()
		{
			return EndPoint.GetHashCode();
		}

		public UdpEndPoint(string hostStr, int port)
        {
            IPAddress ipAddress = ResolveHostname(hostStr, true);
            EndPoint = new IPEndPoint(ipAddress, port);
        }

        public static IPAddress ResolveHostname(string hostStr, bool ipv6)
        {
            IPAddress ipAddress;
            if (!IPAddress.TryParse(hostStr, out ipAddress))
            {
                if (ipv6 && Socket.OSSupportsIPv6)
                {
                    if (hostStr == "localhost")
                    {
                        ipAddress = IPAddress.IPv6Loopback;
                    }
                    else
                    {
                        ipAddress = ResolveAddress(hostStr, AddressFamily.InterNetworkV6);
                    }
                }
                if (ipAddress == null)
                {
                    ipAddress = ResolveAddress(hostStr, AddressFamily.InterNetwork);
                }
            }

            if (ipAddress == null)
            {
                throw new Exception("Invalid address: " + hostStr);
            }

            return ipAddress;
        }

        private static IPAddress ResolveAddress(string hostStr, AddressFamily addressFamily)
		{
			var host = Dns.GetHostEntry(hostStr);

			foreach (IPAddress ip in host.AddressList)
			{
				if (ip.AddressFamily == addressFamily)
				{
					return ip;
				}
			}
			return null;
		}
	}

}

using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using ReliableUdp.Packet;

namespace ReliableUdp
{
    public sealed class UdpSocket
    {
        public const uint IOC_IN = 0x80000000;
        public const uint IOC_VENDOR = 0x18000000;
        public const uint SIO_UDP_CONNRESET = IOC_IN | IOC_VENDOR | 12;
        public const string MULTICAST_GROUP_I_PV4 = "224.0.0.1";
        public const string MULTICAST_GROUP_I_PV6 = "FF02:0:0:0:0:0:0:1";
        public const int SOCKET_TTL = 255;

        private Socket udpSocketv4;
        private Socket udpSocketv6;
        private Thread threadv4;
        private Thread threadv6;
        private bool running;
        private readonly UdpManager.OnMessageReceived onMessageReceived;
        private readonly object receiveLock = new object();

        private static readonly IPAddress multicastAddressV6 = IPAddress.Parse(MULTICAST_GROUP_I_PV6);
        private static readonly bool ipv6Support;
        private const int SOCKET_RECEIVE_POLL_TIME = 100000;
        private const int SOCKET_SEND_POLL_TIME = 5000;
        private const int NO_DATA_WAIT_TIME = 50;

        public UdpEndPoint LocalEndPoint { get; private set; }

        static UdpSocket()
        {
            try
            {
                //Unity3d .NET 2.0 throws exception.
                ipv6Support = Socket.OSSupportsIPv6;
            }
            catch
            {
                ipv6Support = false;
            }
        }

        public UdpSocket(UdpManager.OnMessageReceived onMessageReceived)
        {
            this.onMessageReceived = onMessageReceived;
        }

        private void ReceiveLogic(object state)
        {
            Socket socket = (Socket)state;
            EndPoint bufferEndPoint = new IPEndPoint(socket.AddressFamily == AddressFamily.InterNetwork ? IPAddress.Any : IPAddress.IPv6Any, 0);
            UdpEndPoint bufferNetEndPoint = new UdpEndPoint((IPEndPoint)bufferEndPoint);
            byte[] receiveBuffer = new byte[UdpPacket.SIZE_LIMIT];

            while (this.running)
            {
                int result;

                try
                {
                    if (socket.Available == 0 || !socket.Poll(SOCKET_RECEIVE_POLL_TIME, SelectMode.SelectRead))
                    {
                        Thread.Sleep(NO_DATA_WAIT_TIME);
                        continue;
                    }

                    result = socket.ReceiveFrom(receiveBuffer, 0, receiveBuffer.Length, SocketFlags.None, ref bufferEndPoint);
                    if (!bufferNetEndPoint.EndPoint.Equals(bufferEndPoint))
                    {
                        bufferNetEndPoint = new UdpEndPoint((IPEndPoint)bufferEndPoint);
                    }
                }
                catch (SocketException ex)
                {
                    if (ex.SocketErrorCode == SocketError.ConnectionReset ||
                         ex.SocketErrorCode == SocketError.MessageSize)
                    {
#if UDP_DEBUGGING
                        System.Diagnostics.Debug.WriteLine($"Ignored Error code {ex.SocketErrorCode} with execption {ex}.");
#endif
                        continue;
                    }

#if UDP_DEBUGGING
                    System.Diagnostics.Debug.WriteLine($"Error code {ex.SocketErrorCode} with execption {ex}.");
#endif
                    lock (receiveLock)
                    {
                        this.onMessageReceived(null, 0, (int)ex.SocketErrorCode, bufferNetEndPoint);
                    }
                    continue;
                }

#if UDP_DEBUGGING
                System.Diagnostics.Debug.WriteLine($"Received data from {bufferNetEndPoint} with result {result}.");
#endif
                lock (receiveLock)
                {
                    this.onMessageReceived(receiveBuffer, result, 0, bufferNetEndPoint);
                }
            }
        }

        public bool Bind(string ipv4Address, string ipv6Address, int port, bool reuseAddress, int socketBufferSize)
        {
            this.udpSocketv4 = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            SetConnReset(this.udpSocketv4);

            this.udpSocketv4.Blocking = false;
            this.udpSocketv4.ReceiveBufferSize = socketBufferSize;
            this.udpSocketv4.SendBufferSize = socketBufferSize;
            this.udpSocketv4.Ttl = SOCKET_TTL;

            try
            {
                this.udpSocketv4.ExclusiveAddressUse = !reuseAddress;
            }
            catch (SocketException ex)
            {
            }

            this.udpSocketv4.DontFragment = true;

            try
            {
                this.udpSocketv4.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
            }
            catch (SocketException ex)
            {
#if UDP_DEBUGGING
                System.Diagnostics.Debug.WriteLine($"Broadcast error {ex}.");
#endif
            }

            IPAddress ipv4 = IPAddress.Any;

            if (!string.IsNullOrEmpty(ipv4Address))
            {
                ipv4 = UdpEndPoint.ResolveHostname(ipv4Address, false);
            }

            if (!BindSocket(this.udpSocketv4, new IPEndPoint(ipv4, port)))
            {
                return false;
            }
            this.LocalEndPoint = new UdpEndPoint((IPEndPoint)this.udpSocketv4.LocalEndPoint);

            this.running = true;
			this.threadv4 = new Thread(ReceiveLogic)
			{
				Name = "SocketThreadv4(" + port + ")",
				IsBackground = true
			};
			this.threadv4.Start(this.udpSocketv4);

            if (!ipv6Support)
                return true;

            port = this.LocalEndPoint.Port;

            this.udpSocketv6 = new Socket(AddressFamily.InterNetworkV6, SocketType.Dgram, ProtocolType.Udp);
            SetConnReset(this.udpSocketv6);
            this.udpSocketv6.Blocking = false;
            this.udpSocketv6.ReceiveBufferSize = socketBufferSize;
            this.udpSocketv6.SendBufferSize = socketBufferSize;

            try
            {
                this.udpSocketv6.ExclusiveAddressUse = !reuseAddress;
            }
            catch (SocketException ex)
            {
            }

            this.udpSocketv6.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, true);

            IPAddress ipv6 = IPAddress.IPv6Any;

            if (!string.IsNullOrEmpty(ipv6Address))
            {
                ipv6 = UdpEndPoint.ResolveHostname(ipv6Address, true);
            }


            if (BindSocket(this.udpSocketv6, new IPEndPoint(ipv6, port)))
            {
                this.LocalEndPoint = new UdpEndPoint((IPEndPoint)this.udpSocketv6.LocalEndPoint);

#if !ENABLE_IL2CPP
                try
                {
                    this.udpSocketv6.SetSocketOption(
                         SocketOptionLevel.IPv6,
                         SocketOptionName.AddMembership,
                         new IPv6MulticastOption(multicastAddressV6));
                }
                catch
                {
                    // Unity3d throws exception - ignored
                }
#endif

				this.threadv6 = new Thread(ReceiveLogic)
				{
					Name = "SocketThreadv6(" + port + ")",
					IsBackground = true
				};
				this.threadv6.Start(this.udpSocketv6);
            }

            return true;
        }

        private void SetConnReset(Socket socket)
        {
            try
            {
                socket.IOControl(unchecked((int)SIO_UDP_CONNRESET), new byte[] { Convert.ToByte(false) }, null);
            }
            catch (Exception)
            {
                // will fail on none win os
            }
        }

        private bool BindSocket(Socket socket, IPEndPoint ep)
        {
            try
            {
                socket.Bind(ep);
#if UDP_DEBUGGING
                System.Diagnostics.Debug.WriteLine($"Successfully binded to port {((IPEndPoint)socket.LocalEndPoint).Port}.");
#endif
            }
            catch (SocketException ex)
            {
#if UDP_DEBUGGING
                System.Diagnostics.Debug.WriteLine($"Bind error {ex}");
#endif

                if (ex.SocketErrorCode == SocketError.AddressFamilyNotSupported)
                {
                    return true;
                }
                return false;
            }
            return true;
        }

        public bool SendBroadcast(byte[] data, int offset, int size, int port)
        {
            try
            {
                int result = this.udpSocketv4.SendTo(data, offset, size, SocketFlags.None, new IPEndPoint(IPAddress.Broadcast, port));
                if (result <= 0)
                    return false;
                if (ipv6Support)
                {
                    result = this.udpSocketv6.SendTo(data, offset, size, SocketFlags.None, new IPEndPoint(multicastAddressV6, port));
                    if (result <= 0)
                        return false;
                }
            }
            catch (Exception ex)
            {
#if UDP_DEBUGGING
                System.Diagnostics.Debug.WriteLine(ex.ToString());
#endif
                return false;
            }
            return true;
        }

        public int SendTo(byte[] data, int offset, int size, UdpEndPoint remoteEndPoint, ref int errorCode)
        {
            try
            {
                int result = 0;
                if (remoteEndPoint.EndPoint.AddressFamily == AddressFamily.InterNetwork)
                {
                    if (!this.udpSocketv4.Poll(SOCKET_SEND_POLL_TIME, SelectMode.SelectWrite))
                        return -1;
                    result = this.udpSocketv4.SendTo(data, offset, size, SocketFlags.None, remoteEndPoint.EndPoint);
                }
                else if (ipv6Support)
                {
                    if (!this.udpSocketv6.Poll(SOCKET_SEND_POLL_TIME, SelectMode.SelectWrite))
                        return -1;
                    result = this.udpSocketv6.SendTo(data, offset, size, SocketFlags.None, remoteEndPoint.EndPoint);
                }

#if UDP_DEBUGGING
                System.Diagnostics.Debug.WriteLine($"Send packet to {remoteEndPoint.EndPoint} with result {result}");
#endif
                return result;
            }
            catch (SocketException ex)
            {
                if (ex.SocketErrorCode == SocketError.NoBufferSpaceAvailable)
                {
                    return 0;
                }

                if (ex.SocketErrorCode != SocketError.MessageSize)
                {
#if UDP_DEBUGGING
                    System.Diagnostics.Debug.WriteLine(ex.ToString());
#endif
                }

                errorCode = (int)ex.SocketErrorCode;
                return -1;
            }
            catch (Exception ex)
            {
#if UDP_DEBUGGING
                System.Diagnostics.Debug.WriteLine(ex.ToString());
#endif
                return -1;
            }
        }

        private void CloseSocket(Socket s)
        {
#if NETCORE
            s.Dispose();
#else
            s.Close();
#endif
        }

        public void Close()
        {
            this.running = false;

            if (this.udpSocketv4 != null)
            {
                CloseSocket(this.udpSocketv4);
                this.udpSocketv4 = null;
            }
            if (Thread.CurrentThread != this.threadv4)
            {
                this.threadv4.Join();
            }
            this.threadv4 = null;

            if (this.udpSocketv6 == null)
                return;

            if (this.udpSocketv6 != null)
            {
                CloseSocket(this.udpSocketv6);
                this.udpSocketv6 = null;
            }

            if (Thread.CurrentThread != this.threadv6)
            {
                this.threadv6.Join();
            }
            this.threadv6 = null;
        }
    }

}

using System;
using System.Collections.Generic;
using System.Text;

using ReliableUdp.PacketHandler;
using System.Threading;

using ReliableUdp.BitUtility;
using ReliableUdp.Const;
using ReliableUdp.Enums;
using ReliableUdp.Packet;
using ReliableUdp.Utility;
using System.Collections.Concurrent;

namespace ReliableUdp
{
    public sealed class UdpManager
    {
        public delegate void OnMessageReceived(byte[] data, int length, int errorCode, UdpEndPoint remoteEndPoint);

        private readonly UdpSocket socket;

        private readonly Thread updateThread;

        private ConcurrentQueue<UdpEvent> netEventsQueue;
        private readonly Stack<UdpEvent> netEventsPool;
        private readonly IUdpEventListener netEventListener;

        private readonly UdpPeerCollection peers;
        private readonly int maxConnections;

        public UdpSettings Settings = new UdpSettings();

        public ulong PacketsSent { get; private set; }
        public ulong PacketsReceived { get; private set; }
        public ulong BytesSent { get; private set; }
        public ulong BytesReceived { get; private set; }

        /// <summary>
        /// Returns true if socket listening and update thread is running
        /// </summary>
        public bool IsRunning { get; private set; }

        /// <summary>
        /// Local EndPoint (host and port)
        /// </summary>
        public UdpEndPoint LocalEndPoint
        {
            get { return this.socket.LocalEndPoint; }
        }

        /// <summary>
        /// Connected peers count
        /// </summary>
        public int PeersCount
        {
            get { return this.peers.Count; }
        }

        public UdpPacketPool PacketPool { get; }

        /// <summary>
        /// NetManager constructor
        /// </summary>
        /// <param name="listener">Network events listener</param>
        /// <param name="maxConnections">Maximum connections (incoming and outcoming)</param>
        /// <param name="connectKey">Application key (must be same with remote host for establish connection)</param>
        public UdpManager(IUdpEventListener listener, string connectKey, int maxConnections = int.MaxValue)
        {
            this.updateThread = new Thread(this.Update) { Name = "UpdateThread", IsBackground = true };
            this.socket = new UdpSocket(this.HandlePacket);
            this.netEventListener = listener;
            this.netEventsQueue = new ConcurrentQueue<UdpEvent>();
            this.netEventsPool = new Stack<UdpEvent>();
            this.PacketPool = new UdpPacketPool();

            this.Settings.ConnectKey = connectKey;
            this.peers = new UdpPeerCollection(maxConnections);
            this.maxConnections = maxConnections;

            listener.UdpManager = this;
        }

        public void ConnectionLatencyUpdated(UdpPeer fromPeer, int latency)
        {
            this.CreateLatencyUpdateEvent(fromPeer, latency);
        }

        public bool SendRawAndRecycle(UdpPacket packet, UdpEndPoint remoteEndPoint)
        {
            var result = SendRaw(packet.RawData, 0, packet.Size, remoteEndPoint);
            this.PacketPool.Recycle(packet);
            return result;
        }

        public bool SendRaw(byte[] message, int start, int length, UdpEndPoint remoteEndPoint)
        {
            if (!IsRunning)
                return false;

            int errorCode = 0;
            bool result = this.socket.SendTo(message, start, length, remoteEndPoint, ref errorCode) > 0;

            //10040 message to long... need to check
            //10065 no route to host
            if (errorCode != 0 && errorCode != 10040 && errorCode != 10065)
            {
                UdpPeer fromPeer;
                if (this.peers.TryGetValue(remoteEndPoint, out fromPeer))
                {
                    DisconnectPeer(fromPeer, DisconnectReason.SocketSendError, errorCode, false, null, 0, 0);
                }
                this.CreateErrorEvent(remoteEndPoint, errorCode);
                return false;
            }
            if (errorCode == 10040)
            {
#if UDP_DEBUGGING
                System.Diagnostics.Debug.WriteLine($"10040, datalen {length}");
#endif
                return false;
            }

            PacketsSent++;
            BytesSent += (uint)length;

            return result;
        }

        private void DisconnectPeer(
             UdpPeer peer,
             DisconnectReason reason,
             int socketErrorCode,
             bool sendDisconnectPacket,
             byte[] data,
             int start,
             int count)
        {
            if (sendDisconnectPacket)
            {
                if (count + 8 >= peer.PacketMtuHandler.Mtu)
                {
                    data = null;
                    count = 0;
#if UDP_DEBUGGING
                    System.Diagnostics.Debug.WriteLine("Disconnect data size is more than MTU");
#endif
                }

                var disconnectPacket = this.PacketPool.Get(PacketType.Disconnect, 8 + count);
                BitHelper.Write(disconnectPacket.RawData, 1, peer.ConnectId);
                if (data != null)
                {
                    Buffer.BlockCopy(data, start, disconnectPacket.RawData, 9, count);
                }
                SendRawAndRecycle(disconnectPacket, peer.EndPoint);
            }
            this.CreateDisconnectEvent(peer, reason, socketErrorCode);
            RemovePeer(peer.EndPoint);
        }

        private void ClearPeers()
        {
            lock (this.peers)
            {
                this.peers.Clear();
            }
        }

        private void RemovePeer(UdpEndPoint endPoint)
        {
            this.peers.Remove(endPoint);
        }

        private void RemovePeerAt(int idx)
        {
            this.peers.RemoveAt(idx);
        }

        private void Update()
        {
            while (IsRunning)
            {
                var startTime = DateTime.UtcNow;

                if (this.Settings.NetworkSimulation != null)
                {
                    this.Settings.NetworkSimulation.Update(this.DataReceived);
                }

                this.UpdatePeers();

                int sleepTime = this.Settings.UpdateSleepTime - (int)(DateTime.UtcNow - startTime).TotalMilliseconds;
                if (sleepTime > 0)
                {
                    Thread.Sleep(sleepTime);
                }
            }
        }

        private void UpdatePeers()
        {
            lock (this.peers)
            {
                for (int i = 0; i < this.peers.Count; i++)
                {
                    var udpPeer = this.peers[i];
                    if (udpPeer.ConnectionState == ConnectionState.Connected
                        && udpPeer.NetworkStatisticManagement.TimeSinceLastPacket > this.Settings.DisconnectTimeout)
                    {
#if UDP_DEBUGGING
                        System.Diagnostics.Debug.WriteLine($"Disconnect by timeout {udpPeer.NetworkStatisticManagement.TimeSinceLastPacket} > {this.Settings.DisconnectTimeout}");
#endif
                        this.CreateDisconnectEvent(udpPeer, DisconnectReason.Timeout, 0);

                        this.RemovePeerAt(i);
                        i--;
                    }
                    else if (udpPeer.ConnectionState == ConnectionState.Disconnected)
                    {
                        this.CreateDisconnectEvent(udpPeer, DisconnectReason.ConnectionFailed, 0);

                        this.RemovePeerAt(i);
                        i--;
                    }
                    else
                    {
                        udpPeer.Update(this.Settings.UpdateSleepTime);
                    }
                }
            }
        }

        private void HandlePacket(byte[] data, int length, int errorCode, UdpEndPoint remoteEndPoint)
        {
            if (errorCode == 0)
            {
                bool receivePacket = true;

                if (this.Settings.NetworkSimulation != null)
                {
                    receivePacket = this.Settings.NetworkSimulation.HandlePacket(data, length, remoteEndPoint);
                }

                if (receivePacket)
                    DataReceived(data, length, remoteEndPoint);
            }
            else
            {
                ClearPeers();
                this.CreateErrorEvent(null, errorCode);
            }
        }

        private void DataReceived(byte[] reusableBuffer, int count, UdpEndPoint remoteEndPoint)
        {
            PacketsReceived++;
            BytesReceived += (uint)count;

            UdpPacket packet = this.PacketPool.GetAndRead(reusableBuffer, 0, count);
            if (packet == null)
            {
#if UDP_DEBUGGING
                System.Diagnostics.Debug.WriteLine($"Data Received but packet is null.");
#endif
                return;
            }

            if (packet.Type == PacketType.UnconnectedMessage)
            {
                this.CreateReceiveUnconnectedEvent(remoteEndPoint, packet, count);
                return;
            }

            UdpPeer udpPeer;

            lock (this.peers)
            {
                this.peers.TryGetValue(remoteEndPoint, out udpPeer);
            }
            int peersCount = this.peers.Count;

            if (udpPeer != null)
            {
                if (packet.Type == PacketType.Disconnect)
                {
                    if (BitConverter.ToInt64(packet.RawData, 1) != udpPeer.ConnectId)
                    {
                        this.PacketPool.Recycle(packet);
                        return;
                    }

                    this.CreateDisconnectEventWithData(udpPeer, packet);

                    this.peers.Remove(udpPeer.EndPoint);
                }
                else
                {
                    udpPeer.ProcessPacket(packet);
                }
                return;
            }

            if (peersCount < this.maxConnections && packet.Type == PacketType.ConnectRequest)
            {
                int protoId = BitConverter.ToInt32(packet.RawData, 1);
                if (protoId != ConnectionRequestHandler.PROTOCOL_ID)
                {
#if UDP_DEBUGGING
                    System.Diagnostics.Debug.WriteLine($"Peer connect rejected. Invalid Protocol Id.");
#endif
                    return;
                }

                string peerKey = Encoding.UTF8.GetString(packet.RawData, 13, packet.Size - 13);
                if (peerKey != this.Settings.ConnectKey)
                {
#if UDP_DEBUGGING
                    System.Diagnostics.Debug.WriteLine($"Peer connect rejected. Invalid key {peerKey}.");
#endif
                    return;
                }

                long connectionId = BitConverter.ToInt64(packet.RawData, 5);
                udpPeer = new UdpPeer(this, remoteEndPoint, connectionId);
#if UDP_DEBUGGING
                System.Diagnostics.Debug.WriteLine($"Received Peer connect request Id {udpPeer.ConnectId} EP {remoteEndPoint}.");
#endif

                this.PacketPool.Recycle(packet);

                this.peers.Add(remoteEndPoint, udpPeer);

                this.CreateConnectEvent(udpPeer);
            }
        }

        public void ReceiveFromPeer(UdpPacket packet, UdpEndPoint remoteEndPoint, ChannelType channel)
        {
            UdpPeer fromPeer;
            lock(this.peers)
            {
                this.peers.TryGetValue(remoteEndPoint, out fromPeer);
            }

            if (fromPeer != null)
            {
#if UDP_DEBUGGING
                System.Diagnostics.Debug.WriteLine($"Received message.");
#endif
                this.CreateReceiveEvent(packet, channel, fromPeer);
            }
        }

        public void ReceiveAckFromPeer(UdpPacket packet, UdpEndPoint remoteEndPoint, ChannelType channel)
        {
            UdpPeer fromPeer;
            if (this.peers.TryGetValue(remoteEndPoint, out fromPeer))
            {
#if UDP_DEBUGGING
                System.Diagnostics.Debug.WriteLine($"Received ack message.");
#endif
                this.CreateReceiveAckEvent(packet, channel, fromPeer);
            }
        }

        public void SendTo(long id, UdpDataWriter writer, Enums.ChannelType channelType)
        {
            SendTo(id, writer.Data, 0, writer.Length, channelType);
        }

        private void SendTo(long id, byte[] data, int start, int length, ChannelType channelType)
        {
            lock (this.peers)
            {
                UdpPeer peer;
                if (peers.TryGetValue(id, out peer))
                {
                    peer.Send(data, start, length, channelType);
                }
            }
        }

        /// <summary>
        /// Send data to all connected peers
        /// </summary>
        /// <param name="writer">DataWriter with data</param>
        /// <param name="channelType">Send options (reliable, unreliable, etc.)</param>
        public void SendToAll(UdpDataWriter writer, Enums.ChannelType channelType)
        {
            SendToAll(writer.Data, 0, writer.Length, channelType);
        }

        /// <summary>
        /// Send data to all connected peers
        /// </summary>
        /// <param name="data">Data</param>
        /// <param name="channelType">Send options (reliable, unreliable, etc.)</param>
        public void SendToAll(byte[] data, Enums.ChannelType channelType)
        {
            SendToAll(data, 0, data.Length, channelType);
        }

        /// <summary>
        /// Send data to all connected peers
        /// </summary>
        /// <param name="data">Data</param>
        /// <param name="start">Start of data</param>
        /// <param name="length">Length of data</param>
        /// <param name="channelType">Send options (reliable, unreliable, etc.)</param>
        public void SendToAll(byte[] data, int start, int length, Enums.ChannelType channelType)
        {
            lock (this.peers)
            {
                for (int i = 0; i < this.peers.Count; i++)
                {
                    this.peers[i].Send(data, start, length, channelType);
                }
            }
        }


        /// <summary>
        /// Send data to all connected peers
        /// </summary>
        /// <param name="writer">DataWriter with data</param>
        /// <param name="channelType">Send options (reliable, unreliable, etc.)</param>
        /// <param name="excludePeer">Excluded peer</param>
        public void SendToAll(UdpDataWriter writer, Enums.ChannelType channelType, UdpPeer excludePeer)
        {
            SendToAll(writer.Data, 0, writer.Length, channelType, excludePeer);
        }

        /// <summary>
        /// Send data to all connected peers
        /// </summary>
        /// <param name="data">Data</param>
        /// <param name="channelType">Send options (reliable, unreliable, etc.)</param>
        /// <param name="excludePeer">Excluded peer</param>
        public void SendToAll(byte[] data, Enums.ChannelType channelType, UdpPeer excludePeer)
        {
            SendToAll(data, 0, data.Length, channelType, excludePeer);
        }

        /// <summary>
        /// Send data to all connected peers
        /// </summary>
        /// <param name="data">Data</param>
        /// <param name="start">Start of data</param>
        /// <param name="length">Length of data</param>
        /// <param name="channelType">Send options (reliable, unreliable, etc.)</param>
        /// <param name="excludePeer">Excluded peer</param>
        public void SendToAll(byte[] data, int start, int length, Enums.ChannelType channelType, UdpPeer excludePeer)
        {
            lock (this.peers)
            {
                for (int i = 0; i < this.peers.Count; i++)
                {
                    var udpPeer = this.peers[i];
                    if (udpPeer != excludePeer)
                    {
                        udpPeer.Send(data, start, length, channelType);
                    }
                }
            }
        }

        /// <summary>
        /// Start logic thread and listening on available port
        /// </summary>
        public bool Start()
        {
            return Start(0);
        }

        /// <summary>
        /// Start logic thread and listening on selected port
        /// </summary>
        /// <param name="port">port to listen</param>
        public bool Start(int port, int socketBufferSize = 1024 * 1024)
        {
            return Start(string.Empty, string.Empty, port, socketBufferSize);
        }

        public bool Start(string ipv4Address, string ipv6Address, int port, int socketBufferSize = 1024*1024)
        {
            if (IsRunning)
            {
                return false;
            }

            this.netEventsQueue = new ConcurrentQueue<UdpEvent>();
            if (!this.socket.Bind(ipv4Address, ipv6Address, port, this.Settings.ReuseAddress, socketBufferSize))
                return false;

            StartUpdateThread();
            return true;
        }

        private void StartUpdateThread()
        {
            IsRunning = true;
            this.updateThread.Start();
        }

        /// <summary>
        /// Send message without connection
        /// </summary>
        /// <param name="message">Raw data</param>
        /// <param name="remoteEndPoint">Packet destination</param>
        /// <returns>Operation result</returns>
        public bool SendUnconnectedMessage(byte[] message, UdpEndPoint remoteEndPoint)
        {
            return SendUnconnectedMessage(message, 0, message.Length, remoteEndPoint);
        }

        /// <summary>
        /// Send message without connection
        /// </summary>
        /// <param name="writer">Data serializer</param>
        /// <param name="remoteEndPoint">Packet destination</param>
        /// <returns>Operation result</returns>
        public bool SendUnconnectedMessage(UdpDataWriter writer, UdpEndPoint remoteEndPoint)
        {
            return SendUnconnectedMessage(writer.Data, 0, writer.Length, remoteEndPoint);
        }

        /// <summary>
        /// Send message without connection
        /// </summary>
        /// <param name="message">Raw data</param>
        /// <param name="start">data start</param>
        /// <param name="length">data length</param>
        /// <param name="remoteEndPoint">Packet destination</param>
        /// <returns>Operation result</returns>
        public bool SendUnconnectedMessage(byte[] message, int start, int length, UdpEndPoint remoteEndPoint)
        {
            if (!IsRunning)
                return false;
            var packet = this.PacketPool.GetWithData(PacketType.UnconnectedMessage, message, start, length);
            bool result = SendRawAndRecycle(packet, remoteEndPoint);
            return result;
        }

        /// <summary>
        /// Receive all pending events. Call this in game update code
        /// </summary>
        public void PollEvents()
        {
            while (this.netEventsQueue.Count > 0)
            {
                UdpEvent evt;
                if (this.netEventsQueue.TryDequeue(out evt))
                {
                    ProcessEvent(evt);
                }
            }
        }

        /// <summary>
        /// Connect to remote host
        /// </summary>
        /// <param name="address">Server IP or hostname</param>
        /// <param name="port">Server Port</param>
        public UdpPeer Connect(string address, int port)
        {
            //Create target endpoint
            UdpEndPoint ep = new UdpEndPoint(address, port);
            return Connect(ep);
        }

        /// <summary>
        /// Connect to remote host
        /// </summary>
        /// <param name="target">Server end point (ip and port)</param>
        public UdpPeer Connect(UdpEndPoint target)
        {
            if (!IsRunning)
            {
                if (!this.Start())
                    throw new Exception("Client is not running");
            }
            lock (this.peers)
            {
                UdpPeer alreadyConnectedPeer = null;
                if (this.peers.TryGetValue(target, out alreadyConnectedPeer) || this.peers.Count >= this.maxConnections)
                {
                    return alreadyConnectedPeer;
                }

                var newPeer = new UdpPeer(this, target, 0);
                this.peers.Add(target, newPeer);
                return newPeer;
            }
        }

        /// <summary>
        /// Force closes connection and stop all threads.
        /// </summary>
        public void Stop()
        {
            if (!IsRunning)
                return;

            IsRunning = false;
            lock (this.peers)
            {
                for (int i = 0; i < this.peers.Count; i++)
                {
                    var disconnectPacket = this.PacketPool.Get(PacketType.Disconnect, 8);
                    BitHelper.Write(disconnectPacket.RawData, 1, this.peers[i].ConnectId);
                    SendRawAndRecycle(disconnectPacket, this.peers[i].EndPoint);
                }
            }

            ClearPeers();

            if (Thread.CurrentThread != this.updateThread)
            {
                this.updateThread.Join();
            }

            this.socket.Close();
        }

        /// <summary>
        /// Get first peer. Usefull for Client mode
        /// </summary>
        /// <returns></returns>
        public UdpPeer GetFirstPeer()
        {
            lock (this.peers)
            {
                if (this.peers.Count > 0)
                {
                    return this.peers[0];
                }
            }
            return null;
        }

        /// <summary>
        /// Get copy of current connected peers
        /// </summary>
        /// <returns>Array with connected peers</returns>
        public UdpPeer[] GetPeers()
        {
            UdpPeer[] peers;
            lock (this.peers)
            {
                peers = this.peers.ToArray();
            }
            return peers;
        }

        /// <summary>
        /// Get copy of current connected peers (without allocations)
        /// </summary>
        /// <param name="peers">List that will contain result</param>
        public void GetPeersNonAlloc(List<UdpPeer> peers)
        {
            peers.Clear();
            lock (this.peers)
            {
                for (int i = 0; i < this.peers.Count; i++)
                {
                    peers.Add(this.peers[i]);
                }
            }
        }

        /// <summary>
        /// Disconnect peer from server
        /// </summary>
        /// <param name="peer">peer to disconnect</param>
        public void DisconnectPeer(UdpPeer peer)
        {
            DisconnectPeer(peer, null, 0, 0);
        }

        /// <summary>
        /// Disconnect peer from server and send additional data (Size must be less or equal MTU - 8)
        /// </summary>
        /// <param name="peer">peer to disconnect</param>
        /// <param name="data">additional data</param>
        public void DisconnectPeer(UdpPeer peer, byte[] data)
        {
            DisconnectPeer(peer, data, 0, data.Length);
        }

        /// <summary>
        /// Disconnect peer from server and send additional data (Size must be less or equal MTU - 8)
        /// </summary>
        /// <param name="peer">peer to disconnect</param>
        /// <param name="writer">additional data</param>
        public void DisconnectPeer(UdpPeer peer, UdpDataWriter writer)
        {
            DisconnectPeer(peer, writer.Data, 0, writer.Length);
        }

        /// <summary>
        /// Disconnect peer from server and send additional data (Size must be less or equal MTU - 8)
        /// </summary>
        /// <param name="peer">peer to disconnect</param>
        /// <param name="data">additional data</param>
        /// <param name="start">data start</param>
        /// <param name="count">data length</param>
        public void DisconnectPeer(UdpPeer peer, byte[] data, int start, int count)
        {
            if (peer != null && this.peers.Contains(peer.EndPoint))
            {
                DisconnectPeer(peer, DisconnectReason.DisconnectPeerCalled, 0, true, data, start, count);
            }
        }

#region Events

        private void CreateDisconnectEvent(UdpPeer peer, DisconnectReason reason, int socketErrorCode)
        {
            var netEvent = this.CreateEvent(UdpEventType.Disconnect);
            netEvent.Peer = peer;
            netEvent.AdditionalData = socketErrorCode;
            netEvent.DisconnectReason = reason;
            this.EnqueueEvent(netEvent);
        }

        private void CreateDisconnectEventWithData(UdpPeer udpPeer, UdpPacket packet)
        {
            var netEvent = this.CreateEvent(UdpEventType.Disconnect);
            netEvent.Peer = udpPeer;
            netEvent.DataReader.SetSource(packet.RawData, 5, packet.Size - 5);
            netEvent.DisconnectReason = DisconnectReason.RemoteConnectionClose;
            this.EnqueueEvent(netEvent);
        }

        private void CreateReceiveUnconnectedEvent(UdpEndPoint remoteEndPoint, UdpPacket packet, int count)
        {
            UdpEvent netEvent = this.CreateEvent(UdpEventType.ReceiveUnconnected);
            netEvent.RemoteEndPoint = remoteEndPoint;
            netEvent.DataReader.SetSource(packet.RawData, HeaderSize.DEFAULT, count);
            this.EnqueueEvent(netEvent);
        }

        private void CreateReceiveEvent(UdpPacket packet, ChannelType channel, UdpPeer fromPeer)
        {
            var netEvent = this.CreateEvent(UdpEventType.Receive);
            netEvent.Peer = fromPeer;
            netEvent.RemoteEndPoint = fromPeer.EndPoint;
            netEvent.DataReader.SetSource(packet.GetPacketData());
            netEvent.Channel = channel;
            this.EnqueueEvent(netEvent);
        }

        private void CreateReceiveAckEvent(UdpPacket packet, ChannelType channel, UdpPeer fromPeer)
        {
            var netEvent = this.CreateEvent(UdpEventType.ReceiveAck);
            netEvent.Peer = fromPeer;
            netEvent.RemoteEndPoint = fromPeer.EndPoint;
            netEvent.DataReader.SetSource(packet.GetPacketData());
            netEvent.Channel = channel;
            this.EnqueueEvent(netEvent);
        }

        public void CreateConnectEvent(UdpPeer peer)
        {
            var connectEvent = CreateEvent(UdpEventType.Connect);
            connectEvent.Peer = peer;
            peer.EnqueueEvent(connectEvent);
        }

        private void CreateLatencyUpdateEvent(UdpPeer fromPeer, int latency)
        {
            var evt = this.CreateEvent(UdpEventType.ConnectionLatencyUpdated);
            evt.Peer = fromPeer;
            evt.AdditionalData = latency;
            this.EnqueueEvent(evt);
        }

        private void CreateErrorEvent(UdpEndPoint remoteEndPoint, int errorCode)
        {
            var netEvent = this.CreateEvent(UdpEventType.Error);
            netEvent.RemoteEndPoint = remoteEndPoint;
            netEvent.AdditionalData = errorCode;
            this.EnqueueEvent(netEvent);
        }

        public UdpEvent CreateEvent(UdpEventType type)
        {
            UdpEvent evt = null;

            lock (this.netEventsPool)
            {
                if (this.netEventsPool.Count > 0)
                {
                    evt = this.netEventsPool.Pop();
                }
            }
            if (evt == null)
            {
                evt = new UdpEvent();
            }
            evt.Type = type;
            return evt;
        }

        public void EnqueueEvent(UdpEvent evt)
        {
            this.netEventsQueue.Enqueue(evt);
        }

        private void ProcessEvent(UdpEvent evt)
        {
            switch (evt.Type)
            {
                case UdpEventType.Connect:
                    this.netEventListener.OnPeerConnected(evt.Peer);
                    break;
                case UdpEventType.Disconnect:
                    var info = new DisconnectInfo
                    {
                        Reason = evt.DisconnectReason,
                        AdditionalData = evt.DataReader,
                        SocketErrorCode = evt.AdditionalData
                    };
                    this.netEventListener.OnPeerDisconnected(evt.Peer, info);
                    break;
                case UdpEventType.Receive:
                    this.netEventListener.OnNetworkReceive(evt.Peer, evt.DataReader, evt.Channel);
                    break;
                case UdpEventType.ReceiveUnconnected:
                    this.netEventListener.OnNetworkReceiveUnconnected(evt.RemoteEndPoint, evt.DataReader);
                    break;
                case UdpEventType.ReceiveAck:
                    this.netEventListener.OnNetworkReceiveAck(evt.Peer, evt.DataReader, evt.Channel);
                    break;
                case UdpEventType.Error:
                    this.netEventListener.OnNetworkError(evt.RemoteEndPoint, evt.AdditionalData);
                    break;
                case UdpEventType.ConnectionLatencyUpdated:
                    this.netEventListener.OnNetworkLatencyUpdate(evt.Peer, evt.AdditionalData);
                    break;
            }

            evt.DataReader.Clear();
            evt.Peer = null;
            evt.AdditionalData = 0;
            evt.RemoteEndPoint = null;

            lock (this.netEventsPool)
            {
                this.netEventsPool.Push(evt);
            }
        }
#endregion
    }

}

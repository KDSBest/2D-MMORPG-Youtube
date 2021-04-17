
using ReliableUdp.BitUtility;
using ReliableUdp.Const;
using ReliableUdp.Enums;
using ReliableUdp.Packet;
using System;

namespace ReliableUdp.PacketHandler
{
    public class MtuHandler
    {
        private int mtuIdx;
        private bool finishMtu;
        private int mtuCheckTimer;
        private int mtuCheckAttempts;
        private const int MTU_CHECK_DELAY = 1000;
        private const int MAX_MTU_CHECK_ATTEMPTS = 4;
        private readonly object lockObject = new object();

        public int Mtu { get; private set; } = Const.Mtu.PossibleValues[0];

        public MtuHandler()
        {
        }

        public void ProcessMtuPacket(UdpPeer peer, UdpPacket packet)
        {
            if (packet.Size == 5 || packet.RawData[1] >= Const.Mtu.PossibleValues.Length)
                return;

            int recvMtu = BitConverter.ToInt32(packet.RawData, 1);
            int endMtuCheck = BitConverter.ToInt32(packet.RawData, packet.Size - 4);
            if (packet.Size != recvMtu || recvMtu != endMtuCheck || recvMtu > Const.Mtu.MaxPacketSize)
            {
#if UDP_DEBUGGING
                System.Diagnostics.Debug.WriteLine($"Corrupted MTU Package Recv MTU: {recvMtu} End MTU: {endMtuCheck} Packet Size: {packet.Size}");
#endif
                return;
            }

            if (packet.Type == PacketType.MtuCheck)
            {
                this.mtuCheckAttempts = 0;

#if UDP_DEBUGGING
                System.Diagnostics.Debug.WriteLine($"MTU check. Resend {packet.RawData[1]}");
#endif
                packet.Type = PacketType.MtuOk;
                peer.SendPacket(packet);
            }
            else if (recvMtu > Mtu && !finishMtu)
            {
                if (recvMtu != Const.Mtu.PossibleValues[mtuIdx + 1])
                    return;

                lock (this.lockObject)
                {
                    this.mtuIdx++;
                    this.Mtu = recvMtu;
                }

                if (this.mtuIdx == Const.Mtu.PossibleValues.Length - 1)
                    this.finishMtu = true;

#if UDP_DEBUGGING
                System.Diagnostics.Debug.WriteLine($"MTU is set to {this.Mtu}");
#endif
            }
        }

        public void Update(UdpPeer peer, int deltaTime)
        {
            if (this.finishMtu)
                return;

            this.mtuCheckTimer += deltaTime;
            if (this.mtuCheckTimer < MTU_CHECK_DELAY)
                return;

            this.mtuCheckTimer = 0;
            this.mtuCheckAttempts++;
            if (this.mtuCheckAttempts >= MAX_MTU_CHECK_ATTEMPTS)
            {
                this.finishMtu = true;
                return;
            }

            lock (this.lockObject)
            {
                if (this.mtuIdx >= Const.Mtu.PossibleValues.Length - 1)
                    return;

                int newMtu = Const.Mtu.PossibleValues[this.mtuIdx + 1];
                var p = peer.GetPacketFromPool(PacketType.MtuCheck, newMtu - HeaderSize.DEFAULT);
                BitHelper.Write(p.RawData, 1, newMtu);
                BitHelper.Write(p.RawData, p.Size - 4, newMtu);

                this.SendPacket(peer, p);
            }
        }

        public bool SendPacket(UdpPeer peer, UdpPacket packet)
        {
            bool result = peer.SendRawAndRecycle(packet, peer.EndPoint);
            if (!result)
            {
                this.finishMtu = true;
            }

            return result;
        }
    }
}

using Common.Protocol;
using ReliableUdp;
using ReliableUdp.Enums;
using ReliableUdp.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Extensions
{
	public static class UdpManagerExtensions
	{
        public static void SendMsg(this UdpManager udpManager, long id, IUdpPackage msg, ChannelType channelType)
        {
            if (udpManager == null)
                return;

            UdpDataWriter writer = new UdpDataWriter();
            msg.Write(writer);

            udpManager.SendTo(id, writer, channelType);
        }

        public static void SendMsg(this UdpManager udpManager, IUdpPackage msg, ChannelType channelType)
        {
            if (udpManager == null)
                return;

            UdpDataWriter writer = new UdpDataWriter();
            msg.Write(writer);

            udpManager.SendToAll(writer, channelType);
        }
    }
}

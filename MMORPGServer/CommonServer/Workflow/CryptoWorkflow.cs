using Common.Crypto;
using Common.Protocol.Crypto;
using Common.Extensions;
using ReliableUdp;
using ReliableUdp.Enums;
using ReliableUdp.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Workflow;

namespace CommonServer.Workflow
{
	public class CryptoWorkflow<T> : ICryptoWorkflow where T : ICryptoWorkflow, new()
	{
		public CryptoProvider Crypto { get; set; } = new CryptoProvider();
		public UdpManager UdpManager { get; set; }
		public Action<UdpPeer, IWorkflow> SwitchWorkflow { get; set; }
		private UdpPeer peer;

		public void OnDisconnected(DisconnectInfo disconnectInfo)
		{
		}

		public void OnLatencyUpdate(int latency)
		{
		}

		public void OnReceive(UdpDataReader reader, ChannelType channel)
		{
			AESParameterMessage aesParameterMsg = new AESParameterMessage();
			if(aesParameterMsg.Read(reader))
			{
				Crypto.DecryptAesParameter(aesParameterMsg.AESParameter);

				ICryptoWorkflow wf = new T();
				wf.Crypto = this.Crypto;
				SwitchWorkflow(this.peer, wf);
			}
		}

		public void OnStart(UdpPeer peer)
		{
			this.peer = peer;
			var rsaPublicMsg = new RSAPublicKeyMessage();
			rsaPublicMsg.PublicKey = Crypto.GetPublicCspBlob();
			this.UdpManager.SendMsg(peer.ConnectId, rsaPublicMsg, ChannelType.ReliableOrdered);
		}
	}
}

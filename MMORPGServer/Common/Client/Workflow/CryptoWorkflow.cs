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

namespace Common.Client.Workflow
{
	public class CryptoWorkflow<T> : ICryptoWorkflow where T : ICryptoWorkflow, new()
	{
		public CryptoProvider Crypto { get; set; }
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
			RSAPublicKeyMessage rsaPublicMsg = new RSAPublicKeyMessage();
			if(rsaPublicMsg.Read(reader))
			{
				Crypto = new CryptoProvider(rsaPublicMsg.PublicKey);

				AESParameterMessage aesParameterMessage = new AESParameterMessage();
				aesParameterMessage.AESParameter = Crypto.EncryptAesParameter();

				this.UdpManager.SendMsg(aesParameterMessage, ChannelType.ReliableOrdered);

				ICryptoWorkflow wf = new T();
				wf.Crypto = this.Crypto;
				SwitchWorkflow(this.peer, wf);
			}
		}

		public void OnStart(UdpPeer peer)
		{
			this.peer = peer;
		}
	}
}

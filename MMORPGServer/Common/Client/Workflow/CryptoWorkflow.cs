using Common.Crypto;
using Common.Protocol.Crypto;
using Common.Extensions;
using ReliableUdp;
using ReliableUdp.Enums;
using ReliableUdp.Utility;
using System;
using System.Threading.Tasks;
using Common.Workflow;

namespace Common.Client.Workflow
{
	public class CryptoWorkflow<T> : ICryptoWorkflow where T : ICryptoWorkflow, new()
	{
		public CryptoProvider Crypto { get; set; }
		public UdpManager UdpManager { get; set; }
		public Func<UdpPeer, IWorkflow, Task> SwitchWorkflowAsync { get; set; }
		private UdpPeer peer;

		public async Task OnDisconnectedAsync(DisconnectInfo disconnectInfo)
		{
		}

		public async Task OnLatencyUpdateAsync(int latency)
		{
		}


		public async Task OnReceiveAsync(UdpDataReader reader, ChannelType channel)
		{
			RSAPublicKeyMessage rsaPublicMsg = new RSAPublicKeyMessage();
			if(rsaPublicMsg.Read(reader))
			{
				Crypto = new CryptoProvider(rsaPublicMsg.PublicKey);

				AESParameterMessage aesParameterMessage = new AESParameterMessage
				{
					AESParameter = Crypto.EncryptAesParameter()
				};

				this.UdpManager.SendMsg(aesParameterMessage, ChannelType.ReliableOrdered);

				ICryptoWorkflow wf = new T
				{
					Crypto = this.Crypto
				};
				await SwitchWorkflowAsync(this.peer, wf);
			}
		}

		public async Task OnStartAsync(UdpPeer peer)
		{
			this.peer = peer;
		}
	}
}

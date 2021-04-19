using Common.Crypto;
using Common.Protocol.Crypto;
using Common.Extensions;
using ReliableUdp;
using ReliableUdp.Enums;
using ReliableUdp.Utility;
using System;
using System.Threading.Tasks;
using Common.Workflow;

namespace CommonServer.Workflow
{
	public class CryptoWorkflow<T> : ICryptoWorkflow where T : ICryptoWorkflow, new()
	{
		public CryptoProvider Crypto { get; set; } = new CryptoProvider();
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
			AESParameterMessage aesParameterMsg = new AESParameterMessage();
			if(aesParameterMsg.Read(reader))
			{
				Crypto.DecryptAesParameter(aesParameterMsg.AESParameter);

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
			var rsaPublicMsg = new RSAPublicKeyMessage
			{
				PublicKey = Crypto.GetPublicCspBlob()
			};
			this.UdpManager.SendMsg(peer.ConnectId, rsaPublicMsg, ChannelType.ReliableOrdered);
		}
	}
}

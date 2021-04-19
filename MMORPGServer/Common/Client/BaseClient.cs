using Common.Extensions;
using Common.Protocol;
using Common.Protocol.Crypto;
using Common.Udp;
using Common.Workflow;
using ReliableUdp;
using ReliableUdp.Enums;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Common.Client
{
	public class BaseClient<TWorkflow> : BaseUdpListener<TWorkflow> where TWorkflow : IWorkflow, new()
	{
		public UdpPeer Peer { get; set; }
		private const int delayMs = 50;
		private const int maxWaitMs = 5000;
		private int currentMaxWait = maxWaitMs;
		private bool disconnect = false;
		private Task updateThread;

		public bool IsConnected
		{
			get
			{
				return Peer.ConnectionState == ConnectionState.Connected && !disconnect;
			}
		}

		public async Task<bool> ConnectAsync(string host = "localhost", int port = 3334, string token = "")
		{
			return await Task.Run<bool>(async () =>
			{
				this.UdpManager = new UdpManager(this, ProtocolConstants.ConnectionKey);
				this.Peer = this.UdpManager.Connect(host, port);

				while (this.Peer.ConnectionState == ConnectionState.InProgress && currentMaxWait > 0)
				{
					Console.WriteLine("Connecting...");
					await Task.Delay(delayMs);

					currentMaxWait -= delayMs;
					await this.UdpManager.PollEventsAsync();
				}

				if (this.Peer.ConnectionState == ConnectionState.Connected)
				{
					Console.WriteLine("Connected!");

					if(!string.IsNullOrEmpty(token))
					{
						var jwtMsg = new JwtMessage
						{
							Token = token
						};
						this.UdpManager.SendMsg(jwtMsg, ChannelType.Reliable);
						Console.WriteLine("Jwt Token send.");
					}
				}
				else
				{
					Console.WriteLine("Connection failed!");
					return false;
				}

				updateThread = PollEventsAsync();

				return true;
			});
		}

		public async Task PollEventsAsync()
		{
			while (Peer.ConnectionState == ConnectionState.Connected && !disconnect)
			{
				await Task.Delay(delayMs);
				await this.UdpManager.PollEventsAsync();
			}
		}

		public async Task DisconnectAsync()
		{
			disconnect = true;
			updateThread.Wait(maxWaitMs);
		}

		protected void WaitForResponse(Func<bool> condition)
		{
			var cts = new CancellationTokenSource();
			var token = cts.Token;
			Task waitingForResponse = WaitForAsync(condition, token);
			if (!waitingForResponse.Wait(maxWaitMs))
			{
				cts.Cancel();
			}
		}

		protected async Task WaitForAsync(Func<bool> condition, CancellationToken token)
		{
			while (!condition() && !token.IsCancellationRequested)
			{
				await Task.Delay(delayMs);
			}
		}
	}
}

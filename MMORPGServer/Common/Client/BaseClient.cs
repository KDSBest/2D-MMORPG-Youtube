using Common.Client.Interfaces;
using Common.Protocol;
using Common.Udp;
using Common.Workflow;
using ReliableUdp;
using ReliableUdp.Enums;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Common.Client
{
	public class BaseClient<TWorkflow> : BaseUdpListener<TWorkflow>, IBaseClient<TWorkflow> where TWorkflow : class, IWorkflow, new()
	{
		public UdpPeer Peer { get; set; }
		private const int delayMs = 50;
		private const int maxWaitMs = 5000;
		private int currentMaxWait = maxWaitMs;
		private bool disconnect = false;
		private Task updateThread;

		public TWorkflow Workflow { get; set; }

		public virtual bool IsConnected
		{
			get
			{
				return Peer != null && Peer.ConnectionState == ConnectionState.Connected && !disconnect && Workflow != null;
			}
		}

		public async Task<bool> ConnectAsync(string host = "localhost", int port = 3334)
		{
			return await Task.Run<bool>(async () =>
			{
				this.UdpManager = new UdpManager(this);

				if(BaseClientSettings.Cert != null)
				{
					this.UdpManager.Settings.Cert = BaseClientSettings.Cert;
				}

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

		protected async Task WaitForAsync(Func<bool> condition, CancellationToken token)
		{
			while (!condition() && !token.IsCancellationRequested)
			{
				await Task.Delay(delayMs);
			}
		}

		public override void OnWorkflowSwitch(UdpPeer peer, IWorkflow newWorkflow)
		{
			var wf = newWorkflow as TWorkflow;

			if(wf != null)
				Workflow = wf;
		}
	}
}

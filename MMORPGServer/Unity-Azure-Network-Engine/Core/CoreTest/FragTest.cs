using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;

using ReliableUdp;
using ReliableUdp.Enums;
using ReliableUdp.Utility;

namespace CoreTest
{
	[TestClass]
	public class FragTest
	{
		private class ClientListener : IUdpEventListener
		{
			public bool IsTestRecOk { get; set; }
			public bool IsTestAckOk { get; set; }

			public UdpManager UdpManager { get; set; }

			public void OnPeerConnected(UdpPeer peer)
			{
				Console.WriteLine("[Client] connected to: {0}:{1}", peer.EndPoint.Host, peer.EndPoint.Port);

				byte[] testData = new byte[13218];
				testData[0] = 192;
				testData[13217] = 31;
				peer.Send(testData, ChannelType.ReliableOrdered);
			}

			public void OnPeerDisconnected(UdpPeer peer, DisconnectInfo disconnectInfo)
			{
				Console.WriteLine("[Client] disconnected: " + disconnectInfo.Reason);
			}

			public void OnNetworkError(UdpEndPoint endPoint, int socketErrorCode)
			{
				Console.WriteLine("[Client] error! " + socketErrorCode);
			}

			public void OnNetworkReceive(UdpPeer peer, UdpDataReader reader, ChannelType channel)
			{
				if (reader.AvailableBytes == 13218)
				{
					Console.WriteLine("[{0}] TestFrag: {1}, {2}", peer.UdpManager.LocalEndPoint.Port, reader.Data[0], reader.Data[13217]);
					this.IsTestRecOk = true;
				}
				else
				{
					Assert.Fail("Wrong Byte Count");
				}
			}

			public void OnNetworkReceiveAck(UdpPeer peer, UdpDataReader reader, ChannelType channel)
			{
				peer.Send(reader.Data, ChannelType.Reliable);

				if (reader.AvailableBytes == 13218)
				{
					Console.WriteLine("[Client] Ack TestFrag: {0}, {1}", reader.Data[0], reader.Data[13217]);
					this.IsTestAckOk = true;
				}
				else
				{
					Assert.Fail("Wrong Byte Count");
				}
			}

			public void OnNetworkReceiveUnconnected(UdpEndPoint remoteEndPoint, UdpDataReader reader)
			{

			}

			public void OnNetworkLatencyUpdate(UdpPeer peer, int latency)
			{

			}
		}

		private class ServerListener : IUdpEventListener
		{
			public bool IsTestRecOk { get; set; }
			public bool IsTestAckOk { get; set; }

			public UdpManager UdpManager { get; set; }
			public UdpManager Server;

			public void OnPeerConnected(UdpPeer peer)
			{
				Console.WriteLine("[Server] Peer connected: " + peer.EndPoint);
				var peers = Server.GetPeers();
				foreach (var netPeer in peers)
				{
					Console.WriteLine("ConnectedPeersList: id={0}, ep={1}", netPeer.ConnectId, netPeer.EndPoint);
				}
			}

			public void OnPeerDisconnected(UdpPeer peer, DisconnectInfo disconnectInfo)
			{
				Console.WriteLine("[Server] Peer disconnected: " + peer.EndPoint + ", reason: " + disconnectInfo.Reason);
			}

			public void OnNetworkError(UdpEndPoint endPoint, int socketErrorCode)
			{
				Console.WriteLine("[Server] error: " + socketErrorCode);
			}

			public void OnNetworkReceive(UdpPeer peer, UdpDataReader reader, ChannelType channel)
			{
				peer.Send(reader.Data, ChannelType.Reliable);

				if (reader.AvailableBytes == 13218)
				{
					Console.WriteLine("[Server] TestFrag: {0}, {1}", reader.Data[0], reader.Data[13217]);
					this.IsTestRecOk = true;
				}
				else
					Assert.Fail("Wrong Byte Count");
			}

			public void OnNetworkReceiveAck(UdpPeer peer, UdpDataReader reader, ChannelType channel)
			{
				if (reader.AvailableBytes == 13218)
				{
					Console.WriteLine("[{0}] Ack TestFrag: {1}, {2}", peer.UdpManager.LocalEndPoint.Port, reader.Data[0], reader.Data[13217]);
					this.IsTestAckOk = true;
				}
				else
				{
					Assert.Fail("Wrong Byte Count");
				}
			}

			public void OnNetworkReceiveUnconnected(UdpEndPoint remoteEndPoint, UdpDataReader reader)
			{
				Console.WriteLine("[Server] ReceiveUnconnected: {0}", reader.GetString(100));
			}

			public void OnNetworkLatencyUpdate(UdpPeer peer, int latency)
			{

			}
		}

		private ClientListener clientListener;
		private ServerListener serverListener;

		[TestMethod]
		public void TestFrag()
		{
			this.serverListener = new ServerListener();

			UdpManager server = new UdpManager(this.serverListener, "myapp1", 2);
			if (!server.Start(9050))
			{
				Console.WriteLine("Server start failed");
				Assert.Fail("Server start failed");
			}
			this.serverListener.Server = server;

			this.clientListener = new ClientListener();

			UdpManager client1 = new UdpManager(this.clientListener, "myapp1");
			if (!client1.Start())
			{
				Console.WriteLine("Client1 start failed");
				return;
			}
			client1.Connect("127.0.0.1", 9050);

			bool allOk = false;

			for (int i = 0; i < 2000 && !allOk; i++)
			{
				client1.PollEvents();
				server.PollEvents();
				allOk = this.clientListener.IsTestRecOk && this.clientListener.IsTestAckOk && this.serverListener.IsTestAckOk && this.serverListener.IsTestAckOk;
				Thread.Sleep(15);
			}

			client1.Stop();
			server.Stop();
		}
	}
}

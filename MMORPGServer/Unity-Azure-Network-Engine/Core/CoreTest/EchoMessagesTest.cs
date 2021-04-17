using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;

using ReliableUdp;
using ReliableUdp.Enums;
using ReliableUdp.Utility;

namespace CoreTest
{
	[TestClass]
	public class EchoMessagesTest
	{
		private static int messagesReceivedCount = 0;
		private static int clientMessagesReceivedCount = 0;
		private static int serverMessagesReceivedCount = 0;

		private class ClientListener : IUdpEventListener
		{
			public UdpManager UdpManager { get; set; }

			public void OnPeerConnected(UdpPeer peer)
			{
				Console.WriteLine("[Client] connected to: {0}:{1}", peer.EndPoint.Host, peer.EndPoint.Port);

				UdpDataWriter dataWriter = new UdpDataWriter();
				for (int i = 0; i < 5; i++)
				{
					dataWriter.Reset();
					dataWriter.Put(0);
					dataWriter.Put(i);
					peer.Send(dataWriter, ChannelType.Reliable);

					dataWriter.Reset();
					dataWriter.Put(1);
					dataWriter.Put(i);
					peer.Send(dataWriter, ChannelType.ReliableOrdered);

					dataWriter.Reset();
					dataWriter.Put(2);
					dataWriter.Put(i);
					peer.Send(dataWriter, ChannelType.UnreliableOrdered);

					dataWriter.Reset();
					dataWriter.Put(3);
					dataWriter.Put(i);
					peer.Send(dataWriter, ChannelType.Unreliable);
				}
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
				int type = reader.GetInt();
				int num = reader.GetInt();
				Assert.IsTrue(num < 5 && num >= 0);

				messagesReceivedCount++;
				clientMessagesReceivedCount++;
				Console.WriteLine("[{0}] CNT: {1}, TYPE: {2}, NUM: {3}", peer.UdpManager.LocalEndPoint.Port, messagesReceivedCount, type, num);
			}

			public void OnNetworkReceiveAck(UdpPeer peer, UdpDataReader reader, ChannelType channel)
			{
				int type = reader.GetInt();
				int num = reader.GetInt();
				Assert.IsTrue(num < 5 && num >= 0);

				messagesReceivedCount++;
				clientMessagesReceivedCount++;
				Console.WriteLine("[{0}] Ack CNT: {1}, TYPE: {2}, NUM: {3}", peer.UdpManager.LocalEndPoint.Port, messagesReceivedCount, type, num);
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
			public UdpManager UdpManager { get; set; }

			public void OnPeerConnected(UdpPeer peer)
			{
				Console.WriteLine("[Server] Peer connected: " + peer.EndPoint);
				var peers = this.UdpManager.GetPeers();
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
				messagesReceivedCount++;
				serverMessagesReceivedCount++;
				peer.Send(reader.Data, channel);
			}

			public void OnNetworkReceiveAck(UdpPeer peer, UdpDataReader reader, ChannelType channel)
			{
				int type = reader.GetInt();
				int num = reader.GetInt();
				Assert.IsTrue(num < 5 && num >= 0);

				messagesReceivedCount++;
				serverMessagesReceivedCount++;
				Console.WriteLine("[{0}] Ack CNT: {1}, TYPE: {2}, NUM: {3}", peer.UdpManager.LocalEndPoint.Port, messagesReceivedCount, type, num);
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
		public void TestEcho()
		{
			messagesReceivedCount = 0;
			clientMessagesReceivedCount = 0;
			serverMessagesReceivedCount = 0;

			this.serverListener = new ServerListener();

			UdpManager server = new UdpManager(this.serverListener, "myapp1", 2);
			if (!server.Start(9050))
			{
				Console.WriteLine("Server start failed");
				Assert.Fail("Server start failed");
			}
	
			this.clientListener = new ClientListener();

			UdpManager client1 = new UdpManager(this.clientListener, "myapp1");
			if (!client1.Start())
			{
				Console.WriteLine("Client1 start failed");
				return;
			}
			client1.Connect("127.0.0.1", 9050);

			UdpManager client2 = new UdpManager(this.clientListener, "myapp1");
			client2.Start();
			client2.Connect("::1", 9050);

			for (int i = 0; i < 1000 && messagesReceivedCount != 120; i++)
			{
				client1.PollEvents();
				client2.PollEvents();
				server.PollEvents();
				Thread.Sleep(15);
			}

			client1.Stop();
			client2.Stop();
			server.Stop();

			Console.WriteLine("ServStats:\n BytesReceived: {0}\n PacketsReceived: {1}\n BytesSent: {2}\n PacketsSent: {3}",
				 server.BytesReceived,
				 server.PacketsReceived,
				 server.BytesSent,
				 server.PacketsSent);
			Console.WriteLine("Client1Stats:\n BytesReceived: {0}\n PacketsReceived: {1}\n BytesSent: {2}\n PacketsSent: {3}",
				 client1.BytesReceived,
				 client1.PacketsReceived,
				 client1.BytesSent,
				 client1.PacketsSent);
			Console.WriteLine("Client2Stats:\n BytesReceived: {0}\n PacketsReceived: {1}\n BytesSent: {2}\n PacketsSent: {3}",
				 client2.BytesReceived,
				 client2.PacketsReceived,
				 client2.BytesSent,
				 client2.PacketsSent);

			Assert.AreEqual(120, messagesReceivedCount);
			Assert.AreEqual(serverMessagesReceivedCount, clientMessagesReceivedCount);
		}
	}
}

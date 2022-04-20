using Common;
using Common.IoC;
using Common.PublishSubscribe;
using CommonServer.GameDesign;
using CommonServer.ServerModel;
using CommonServer.WorldManagement;
using System;
using System.Threading.Tasks;

namespace EnemyWorkerService
{
	public class Program
	{
		public static async Task Main(string[] args)
		{
			DI.Instance.Register<IPubSub>(() => new PubSub(), RegistrationType.Singleton);
			DI.Instance.Register<IPlayerWorldManagement>(() => new PlayerWorldManagement(), RegistrationType.Singleton);

			string servername = $"E*{MapConfiguration.MapName}";
			Console.WriteLine($"Start Load Balancing Worker {servername}.");
			var server = new EnemyLoadBalancerWorker(servername);

			server.Start();

			while (server.IsRunning)
			{
				await Task.Delay(100);
			}
		}
	}
}

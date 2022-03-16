using Common;
using CommonServer.GameDesign;
using CommonServer.ServerModel;
using System;
using System.Threading.Tasks;

namespace EnemyWorkerService
{
	public class Program
	{
		public static async Task Main(string[] args)
		{
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

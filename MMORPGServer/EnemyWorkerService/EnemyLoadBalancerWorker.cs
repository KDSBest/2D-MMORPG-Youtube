using CommonServer.GameDesign;
using CommonServer.ServerModel;
using System;
using System.Threading.Tasks;

namespace EnemyWorkerService
{
	public class EnemyLoadBalancerWorker : LoadBalancerWorker<EnemyLoadEntry>
	{
		private EnemyManagement enemyManagement;

		public EnemyLoadBalancerWorker(string redisKeyNamePrefix) : base(redisKeyNamePrefix)
		{
			enemyManagement = new EnemyManagement();
		}

		protected override async Task HandleLoad(EnemyLoadEntry jobContext)
		{
			Console.WriteLine($"Handle {jobContext.Name}");
			this.enemyManagement.Update(this.UpdateDelay, jobContext);
		}
	}
}

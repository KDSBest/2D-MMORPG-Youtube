using CommonServer.GameDesign;
using CommonServer.ServerModel;
using System;
using System.Threading.Tasks;

namespace EnemyWorkerService
{
	public class EnemyLoadBalancerWorker : LoadBalancerWorker<EnemyJob>
	{
		private EnemyManagement enemyManagement;

		public EnemyLoadBalancerWorker(string redisKeyNamePrefix) : base(redisKeyNamePrefix)
		{
			enemyManagement = new EnemyManagement();
		}

		protected override async Task HandleLoad(EnemyJob jobContext)
		{
			await this.enemyManagement.Update(this.UpdateDelay, jobContext);
		}
	}
}

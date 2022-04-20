using Common.IoC;
using Common.Protocol.Map;
using Common.PublishSubscribe;
using CommonServer.GameDesign;
using CommonServer.ServerModel;
using CommonServer.WorldManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EnemyWorkerService
{
	public class EnemyLoadBalancerWorker : LoadBalancerWorker<EnemyJob>
	{
		private EnemyManagement enemyManagement;
		private IPlayerWorldManagement worldManagement;
		private List<PlayerStateMessage> world = new List<PlayerStateMessage>();

		public EnemyLoadBalancerWorker(string redisKeyNamePrefix) : base(redisKeyNamePrefix)
		{
			enemyManagement = new EnemyManagement();
			worldManagement = DI.Instance.Resolve<IPlayerWorldManagement>();
			worldManagement.Initialize(false, false);
		}
		protected override void PrepareUpdate()
		{
			world = worldManagement.LastState.Values.Where(x => x is PlayerStateMessage).Cast<PlayerStateMessage>().ToList();
		}

		protected override async Task HandleLoad(EnemyJob jobContext)
		{
			await this.enemyManagement.Update(this.UpdateDelay, jobContext, world);
		}
	}
}

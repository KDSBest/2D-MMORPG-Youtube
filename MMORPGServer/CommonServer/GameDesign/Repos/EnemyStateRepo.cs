using Common;
using Common.Protocol.Map;
using CommonServer.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonServer.GameDesign.Repos
{
	public class EnemyStateRepo
	{
		public string GetEnemyRedisName(string enemyName)
		{
			return $"{MapConfiguration.MapName}_{enemyName}";
		}

		public EnemyStateMessage Get(string name)
		{
			return RedisKV.Get<EnemyStateMessage>(GetEnemyRedisName(name));
		}

		public void Save(string name, EnemyStateMessage enemy)
		{
			RedisKV.Set<EnemyStateMessage>(GetEnemyRedisName(name), enemy);
		}
	}
}

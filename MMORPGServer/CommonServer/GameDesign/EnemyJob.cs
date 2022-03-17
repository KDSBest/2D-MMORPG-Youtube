using Common.GameDesign;
using CommonServer.ServerModel.Repos;

namespace CommonServer.GameDesign
{
	public class EnemyJob : INameable
	{
		public string Name { get; set; }
		public EnemySpawnConfig Config { get; set; }
	}
}

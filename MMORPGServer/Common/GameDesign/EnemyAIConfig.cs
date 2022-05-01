using System.Collections.Generic;

namespace Common.GameDesign
{
	public class EnemyAIConfig
	{
		public Dictionary<SkillCastType, int> Skills = new Dictionary<SkillCastType, int>();
		public List<EnemyAICastPriority> CastPriority = new List<EnemyAICastPriority>();
	}
}

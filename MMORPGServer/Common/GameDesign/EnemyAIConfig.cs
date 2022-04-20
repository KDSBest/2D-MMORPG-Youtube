using System.Collections.Generic;

namespace Common.GameDesign
{
	public class EnemyAIConfig
	{
		public Dictionary<SkillCastType, int> Skills = new Dictionary<SkillCastType, int>();
		public List<SkillCastType> CastOrder = new List<SkillCastType>();
	}
}

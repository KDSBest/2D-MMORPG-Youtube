using System;
using System.Collections.Generic;
using System.Text;

namespace Common.GameDesign
{
	public static class GameDesignConfiguration
	{
		public static GameDesignSkills Skills = new GameDesignSkills();

		public static Dictionary<SkillCastType, int> AnimationDelay = new Dictionary<SkillCastType, int>()
		{
			{ SkillCastType.Fireball, 1000  },
			{ SkillCastType.LightningBolt, 700  }
		};

		public static Dictionary<SkillCastType, int> SkillIndicatorDelay = new Dictionary<SkillCastType, int>()
		{
			{ SkillCastType.Boss1Attack1, 5000  },
			{ SkillCastType.Boss1Attack2, 5000  }
		};

		public static int CooldownLatencyAllowance = 100;
	}
}

﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Common.GameDesign
{
	public static class GameDesignConfiguration
	{
		public static Dictionary<SkillCastType, int> Cooldowns = new Dictionary<SkillCastType, int>()
		{
			{ SkillCastType.Fireball, 10000  },
			{ SkillCastType.LightningBolt, 5000  },
		};

		public static Dictionary<SkillCastType, int> AnimationDelay = new Dictionary<SkillCastType, int>()
		{
			{ SkillCastType.Fireball, 1000  },
			{ SkillCastType.LightningBolt, 1000  },
		};

		public static int CooldownLatencyAllowance = 100;


	}
}
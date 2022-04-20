using Common.GameDesign;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonServer.GameDesign
{
	public class CooldownManagement
	{
		private Dictionary<SkillCastType, DateTime> usedSkill = new Dictionary<SkillCastType, DateTime>();

		public bool CanCast(SkillCastType type, int level, int cooldownLatencyAllowance = 0)
		{
			if (usedSkill.ContainsKey(type))
			{
				var elapsedTime = (DateTime.UtcNow - usedSkill[type]).TotalMilliseconds + cooldownLatencyAllowance;
				int cd = GameDesignConfiguration.Skills.SkillTable[type].GetStats(level).Cooldown;

				return elapsedTime >= cd;
			}

			return true;
		}

		public void Cast(SkillCastType type)
		{
			usedSkill[type] = DateTime.UtcNow;
		}
	}
}

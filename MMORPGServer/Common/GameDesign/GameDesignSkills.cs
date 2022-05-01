using System.Collections.Generic;

namespace Common.GameDesign
{
	public class GameDesignSkills
	{
		public Dictionary<SkillCastType, SkillProgressionTable> SkillTable = new Dictionary<SkillCastType, SkillProgressionTable>()
		{
			{
				SkillCastType.LightningBolt, new SkillProgressionTable()
				{
					{
						1, new SkillStats()
						{
							Cooldown = 1000,
							IsMagic = true,
							SkillDamagePercent = 1
						}
					}
				}
			},
			{
				SkillCastType.Fireball, new SkillProgressionTable()
				{
					{
						1, new SkillStats()
						{
							Cooldown = 15000,
							IsMagic = true,
							SkillDamagePercent = 10
						}
					}
				}
			},
			{
				SkillCastType.Boss1Attack1, new SkillProgressionTable()
				{
					{
						1, new SkillStats()
						{
							Cooldown = 12000,
							IsMagic = true,
							SkillDamagePercent = 1
						}
					}
				}
			},
			{
				SkillCastType.Boss1Attack2, new SkillProgressionTable()
				{
					{
						1, new SkillStats()
						{
							Cooldown = 12000,
							IsMagic = true,
							SkillDamagePercent = 1
						}
					}
				}
			}
		};
	}
}

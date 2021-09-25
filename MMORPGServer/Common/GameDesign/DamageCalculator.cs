using Common.Protocol.Combat;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common.GameDesign
{

	public class DamageCalculator
	{
		public const float AttackOverDefenseConst = 1.5f;
		public const float LevelDiffImpact = 2.5f;
		private Random rnd = new Random();

		public DamageInfo GetDamage(EntityStats caster, EntityStats receiver, SkillStats skill)
		{
			int damage = 1;

			int atk = skill.IsMagic ? caster.MAttack : caster.Attack;
			int def = skill.IsMagic ? receiver.MDefense : receiver.Defense;

			float atkDefMod = atk * AttackOverDefenseConst / def;
			int levelDiff = caster.Level - receiver.Level;
			float levelDiffMod = levelDiff * LevelDiffImpact;
			float skillDamage = atkDefMod * (1 + levelDiffMod) * skill.SkillDamagePercent * (1 + caster.BonusDamagePercent);
			
			int isCritRoll = rnd.Next(0, 100 * 1000);
			bool isCrit = isCritRoll < caster.CritRate * 100 * 1000;


			damage = (int) (isCrit ? skillDamage * (1 + caster.CritDamagePercent) : skillDamage);

			return new DamageInfo()
			{
				Damage = damage <= 0 ? 1 : damage,
				IsCrit = isCrit
			};
		}
	}
}

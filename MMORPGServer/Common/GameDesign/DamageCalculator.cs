using Common.Protocol.Combat;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common.GameDesign
{

	public class DamageCalculator
	{
		public const float AttackOverDefenseConst = 1.5f;
		public const float LevelDiffImpact = 0.025f;
		public const float MaxDamageVariance = 0.1f;

		private Random rnd = new Random();

		public DamageInfo GetDamage(EntityStats caster, EntityStats receiver, SkillStats skill)
		{
			int atk = skill.IsMagic ? caster.MAttack : caster.Attack;
			int def = skill.IsMagic ? receiver.MDefense : receiver.Defense;

			float atkDefMod = atk * AttackOverDefenseConst / def;
			int levelDiff = caster.Level - receiver.Level;
			float levelDiffMod = levelDiff * LevelDiffImpact;
			float skillDamage = atkDefMod * (1 + levelDiffMod) * skill.SkillDamagePercent * (1 + caster.BonusDamagePercent);

			bool isCrit = CalculateIsCrit(caster);

			int damage = AddCritDamage(caster, skillDamage, isCrit);

			damage = CalculateDamageVariance(damage);

			return new DamageInfo()
			{
				Damage = damage <= 0 ? 1 : damage,
				IsCrit = isCrit
			};
		}

		private int CalculateDamageVariance(int damage)
		{
			int maxDamageVariance = (int)(damage * MaxDamageVariance);
			if (maxDamageVariance > 0)
			{
				int damageVariance = rnd.Next(0, maxDamageVariance + 1);
				damage -= damageVariance;
			}

			return damage;
		}

		private static int AddCritDamage(EntityStats caster, float skillDamage, bool isCrit)
		{
			return (int)(isCrit ? skillDamage * (1 + caster.CritDamagePercent) : skillDamage);
		}

		private bool CalculateIsCrit(EntityStats caster)
		{
			int isCritRoll = rnd.Next(0, 100 * 1000);
			bool isCrit = isCritRoll < caster.CritRate * 100 * 1000;
			return isCrit;
		}
	}
}

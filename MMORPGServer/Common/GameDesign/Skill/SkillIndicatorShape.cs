using System.Numerics;

namespace Common.GameDesign.Skill
{
	public abstract class SkillIndicatorShape
	{
		public bool IsExcluding = false;

		public abstract bool IsHit(Vector2 position);
	}
}

using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Common.GameDesign.Skill
{
	public class SkillCollision
	{
		public SkillCastType CastType = SkillCastType.Boss1Attack1;
		public List<SkillCollisionShape> Shapes = new List<SkillCollisionShape>();
		public Vector2Int Size = new Vector2Int();

		public bool IsHit(Vector2 position)
		{
			bool hit = false;

			foreach(var shape in Shapes)
			{
				if(shape.IsHit(position))
				{
					hit = !shape.IsExcluding;
				}
			}

			return hit;
		}
	}
}

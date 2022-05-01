using System.Numerics;

namespace Common.GameDesign.Skill
{
	public class CircleShape : SkillCollisionShape
	{
		public Vector2 Position;
		public float Radius;

		public override bool IsHit(Vector2 position)
		{
			return Vector2.DistanceSquared(Position, position) <= Radius * Radius;
		}
	}
}

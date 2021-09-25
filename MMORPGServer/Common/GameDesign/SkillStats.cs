namespace Common.GameDesign
{
	public class SkillStats
	{
		public int Cooldown { get; set; } = 1000;
		public float SkillDamagePercent { get; set; } = 1;

		public bool IsMagic { get; set; } = true;
	}
}

namespace Common.GameDesign
{
	public class EntityStats
	{
		public int Attack { get; set; } = 6;
		public int Defense { get; set; } = 5;
		public int MAttack { get; set; } = 6;
		public int MDefense { get; set; } = 5;
		public int Level { get; set; } = 1;

		public float CritRate { get; set; } = 0.05f;
		public float CritDamagePercent { get; set; } = 0.5f;

		public float BonusDamagePercent { get; set; } = 0;
		public int MaxHP { get; set; } = 50;
	}
}

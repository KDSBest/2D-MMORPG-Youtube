using ReliableUdp.Utility;

namespace Common.GameDesign
{
	public class EntityStats
	{
		public int Attack { get; set; } = 6;
		public int Defense { get; set; } = 5;
		public int MAttack { get; set; } = 6;
		public int MDefense { get; set; } = 5;
		public int Level { get; set; } = 1;
        public int MaxHP { get; set; } = 50;

        public float CritRate { get; set; } = 0.05f;
		public float CritDamagePercent { get; set; } = 0.5f;

		public float BonusDamagePercent { get; set; } = 0;


        public void WriteData(UdpDataWriter writer)
        {
            writer.Put(Attack);
            writer.Put(Defense);
            writer.Put(MAttack);
            writer.Put(MDefense);
            writer.Put(Level);
            writer.Put(MaxHP);

            writer.Put(CritRate);
            writer.Put(CritDamagePercent);
            writer.Put(BonusDamagePercent);
        }

        public void ReadData(UdpDataReader reader)
        {
            Attack = reader.GetInt();
            Defense = reader.GetInt();
            MAttack = reader.GetInt();
            MDefense = reader.GetInt();
            Level = reader.GetInt();
            MaxHP = reader.GetInt();

            CritRate = reader.GetFloat();
            CritDamagePercent = reader.GetFloat();
            BonusDamagePercent = reader.GetFloat();
        }
    }
}

using Common.GameDesign;
using ReliableUdp.Utility;
using System.Numerics;

namespace Common.Protocol.Combat
{
	public class DamageMessage : BaseUdpPackage
    {
        public DamageInfo DamageInfo = new DamageInfo();

        public SkillTarget Target { get; set; } = new SkillTarget();

        public DamageMessage() : base(MessageType.Damage)
        {
        }

        protected override void WriteData(UdpDataWriter writer)
        {
            writer.Put(DamageInfo.IsCrit);
            writer.Put(DamageInfo.Damage);
            Target.WriteData(writer);
        }

        protected override bool ReadData(UdpDataReader reader)
        {
            DamageInfo.IsCrit = reader.GetBool();
            DamageInfo.Damage = reader.GetInt();
            Target.ReadData(reader);

            return true;
        }
    }
}

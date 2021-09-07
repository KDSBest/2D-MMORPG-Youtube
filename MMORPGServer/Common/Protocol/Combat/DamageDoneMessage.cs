﻿using Common.GameDesign;
using ReliableUdp.Utility;
using System.Numerics;

namespace Common.Protocol.Combat
{
	public class DamageDoneMessage : BaseUdpPackage
    {
        public int Damage { get; set; }

        public SkillTarget Target { get; set; } = new SkillTarget();

        public DamageDoneMessage() : base(MessageType.DamageDone)
        {
        }

        protected override void WriteData(UdpDataWriter writer)
        {
            writer.Put(Damage);
            Target.WriteData(writer);
        }

        protected override bool ReadData(UdpDataReader reader)
        {
            Damage = reader.GetInt();
            Target.ReadData(reader);

            return true;
        }
    }
}

using Newtonsoft.Json;
using ReliableUdp.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Common.Protocol.PlayerEvent
{
	public class PlayerEventMessage : BaseUdpPackage
    {
        public PlayerEventType Type { get; set; }
        public DateTime CreationDate { get; set; }

        public Dictionary<string, int> Add { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> Remove { get; set; } = new Dictionary<string, int>();


        public PlayerEventMessage() : base(MessageType.PlayerEvent)
        {
        }

        protected override void WriteData(UdpDataWriter writer)
        {
            writer.Put((byte)Type);
            writer.Put(CreationDate.Ticks);

            int c = Add.Count;
            var add = Add.ToList();
            writer.Put(c);

            for (int i = 0; i < c; i++)
            {
                writer.Put(add[i].Key);
                writer.Put(add[i].Value);
            }

            c = Remove.Count;
            var remove = Remove.ToList();
            writer.Put(c);

            for (int i = 0; i < c; i++)
            {
                writer.Put(remove[i].Key);
                writer.Put(remove[i].Value);
            }
        }

        protected override bool ReadData(UdpDataReader reader)
        {
            Type = (PlayerEventType)reader.GetByte();
            CreationDate = new DateTime(reader.GetLong());

            int c = reader.GetInt();

            for (int i = 0; i < c; i++)
            {
                Add.Add(reader.GetString(), reader.GetInt());
            }

            c = reader.GetInt();

            for (int i = 0; i < c; i++)
            {
                Remove.Add(reader.GetString(), reader.GetInt());
            }

            return true;
        }
    }
}

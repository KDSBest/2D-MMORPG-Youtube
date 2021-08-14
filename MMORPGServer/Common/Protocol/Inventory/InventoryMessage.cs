using ReliableUdp.Utility;
using System.Linq;

namespace Common.Protocol.Inventory
{
	public class InventoryMessage : BaseUdpPackage
    {
        public Inventory Inventory { get; set; }

        public InventoryMessage() : base(MessageType.Inventory)
        {
        }

        protected override void WriteData(UdpDataWriter writer)
        {
            var items = Inventory.Items.ToList();
            int c = items.Count;
            writer.Put(c);

            for(int i = 0; i < c; i++)
			{
                writer.Put(items[i].Key);
                writer.Put(items[i].Value);
            }
        }

        protected override bool ReadData(UdpDataReader reader)
        {
            Inventory = new Inventory();
            int c = reader.GetInt();

            for (int i = 0; i < c; i++)
            {
                Inventory.Items.Add(reader.GetString(), reader.GetInt());
            }

            return true;
        }
    }

}

using ReliableUdp.Utility;

namespace Common.Protocol
{
	public abstract class BaseUdpPackage : IUdpPackage
    {
        private MessageType type;

        public BaseUdpPackage(MessageType type)
        {
            this.type = type;
        }

        protected virtual void WriteData(UdpDataWriter writer)
		{

		}

        protected virtual bool ReadData(UdpDataReader reader)
		{
            return true;
		}

        public void Write(UdpDataWriter writer)
        {
            writer.Put((byte)type);
            WriteData(writer);
        }

        public bool Read(UdpDataReader reader)
        {
            if (reader.PeekByte() != (byte)type)
            {
                return false;
            }

            reader.GetByte();

            return ReadData(reader);
        }
    }

}

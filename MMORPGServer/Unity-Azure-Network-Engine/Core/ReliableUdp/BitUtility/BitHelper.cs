namespace ReliableUdp.BitUtility
{
	public static class BitHelper
	{
		private static void WriteEndian(byte[] buffer, int offset, ulong data)
		{
#if BIGENDIAN
            buffer[offset + 7] = (byte)(data);
            buffer[offset + 6] = (byte)(data >> 8);
            buffer[offset + 5] = (byte)(data >> 16);
            buffer[offset + 4] = (byte)(data >> 24);
            buffer[offset + 3] = (byte)(data >> 32);
            buffer[offset + 2] = (byte)(data >> 40);
            buffer[offset + 1] = (byte)(data >> 48);
            buffer[offset    ] = (byte)(data >> 56);
#else
			buffer[offset] = (byte)(data);
			buffer[offset + 1] = (byte)(data >> 8);
			buffer[offset + 2] = (byte)(data >> 16);
			buffer[offset + 3] = (byte)(data >> 24);
			buffer[offset + 4] = (byte)(data >> 32);
			buffer[offset + 5] = (byte)(data >> 40);
			buffer[offset + 6] = (byte)(data >> 48);
			buffer[offset + 7] = (byte)(data >> 56);
#endif
		}

		private static void WriteEndian(byte[] buffer, int offset, int data)
		{
#if BIGENDIAN
            buffer[offset + 3] = (byte)(data);
            buffer[offset + 2] = (byte)(data >> 8);
            buffer[offset + 1] = (byte)(data >> 16);
            buffer[offset    ] = (byte)(data >> 24);
#else
			buffer[offset] = (byte)(data);
			buffer[offset + 1] = (byte)(data >> 8);
			buffer[offset + 2] = (byte)(data >> 16);
			buffer[offset + 3] = (byte)(data >> 24);
#endif
		}

		public static void WriteEndian(byte[] buffer, int offset, short data)
		{
#if BIGENDIAN
            buffer[offset + 1] = (byte)(data);
            buffer[offset    ] = (byte)(data >> 8);
#else
			buffer[offset] = (byte)(data);
			buffer[offset + 1] = (byte)(data >> 8);
#endif
		}

		public static void Write(byte[] bytes, int startIndex, double value)
		{
			ConverterHelperDouble ch = new ConverterHelperDouble { Adouble = value };
			WriteEndian(bytes, startIndex, ch.Along);
		}

		public static void Write(byte[] bytes, int startIndex, float value)
		{
			ConverterHelperFloat ch = new ConverterHelperFloat { Afloat = value };
			WriteEndian(bytes, startIndex, ch.Aint);
		}

		public static void Write(byte[] bytes, int startIndex, short value)
		{
			WriteEndian(bytes, startIndex, value);
		}

		public static void Write(byte[] bytes, int startIndex, ushort value)
		{
			WriteEndian(bytes, startIndex, (short)value);
		}

		public static void Write(byte[] bytes, int startIndex, int value)
		{
			WriteEndian(bytes, startIndex, value);
		}

		public static void Write(byte[] bytes, int startIndex, uint value)
		{
			WriteEndian(bytes, startIndex, (int)value);
		}

		public static void Write(byte[] bytes, int startIndex, long value)
		{
			WriteEndian(bytes, startIndex, (ulong)value);
		}

		public static void Write(byte[] bytes, int startIndex, ulong value)
		{
			WriteEndian(bytes, startIndex, value);
		}
	}
}
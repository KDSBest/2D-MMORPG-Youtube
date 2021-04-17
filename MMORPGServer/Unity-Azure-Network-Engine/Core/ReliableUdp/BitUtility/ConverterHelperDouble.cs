using System.Runtime.InteropServices;

namespace ReliableUdp.BitUtility
{
	[StructLayout(LayoutKind.Explicit)]
	public struct ConverterHelperDouble
	{
		[FieldOffset(0)]
		public ulong Along;

		[FieldOffset(0)]
		public double Adouble;
	}
}
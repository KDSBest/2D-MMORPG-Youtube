using System.Runtime.InteropServices;

namespace ReliableUdp.BitUtility
{
	[StructLayout(LayoutKind.Explicit)]
	public struct ConverterHelperFloat
	{
		[FieldOffset(0)]
		public int Aint;

		[FieldOffset(0)]
		public float Afloat;
	}
}
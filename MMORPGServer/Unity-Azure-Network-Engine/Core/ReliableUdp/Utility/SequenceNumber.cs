namespace ReliableUdp.Utility
{
	public class SequenceNumber
	{
		public const ushort MAX_SEQUENCE = 32768;
		public const ushort MAX_SEQUENCE_HALF = MAX_SEQUENCE / 2;
        
		public int Value { get; set; }

		public bool IsValid
		{
			get
			{
				return this.Value <= MAX_SEQUENCE;
			}
		}

		public SequenceNumber(int value)
		{
			this.Value = value;
		}

		public override int GetHashCode()
		{
			return this.Value;
		}

		public override bool Equals(object obj)
		{
			return this == (SequenceNumber)obj;
		}

		public static bool operator !=(SequenceNumber a, SequenceNumber b)
		{
			return !(a == b);
		}

		public static bool operator ==(SequenceNumber a, SequenceNumber b)
		{
			if ((object)a == null)
			{
				return (object) b == null;
			}

			return a.Value == b.Value;
		}

		public static SequenceNumber operator ++(SequenceNumber a)
		{
			a.Value = (a.Value + 1) % MAX_SEQUENCE;
			return a;
		}

		public static int operator -(SequenceNumber a, SequenceNumber b)
		{
			int val = ((a.Value - b.Value + MAX_SEQUENCE + MAX_SEQUENCE_HALF) % MAX_SEQUENCE - MAX_SEQUENCE_HALF);
			return val;
		}
	}
}

using Common.ScriptLanguage.AST;
using System;
using System.Globalization;

namespace Common.ScriptLanguage.VM
{
	public class VMVar
	{
		public VMType Type { get; set; } = VMType.Number;

		public string ValueString { get; set; }

		public double ValueNumber { get; set; }

		public VMVar Cast(VMType to)
		{
			if (Type == to)
				return this.Clone();

			switch(to)
			{
				case VMType.String:
					return new VMVar()
					{
						ValueString = this.ValueNumber.ToString(),
						Type = VMType.String
					};

				case VMType.Number:
					double dResult;
					if (!double.TryParse(this.ValueString, NumberStyles.Any, CultureInfo.GetCultureInfo("en"), out dResult))
					{
						throw new NotSupportedException();
					}

					return new VMVar()
					{
						ValueNumber = dResult,
						Type = VMType.Number
					};

				default:
					throw new NotSupportedException();
			}
		}

		public bool GetBool()
		{
			switch (Type)
			{
				case VMType.String:
					return !(string.IsNullOrEmpty(ValueString) || ValueString.ToLower() == "false");

				case VMType.Number:
					return ValueNumber != 0;

				default:
					throw new NotSupportedException();
			}
		}

		public void SetBool(bool result)
		{
			switch (Type)
			{
				case VMType.String:
					ValueString = result ? "true" : "false";
					break;

				case VMType.Number:
					ValueNumber = result ? -1 : 0;
					break;

				default:
					throw new NotSupportedException();
			}
		}

		public VMVar Clone()
		{
			return new VMVar()
			{
				Type = Type,
				ValueString = ValueString,
				ValueNumber = ValueNumber
			};
		}

		public static VMVar operator !(VMVar a)
		{
			var result = a.Clone();
			result.SetBool(!a.GetBool());

			return result;
		}

		public static VMVar operator -(VMVar a)
		{
			var result = a.Cast(VMType.Number);
			result.ValueNumber = -result.ValueNumber;

			return result;
		}

		public static VMVar operator &(VMVar a, VMVar b)
		{
			var result = a.Clone();
			result.SetBool(a.GetBool() & b.GetBool());

			return result;
		}

		public static VMVar operator |(VMVar a, VMVar b)
		{
			var result = a.Clone();
			result.SetBool(a.GetBool() | b.GetBool());

			return result;
		}

		public static bool operator true(VMVar a)
		{
			return a.GetBool();
		}

		public static bool operator false(VMVar a)
		{
			return !a.GetBool();
		}

		public override bool Equals(object b)
		{
			if (object.ReferenceEquals(this, b))
				return true;

			VMVar aVar = this;
			VMVar bVar = b as VMVar;
			if(b == null)
			{
				return false;
			}

			return aVar == bVar;
		}

		public static VMVar operator ++(VMVar a)
		{
			var result = a.Cast(VMType.Number);
			result.ValueNumber++;

			return result;
		}

		public static VMVar operator --(VMVar a)
		{
			var result = a.Cast(VMType.Number);
			result.ValueNumber--;

			return result;
		}

		public static VMVar operator >(VMVar a, VMVar b)
		{
			var aStr = a.Cast(VMType.Number);
			var bStr = b.Cast(VMType.Number);

			return aStr.ValueNumber > bStr.ValueNumber;
		}

		public static VMVar operator <(VMVar a, VMVar b)
		{
			var aStr = a.Cast(VMType.Number);
			var bStr = b.Cast(VMType.Number);

			return aStr.ValueNumber < bStr.ValueNumber;
		}

		public static VMVar operator >=(VMVar a, VMVar b)
		{
			var aStr = a.Cast(VMType.Number);
			var bStr = b.Cast(VMType.Number);

			return aStr.ValueNumber >= bStr.ValueNumber;
		}

		public static VMVar operator <=(VMVar a, VMVar b)
		{
			var aStr = a.Cast(VMType.Number);
			var bStr = b.Cast(VMType.Number);

			return aStr.ValueNumber <= bStr.ValueNumber;
		}

		public static VMVar operator ^(VMVar a, VMVar b)
		{
			var result = a.Clone();
			result.SetBool(a.GetBool() ^ b.GetBool());

			return result;
		}

		public static VMVar operator *(VMVar a, VMVar b)
		{
			VMVar aNum = a.Cast(VMType.Number);
			VMVar bNum = b.Cast(VMType.Number);

			aNum.ValueNumber *= bNum.ValueNumber;

			return aNum;
		}

		public static VMVar operator /(VMVar a, VMVar b)
		{
			VMVar aNum = a.Cast(VMType.Number);
			VMVar bNum = b.Cast(VMType.Number);

			aNum.ValueNumber /= bNum.ValueNumber;

			return aNum;
		}

		public static VMVar operator %(VMVar a, VMVar b)
		{
			VMVar aNum = a.Cast(VMType.Number);
			VMVar bNum = b.Cast(VMType.Number);

			aNum.ValueNumber %= bNum.ValueNumber;

			return aNum;
		}

		public static VMVar operator +(VMVar a, VMVar b)
		{
			VMVar aNum = a.Cast(VMType.Number);
			VMVar bNum = b.Cast(VMType.Number);

			aNum.ValueNumber += bNum.ValueNumber;

			return aNum;
		}

		public static VMVar operator -(VMVar a, VMVar b)
		{
			VMVar aNum = a.Cast(VMType.Number);
			VMVar bNum = b.Cast(VMType.Number);

			aNum.ValueNumber -= bNum.ValueNumber;

			return aNum;
		}

		public static bool operator ==(VMVar a, VMVar b)
		{
			var aStr = a.Cast(VMType.String);
			var bStr = b.Cast(VMType.String);

			return aStr.ValueString == bStr.ValueString;
		}

		public static bool operator !=(VMVar a, VMVar b)
		{
			return !(a == b);
		}

		public override int GetHashCode()
		{
			return Cast(VMType.String).ValueString.GetHashCode();
		}

		public override string ToString()
		{
			return Cast(VMType.String).ValueString;
		}

		public static implicit operator bool(VMVar value)
		{
			return value.GetBool();
		}

		public static implicit operator VMVar(bool value)
		{
			var result = new VMVar();

			result.SetBool(value);

			return result;
		}
	}
}

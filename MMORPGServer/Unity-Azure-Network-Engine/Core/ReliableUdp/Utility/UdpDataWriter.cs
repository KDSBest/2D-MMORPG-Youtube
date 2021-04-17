using System;
using System.Text;

using ReliableUdp.BitUtility;

namespace ReliableUdp.Utility
{
    public class UdpDataWriter
	{
		protected byte[] _data;
		protected int Position;

		private int maxLength;
		private readonly bool autoResize;

		public UdpDataWriter()
		{
			this.maxLength = 64;
			this._data = new byte[this.maxLength];
			this.autoResize = true;
		}

		public UdpDataWriter(bool autoResize)
		{
			this.maxLength = 64;
			this._data = new byte[this.maxLength];
			this.autoResize = autoResize;
		}

		public UdpDataWriter(bool autoResize, int initialSize)
		{
			this.maxLength = initialSize;
			this._data = new byte[this.maxLength];
			this.autoResize = autoResize;
		}

		public void ResizeIfNeed(int newSize)
		{
			if (this.maxLength < newSize)
			{
				while (this.maxLength < newSize)
				{
					this.maxLength *= 2;
				}
				Array.Resize(ref this._data, this.maxLength);
			}
		}

		public void Reset(int size)
		{
			this.ResizeIfNeed(size);
			this.Position = 0;
		}

		public void Reset()
		{
			this.Position = 0;
		}

		public byte[] CopyData()
		{
			byte[] resultData = new byte[this.Position];
			Buffer.BlockCopy(this._data, 0, resultData, 0, this.Position);
			return resultData;
		}

		public byte[] Data
		{
			get { return this._data; }
		}

		public int Length
		{
			get { return this.Position; }
		}

		public void Put(float value)
		{
			if (this.autoResize)
				this.ResizeIfNeed(this.Position + 4);
			BitHelper.Write(this._data, this.Position, value);
			this.Position += 4;
		}

		public void Put(double value)
		{
			if (this.autoResize)
				this.ResizeIfNeed(this.Position + 8);
			BitHelper.Write(this._data, this.Position, value);
			this.Position += 8;
		}

		public void Put(long value)
		{
			if (this.autoResize)
				this.ResizeIfNeed(this.Position + 8);
			BitHelper.Write(this._data, this.Position, value);
			this.Position += 8;
		}

		public void Put(ulong value)
		{
			if (this.autoResize)
				this.ResizeIfNeed(this.Position + 8);
			BitHelper.Write(this._data, this.Position, value);
			this.Position += 8;
		}

		public void Put(int value)
		{
			if (this.autoResize)
				this.ResizeIfNeed(this.Position + 4);
			BitHelper.Write(this._data, this.Position, value);
			this.Position += 4;
		}

		public void Put(uint value)
		{
			if (this.autoResize)
				this.ResizeIfNeed(this.Position + 4);
			BitHelper.Write(this._data, this.Position, value);
			this.Position += 4;
		}

		public void Put(ushort value)
		{
			if (this.autoResize)
				this.ResizeIfNeed(this.Position + 2);
			BitHelper.Write(this._data, this.Position, value);
			this.Position += 2;
		}

		public void Put(short value)
		{
			if (this.autoResize)
				this.ResizeIfNeed(this.Position + 2);
			BitHelper.Write(this._data, this.Position, value);
			this.Position += 2;
		}

		public void Put(sbyte value)
		{
			if (this.autoResize)
				this.ResizeIfNeed(this.Position + 1);
			this._data[this.Position] = (byte)value;
			this.Position++;
		}

		public void Put(byte value)
		{
			if (this.autoResize)
				this.ResizeIfNeed(this.Position + 1);
			this._data[this.Position] = value;
			this.Position++;
		}

		public void Put(byte[] data, int offset, int length)
		{
			if (this.autoResize)
				this.ResizeIfNeed(this.Position + length);
			Buffer.BlockCopy(data, offset, this._data, this.Position, length);
			this.Position += length;
		}

		public void Put(byte[] data)
		{
			if (this.autoResize)
				this.ResizeIfNeed(this.Position + data.Length);
			Buffer.BlockCopy(data, 0, this._data, this.Position, data.Length);
			this.Position += data.Length;
		}

		public void Put(bool value)
		{
			if (this.autoResize)
				this.ResizeIfNeed(this.Position + 1);
			this._data[this.Position] = (byte)(value ? 1 : 0);
			this.Position++;
		}

		public void Put(float[] value)
		{
			ushort len = value == null ? (ushort)0 : (ushort)value.Length;
			if (this.autoResize)
				this.ResizeIfNeed(this.Position + len * 4 + 2);
			this.Put(len);
			for (int i = 0; i < len; i++)
			{
				this.Put(value[i]);
			}
		}

		public void Put(double[] value)
		{
			ushort len = value == null ? (ushort)0 : (ushort)value.Length;
			if (this.autoResize)
				this.ResizeIfNeed(this.Position + len * 8 + 2);
			this.Put(len);
			for (int i = 0; i < len; i++)
			{
				this.Put(value[i]);
			}
		}

		public void Put(long[] value)
		{
			ushort len = value == null ? (ushort)0 : (ushort)value.Length;
			if (this.autoResize)
				this.ResizeIfNeed(this.Position + len * 8 + 2);
			this.Put(len);
			for (int i = 0; i < len; i++)
			{
				this.Put(value[i]);
			}
		}

		public void Put(ulong[] value)
		{
			ushort len = value == null ? (ushort)0 : (ushort)value.Length;
			if (this.autoResize)
				this.ResizeIfNeed(this.Position + len * 8 + 2);
			this.Put(len);
			for (int i = 0; i < len; i++)
			{
				this.Put(value[i]);
			}
		}

		public void Put(int[] value)
		{
			ushort len = value == null ? (ushort)0 : (ushort)value.Length;
			if (this.autoResize)
				this.ResizeIfNeed(this.Position + len * 4 + 2);
			this.Put(len);
			for (int i = 0; i < len; i++)
			{
				this.Put(value[i]);
			}
		}

		public void Put(uint[] value)
		{
			ushort len = value == null ? (ushort)0 : (ushort)value.Length;
			if (this.autoResize)
				this.ResizeIfNeed(this.Position + len * 4 + 2);
			this.Put(len);
			for (int i = 0; i < len; i++)
			{
				this.Put(value[i]);
			}
		}

		public void Put(ushort[] value)
		{
			ushort len = value == null ? (ushort)0 : (ushort)value.Length;
			if (this.autoResize)
				this.ResizeIfNeed(this.Position + len * 2 + 2);
			this.Put(len);
			for (int i = 0; i < len; i++)
			{
				this.Put(value[i]);
			}
		}

		public void Put(short[] value)
		{
			ushort len = value == null ? (ushort)0 : (ushort)value.Length;
			if (this.autoResize)
				this.ResizeIfNeed(this.Position + len * 2 + 2);
			this.Put(len);
			for (int i = 0; i < len; i++)
			{
				this.Put(value[i]);
			}
		}

		public void Put(bool[] value)
		{
			ushort len = value == null ? (ushort)0 : (ushort)value.Length;
			if (this.autoResize)
				this.ResizeIfNeed(this.Position + len + 2);
			this.Put(len);
			for (int i = 0; i < len; i++)
			{
				this.Put(value[i]);
			}
		}

		public void Put(string[] value)
		{
			ushort len = value == null ? (ushort)0 : (ushort)value.Length;
			this.Put(len);
			for (int i = 0; i < len; i++)
			{
				this.Put(value[i]);
			}
		}

		public void Put(string[] value, int maxLength)
		{
			ushort len = value == null ? (ushort)0 : (ushort)value.Length;
			this.Put(len);
			for (int i = 0; i < len; i++)
			{
				this.Put(value[i], maxLength);
			}
		}

		public void Put(string value)
		{
			if (string.IsNullOrEmpty(value))
			{
				this.Put(0);
				return;
			}

			int bytesCount = Encoding.UTF8.GetByteCount(value);
			if (this.autoResize)
				this.ResizeIfNeed(this.Position + bytesCount + 4);
			this.Put(bytesCount);

			Encoding.UTF8.GetBytes(value, 0, value.Length, this._data, this.Position);
			this.Position += bytesCount;
		}

		public void Put(string value, int maxLength)
		{
			if (string.IsNullOrEmpty(value))
			{
				this.Put(0);
				return;
			}

			int length = value.Length > maxLength ? maxLength : value.Length;

			int bytesCount = Encoding.UTF8.GetByteCount(value);
			if (this.autoResize)
				this.ResizeIfNeed(this.Position + bytesCount + 4);

			this.Put(bytesCount);

			Encoding.UTF8.GetBytes(value, 0, length, this._data, this.Position);

			this.Position += bytesCount;
		}
	}
}
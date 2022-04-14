using System.IO;
using System.Text;

namespace Assets.Scripts
{
	public class UnityConsoleTextWriter : TextWriter
	{
		private StringBuilder buffer = new StringBuilder();

		public override void Flush()
		{
			UnityEngine.Debug.Log(buffer.ToString());
			buffer.Clear();
		}

		private void FlushOnNewLine()
		{
			if(buffer.Length > 0 && buffer[buffer.Length - 1] == '\n')
			{
				Flush();
			}
		}

		public override void Write(string value)
		{
			if (string.IsNullOrEmpty(value))
				return;

			buffer.Append(value);
			FlushOnNewLine();
		}

		public override void Write(char value)
		{
			buffer.Append(value);
			FlushOnNewLine();
		}

		public override void Write(char[] value, int index, int count)
		{
			if (count == 0)
				return;
			
			buffer.Append(value, index, count);
			FlushOnNewLine();
		}

		public override Encoding Encoding
		{
			get { return Encoding.Default; }
		}
	}
}
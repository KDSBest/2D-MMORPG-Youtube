using System;

namespace Common.ScriptLanguage
{
	public class LexerException : Exception
    {
        public int LineNumber;

        public LexerException(string message, int lineNumber)
            : base(message)
        {
            LineNumber = lineNumber;
        }
    }
}

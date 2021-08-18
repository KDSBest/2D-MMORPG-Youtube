using System;

namespace Common.ScriptLanguage
{
	public class ParserException : Exception
    {
        public int LineNumber;

        public ParserException(string message, int lineNumber)
            : base(message)
        {
            LineNumber = lineNumber;
        }
    }
}

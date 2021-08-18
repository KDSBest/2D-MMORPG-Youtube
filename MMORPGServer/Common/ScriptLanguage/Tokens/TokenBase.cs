namespace Common.ScriptLanguage.Tokens
{
	public abstract class TokenBase
    {
        public string Token;
        public int LineNumber;

        public TokenBase(string token, int lineNumber)
        {
            Token = token;
            LineNumber = lineNumber;
        }
    }
}

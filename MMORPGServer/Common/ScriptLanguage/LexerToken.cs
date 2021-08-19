namespace Common.ScriptLanguage
{
	public class LexerToken
    {
        public string Token;
        public int LineNumber;

        public LexerToken(string token, int lineNumber)
        {
            Token = token;
            LineNumber = lineNumber;
        }
    }
}

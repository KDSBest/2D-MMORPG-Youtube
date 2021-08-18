namespace Common.ScriptLanguage.Tokens
{
	public class ReservedWordToken : TokenBase
    {
        public ReservedWordToken(string token, int lineNumber)
            : base(token, lineNumber)
        {
        }
    }
}

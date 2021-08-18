namespace Common.ScriptLanguage
{
	/// <summary>
	/// Holds a Token as a String and the corresponding Line Number
	/// </summary>
	public class LexerToken
    {
        public string Token;
        public int LineNumber;

        /// <summary>
        /// trivial
        /// </summary>
        /// <param name="token">trivial</param>
        /// <param name="lineNumber">trivial</param>
        public LexerToken(string token, int lineNumber)
        {
            Token = token;
            LineNumber = lineNumber;
        }
    }
}

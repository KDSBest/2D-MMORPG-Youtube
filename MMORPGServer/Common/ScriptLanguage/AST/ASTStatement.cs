namespace Common.ScriptLanguage.AST
{
	public class ASTStatement : ASTNode
    {
        public ASTNode Statement;

		public override string ToString()
		{
			return "ASTStatement: \r\n" +
				Statement != null ? Statement.ToString() : "(none)";
		}
	}
}

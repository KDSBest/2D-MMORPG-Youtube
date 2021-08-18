namespace Common.ScriptLanguage.AST
{
	public class ASTReturn : ASTNode
    {
        public ASTStatement Return;

		public override string ToString()
		{
			return "ASTReturn:\r\n" + Return.ToString();
		}
	}
}

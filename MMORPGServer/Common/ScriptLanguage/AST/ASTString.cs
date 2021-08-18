namespace Common.ScriptLanguage.AST
{
	public class ASTString : ASTNode
    {
        public string String;

		public override string ToString()
		{
			return "ASTString: " + this.String.Replace("\n", "\\n");
		}
	}
}

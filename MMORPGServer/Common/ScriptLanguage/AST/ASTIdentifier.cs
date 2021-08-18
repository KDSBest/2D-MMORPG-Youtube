namespace Common.ScriptLanguage.AST
{
	public class ASTIdentifier : ASTNode
    {
        public string Identifier;

		public override string ToString()
		{
			return "ASTIdentifier: " + this.Identifier;
		}
	}
}

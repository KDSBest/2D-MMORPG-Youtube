namespace Common.ScriptLanguage.AST
{
	public class ASTClassParameter : ASTNode
    {
        public string Scope;
        public ASTStatement Parameter;

		public override string ToString()
		{
			return "ASTClassParameter: " + this.Scope + "\r\n" +
				Parameter != null ? Parameter.ToString() : "(none)";
		}
	}
}

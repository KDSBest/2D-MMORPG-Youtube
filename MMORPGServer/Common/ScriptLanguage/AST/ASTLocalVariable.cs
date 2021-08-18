namespace Common.ScriptLanguage.AST
{
	public class ASTLocalVariable : ASTNode
    {
        public string Name;
        public VMType Type;

		public override string ToString()
		{
			return "ASTLocalVariable:\r\n" + this.Type.ToString() + ": " + this.Name;
		}
	}
}

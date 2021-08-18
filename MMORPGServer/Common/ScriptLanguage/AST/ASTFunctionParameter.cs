namespace Common.ScriptLanguage.AST
{
	public class ASTFunctionParameter : ASTNode
    {
        public string Name;
        public VMType Type;

		public override string ToString()
		{
            return "ASTFunctionParameter:\r\n" + this.Type.ToString() + ": " + this.Name;
        }
	}
}

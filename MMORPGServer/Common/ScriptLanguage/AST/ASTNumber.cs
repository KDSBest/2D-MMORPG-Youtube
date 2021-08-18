namespace Common.ScriptLanguage.AST
{
	public class ASTNumber : ASTNode
    {
        public double Number;

		public override string ToString()
		{
			return "ASTNumber: " + this.Number;
		}
	}
}

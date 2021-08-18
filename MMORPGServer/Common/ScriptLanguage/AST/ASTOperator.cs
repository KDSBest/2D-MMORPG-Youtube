namespace Common.ScriptLanguage.AST
{
	public class ASTOperator : ASTNode
    {
        public ASTNode Left;
        public ASTNode Right;
        public string Operator;
        public int Precedence;

		public override string ToString()
		{
			return "ASTOperator: " + this.Operator + "\r\n" +
				"Left: " + (Left != null ? Left.ToString() : "(none)") + "\r\n" +
				"Right: " + (Right != null ? Right.ToString() : "(none)");
		}
	}
}

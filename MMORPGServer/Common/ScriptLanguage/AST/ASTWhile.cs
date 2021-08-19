using System.Collections.Generic;

namespace Common.ScriptLanguage.AST
{
	public class ASTWhile : ASTNode
    {
        public ASTStatement Condition;
        public List<ASTNode> CodeBlock;

		public override string ToString()
		{
            string result = "ASTWhile:";

            result += "\r\nCondition:";
            if (this.Condition != null)
            {
                result += "\r\n" + this.Condition.ToString();
            }

            result += "\r\nCodeBlock:";
            foreach (ASTNode codeBlock in this.CodeBlock)
            {
                if (codeBlock == null)
                    continue;

                result += "\r\n" + codeBlock.ToString();
            }

            return result;
        }
	}
}

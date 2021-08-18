using System.Collections.Generic;

namespace Common.ScriptLanguage.AST
{
	public class ASTIf : ASTNode
    {
        public ASTStatement Condition;
        public List<ASTNode> CodeBlock;
        public List<ASTNode> ElseCodeBlock;

		public override string ToString()
		{
            string result = "ASTIf: \r\n";
            result += "Condition:  \r\n";
            result += this.Condition.ToString();

            result += "\r\n CodeBlock:";
            foreach (ASTNode codeBlock in this.CodeBlock)
            {
                if (codeBlock == null)
                    continue;

                result += "\r\n" + codeBlock.ToString();
            }
            if (this.ElseCodeBlock != null)
            {
                result += "\r\nElse:\r\n";
                foreach (ASTNode codeBlock in this.ElseCodeBlock)
				{
                    if (codeBlock == null)
                        continue;

                    result += "\r\n" + codeBlock.ToString();
                }
            }

            return result;
        }
	}
}

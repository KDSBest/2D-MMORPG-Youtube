using System.Collections.Generic;

namespace Common.ScriptLanguage.AST
{
	public class ASTFor : ASTNode
    {
        public ASTStatement Initialise;
        public ASTStatement Condition;
        public ASTStatement Counter;
        public List<ASTNode> CodeBlock;

		public override string ToString()
		{
            string result = "ASTFor:";

            result += "\r\nInit:";
            if (this.Initialise != null)
            {
                result += "\r\n" + this.Initialise.ToString();
            }

            result += "\r\nCondition:";
            if (this.Condition != null)
            {
                result += "\r\n" + this.Condition.ToString();
            }

            result += "\r\nCounter:";
            if (this.Counter != null)
            {
                result += "\r\n" + this.Counter.ToString();
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

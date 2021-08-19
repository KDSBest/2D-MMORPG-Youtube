using System.Collections.Generic;

namespace Common.ScriptLanguage.AST
{
	public class ASTFunction : ASTNode
    {
        public string Name;
        public VMType ReturnValue;
        public List<ASTNode> Parameter;
        public List<ASTNode> CodeBlock;

		public override string ToString()
		{
            string result = "ASTFunction:";

            result += "\r\nParameter:";
            foreach (ASTNode para in this.Parameter)
                result += "\r\n" + para.ToString();

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

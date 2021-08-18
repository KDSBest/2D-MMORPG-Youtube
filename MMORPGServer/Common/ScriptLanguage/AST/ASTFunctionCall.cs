using System.Collections.Generic;

namespace Common.ScriptLanguage.AST
{
	public class ASTFunctionCall : ASTNode
    {
        public string Scope;
        public string FunctionName;
        public List<ASTStatement> Parameter;

        public ASTFunctionCall()
        {
            Parameter = new List<ASTStatement>();
        }

		public override string ToString()
		{
            string result = "ASTFunctionCall: " + this.Scope + "." + this.FunctionName;
            result += "\r\nParameters:";
            foreach (ASTNode param in this.Parameter)
                result += "\r\n" + param.ToString();
            return result;
		}
	}
}

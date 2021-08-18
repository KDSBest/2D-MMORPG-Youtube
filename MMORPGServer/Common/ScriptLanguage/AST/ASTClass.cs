using System.Collections.Generic;

namespace Common.ScriptLanguage.AST
{
	public class ASTClass : ASTNode
    {
        public string Name;
        public List<ASTConstructor> Constructors;
        public List<ASTClassAttribute> Attributes;
        public List<ASTFunction> Functions;

		public override string ToString()
		{
            string result = "ASTClass: " + this.Name;

            result += "\r\nAttributes:";
            foreach (ASTNode n in this.Attributes)
                result += "\r\n" + n.ToString();

            result += "\r\nConstructors:";
            foreach (ASTNode n in this.Constructors)
                result += "\r\n" + n.ToString();

            result += "\r\nFunctions:";
            foreach (ASTNode n in this.Functions)
                result += "\r\n" + n.ToString();

            return result;
        }
    }
}

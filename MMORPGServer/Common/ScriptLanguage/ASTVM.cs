using Common.ScriptLanguage.AST;
using Common.ScriptLanguage.VM;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common.ScriptLanguage
{
	public class ASTVM
	{
		private Dictionary<string, IVMScope> linkedScopes = new Dictionary<string, IVMScope>();

		public void AddLinkedScope(IVMScope vmScope)
		{
			if(linkedScopes.ContainsKey(vmScope.Scope))
			{
				linkedScopes[vmScope.Scope] = vmScope;
			}
			else
			{
				linkedScopes.Add(vmScope.Scope, vmScope);
			}
		}

		public List<VMVar> Execute(List<ASTStatement> nodes)
		{
			List<VMVar> result = new List<VMVar>();

			for(int i = 0; i < nodes.Count; i++)
			{
				result.Add(Execute(nodes[i]));
			}

			return result;
		}

		public VMVar Execute(ASTNode node)
		{
			switch(node)
			{
				case ASTStatement statementNode:
					return ExecuteStatement(statementNode);
				case ASTOperator operatorNode:
					return ExecuteOperator(operatorNode);
				case ASTFunctionCall functionCallNode:
					var parameters = Execute(functionCallNode.Parameter);
					return ExecuteFunctionCall(functionCallNode.Scope, functionCallNode.FunctionName, parameters);
				case ASTNumber numberNode:
					return new VMVar()
					{
						Type = VMType.Number,
						ValueNumber = numberNode.Number
					};
				case ASTString stringNode:
					return new VMVar()
					{
						Type = VMType.String,
						ValueString = stringNode.String
					};
				default:
					throw new NotSupportedException();
			}
		}

		private VMVar ExecuteFunctionCall(string scope, string functionName, List<VMVar> parameters)
		{
			return linkedScopes[scope].Execute(functionName, parameters);
		}

		private VMVar ExecuteOperator(ASTOperator operatorNode)
		{
			if (operatorNode.Right == null)
				return ExecuteUnaryOperator(operatorNode);
			return ExecuteBinaryOperator(operatorNode);
		}

		private VMVar ExecuteBinaryOperator(ASTOperator operatorNode)
		{
			VMVar lVar = Execute(operatorNode.Left);
			VMVar rVar = Execute(operatorNode.Right);
			switch (operatorNode.Operator)
			{
				case "*":
					return lVar * rVar;
				case "/":
					return lVar / rVar;
				case "%":
					return lVar % rVar;
				case "+":
					return lVar + rVar;
				case "-":
					return lVar - rVar;
				case "&":
					return lVar & rVar;
				case "|":
					return lVar | rVar;
				case "&&":
					return lVar && rVar;
				case "||":
					return lVar || rVar;
				case "==":
					return lVar == rVar;
				case "!=":
					return lVar != rVar;
				case ">":
					return lVar > rVar;
				case "<":
					return lVar < rVar;
				case ">=":
					return lVar >= rVar;
				case "<=":
					return lVar <= rVar;
				case "^":
					return lVar ^ rVar;

				default:
					throw new NotSupportedException();
			}
		}

		private VMVar ExecuteUnaryOperator(ASTOperator operatorNode)
		{
			VMVar lVar = Execute(operatorNode.Left);
			switch (operatorNode.Operator)
			{
				case "!":
					return !lVar;
				case "-":
					return -lVar;
				case "+":
					return lVar;
				case "--":
					return --lVar;
				case "++":
					return ++lVar;
				default:
					throw new NotSupportedException();
			}
		}

		private VMVar ExecuteStatement(ASTStatement statement)
		{
			return Execute(statement.Statement);
		}
	}
}

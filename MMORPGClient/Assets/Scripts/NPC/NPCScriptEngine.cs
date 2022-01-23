using Common.ScriptLanguage;
using Common.ScriptLanguage.VM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.NPC
{

	public class NPCScriptEngine
	{
		public List<IVMScope> Scopes = new List<IVMScope>()
		{
			new QuestBackendScope(),
			new TeleportBackendScope()
		};

		public VMVar Execute(string script)
		{
			var lexer = new Lexer();
			var tokens = lexer.Lex(script);

			var parser = new Parser();
			var statement = parser.ParseInlineStatement(tokens);

			var vm = new ASTVM();

			foreach (var scope in Scopes)
				vm.AddLinkedScope(scope);

			return vm.Execute(statement);
		}
	}
}

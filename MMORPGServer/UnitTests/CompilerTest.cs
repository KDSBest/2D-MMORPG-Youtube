using Common.Crypto;
using Common.ScriptLanguage;
using Common.ScriptLanguage.AST;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;

namespace UnitTests
{
	[TestClass]
	public class CompilerTest
	{
		[TestMethod]
		public void CompileStatementTest()
		{
			var lexer = new Lexer();
			var tokens = lexer.Lex(File.ReadAllText("statement.kds"));

			var parser = new Parser();
			var statement = parser.ParseInlineStatement(tokens);
			var result = statement.ToString();
			Assert.AreEqual(File.ReadAllText("statement.ast"), result);
		}

		[TestMethod]
		public void CompileClassTest()
		{
			var lexer = new Lexer();
			var tokens = lexer.Lex(File.ReadAllText("testscriptsimple.kds"));

			var parser = new Parser();
			List<ASTClass> classes = parser.Parse(tokens);

			string result = string.Empty;
			foreach (var c in classes)
				result += c.ToString() + "\r\n\r\n";

			Assert.AreEqual(File.ReadAllText("testscriptsimple.ast"), result);
		}

		[TestMethod]
		public void CompileComplexClassesTest()
		{
			var lexer = new Lexer();
			var tokens = lexer.Lex(File.ReadAllText("testscript.kds"));

			var parser = new Parser();
			List<ASTClass> classes = parser.Parse(tokens);

			string result = string.Empty;
			foreach (var c in classes)
				result += c.ToString() + "\r\n\r\n";

			Assert.AreEqual(File.ReadAllText("testscript.ast"), result);
		}
	}
}

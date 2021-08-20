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
		public void CompilerUnaryOperatorEqualTest()
		{
			var lexer = new Lexer();
			var tokens = lexer.Lex("1++");
			var tokens2 = lexer.Lex("++1");

			var parser = new Parser();
			var statement = parser.ParseInlineStatement(tokens);
			var statement2 = parser.ParseInlineStatement(tokens2);
			var result = statement.ToString();
			var result2 = statement2.ToString();
			Assert.AreEqual(result, result2);
		}

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
		public void FixParserOutOfRangeExceptionOnOperatorEnding()
		{
			var lexer = new Lexer();
			var tokens = lexer.Lex("1 + 2++");

			var parser = new Parser();
			var statement = parser.ParseInlineStatement(tokens);
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

		[TestMethod]
		public void CompileComplexClassesMultipleTest()
		{
			var lexer = new Lexer();
			var tokens = lexer.Lex(File.ReadAllText("testscript.kds"));
			var tokens2 = lexer.Lex(File.ReadAllText("testscript.kds"));

			var parser = new Parser();
			List<ASTClass> classes = parser.Parse(tokens);
			List<ASTClass> classes2 = parser.Parse(tokens2);

			string result = string.Empty;
			foreach (var c in classes)
				result += c.ToString() + "\r\n\r\n";

			string result2 = string.Empty;
			foreach (var c in classes2)
				result2 += c.ToString() + "\r\n\r\n";

			Assert.AreEqual(File.ReadAllText("testscript.ast"), result);
			Assert.AreEqual(result, result2);
		}
	}
}

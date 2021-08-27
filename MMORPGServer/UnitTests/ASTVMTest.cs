using Common.Crypto;
using Common.ScriptLanguage;
using Common.ScriptLanguage.AST;
using Common.ScriptLanguage.VM;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.IO;

namespace UnitTests
{
	[TestClass]
	public class ASTVMTest
	{
		private VMVar Execute(string sourceCode, params IVMScope[] scopes)
		{
			var lexer = new Lexer();
			var tokens = lexer.Lex(sourceCode);

			var parser = new Parser();
			var statement = parser.ParseInlineStatement(tokens);

			var vm = new ASTVM();

			foreach (var scope in scopes)
				vm.AddLinkedScope(scope);

			return vm.Execute(statement);
		}

		[TestMethod]
		public void VMNumberTest()
		{
			var result = Execute("10");

			Assert.AreEqual(VMType.Number, result.Type);
			Assert.AreEqual(10, result.ValueNumber);
		}

		[TestMethod]
		public void VMStringTest()
		{
			string textString = "test string by kdsbest";
			var result = Execute($"\"{textString}\"");

			Assert.AreEqual(VMType.String, result.Type);
			Assert.AreEqual(textString, result.ValueString);
		}

		[TestMethod]
		public void VMComplexFormula()
		{
			// 1 * 2 = 2
			// 3 % 4 = 3
			// -1 + 2 + 3 = 4
			var result = Execute("-1 + 1 * 2 + 3 % 4");

			Assert.AreEqual(VMType.Number, result.Type);
			Assert.AreEqual(4, result.ValueNumber);
			Assert.AreEqual(true, result.GetBool());

			result = Execute("(-1 + 1 * 2 + 3 % 3) == 1");

			Assert.AreEqual(VMType.Number, result.Type);
			Assert.AreEqual(-1, result.ValueNumber);
			Assert.AreEqual(true, result.GetBool());

			result = Execute("1 != (-1 + 1 * 2 + 3 % 3)");

			Assert.AreEqual(VMType.Number, result.Type);
			Assert.AreEqual(0, result.ValueNumber);
			Assert.AreEqual(false, result.GetBool());

			result = Execute("((-1 + 1 * 2 + 3) % 3) == 1");

			Assert.AreEqual(VMType.Number, result.Type);
			Assert.AreEqual(-1, result.ValueNumber);
			Assert.AreEqual(true, result.GetBool());

			result = Execute("((-1 + 1 * 2 + 3) % 3) == (-1 + 1 * 2 + 3 % 3)");

			Assert.AreEqual(VMType.Number, result.Type);
			Assert.AreEqual(-1, result.ValueNumber);
			Assert.AreEqual(true, result.GetBool());
		}

		[TestMethod]
		public void VMNumberAndStringTest()
		{
			var result = Execute("-1 && \"true\"");

			Assert.AreEqual(VMType.Number, result.Type);
			Assert.AreEqual(-1, result.ValueNumber);
			Assert.AreEqual(true, result.GetBool());
		}

		[TestMethod]
		public void VMStringAndNumberTest()
		{
			var result = Execute("\"true\" && -1");

			Assert.AreEqual(VMType.String, result.Type);
			Assert.AreEqual("true", result.ValueString);
			Assert.AreEqual(true, result.GetBool());
		}

		[TestMethod]
		public void VMStringXorNumberTest()
		{
			var result = Execute("\"true\" ^ -1");

			Assert.AreEqual(VMType.String, result.Type);
			Assert.AreEqual("false", result.ValueString);
			Assert.AreEqual(false, result.GetBool());
		}

		[TestMethod]
		public void VMNumberXorStringTest()
		{
			var result = Execute("-1 ^ \"true\"");

			Assert.AreEqual(VMType.Number, result.Type);
			Assert.AreEqual(0, result.ValueNumber);
			Assert.AreEqual(false, result.GetBool());
		}

		[TestMethod]
		public void VMScopeTest()
		{
			Mock<IVMScope> questMock = new Mock<IVMScope>();
			questMock.Setup(x => x.Scope).Returns("Quest");
			questMock.Setup(x => x.Execute(It.IsAny<string>(), It.IsAny<List<VMVar>>())).Returns(new VMVar()
			{
				Type = VMType.Number,
				ValueNumber = -1
			});

			var result = Execute("Quest.IsAvailable(\"TutorialQuest\")", questMock.Object);

			Assert.AreEqual(VMType.Number, result.Type);
			Assert.AreEqual(-1, result.ValueNumber);

			questMock.Verify(x => x.Execute("IsAvailable", new List<VMVar>()
			{
				new VMVar()
				{
					Type = VMType.String,
					ValueString = "TutorialQuest"
				}
			}), Times.Once);
		}

		[TestMethod]
		public void VMFullExampleCharacterIsLevel11Test()
		{
			Mock<IVMScope> questMock = new Mock<IVMScope>();
			questMock.Setup(x => x.Scope).Returns("Quest");
			questMock.Setup(x => x.Execute(It.IsAny<string>(), It.IsAny<List<VMVar>>())).Returns(new VMVar()
			{
				Type = VMType.Number,
				ValueNumber = -1
			});

			Mock<IVMScope> charcterMock = new Mock<IVMScope>();
			charcterMock.Setup(x => x.Scope).Returns("Character");
			charcterMock.Setup(x => x.Execute(It.IsAny<string>(), It.IsAny<List<VMVar>>())).Returns(new VMVar()
			{
				Type = VMType.Number,
				ValueNumber = 11
			});

			var result = Execute("Quest.IsAvailable(\"Tutorial\" + \"Quest\") && Character.Level() > 10", questMock.Object, charcterMock.Object);

			Assert.AreEqual(VMType.Number, result.Type);
			Assert.AreEqual(-1, result.ValueNumber);
			Assert.AreEqual(true, (bool)result);

			questMock.Verify(x => x.Execute("IsAvailable", new List<VMVar>()
			{
				new VMVar()
				{
					Type = VMType.String,
					ValueString = "TutorialQuest"
				}
			}), Times.Once);

			charcterMock.Verify(x => x.Execute("Level", new List<VMVar>()), Times.Once);
		}

		[TestMethod]
		public void VMFullExampleCharacterIsLevel9Test()
		{
			Mock<IVMScope> questMock = new Mock<IVMScope>();
			questMock.Setup(x => x.Scope).Returns("Quest");
			questMock.Setup(x => x.Execute(It.IsAny<string>(), It.IsAny<List<VMVar>>())).Returns(new VMVar()
			{
				Type = VMType.Number,
				ValueNumber = -1
			});

			Mock<IVMScope> charcterMock = new Mock<IVMScope>();
			charcterMock.Setup(x => x.Scope).Returns("Character");
			charcterMock.Setup(x => x.Execute(It.IsAny<string>(), It.IsAny<List<VMVar>>())).Returns(new VMVar()
			{
				Type = VMType.Number,
				ValueNumber = 9
			});

			var result = Execute("Quest.IsAvailable(\"TutorialQuest\") && Character.Level() > 10", questMock.Object, charcterMock.Object);

			Assert.AreEqual(VMType.Number, result.Type);
			Assert.AreEqual(0, result.ValueNumber);
			Assert.AreEqual(false, (bool)result);

			questMock.Verify(x => x.Execute("IsAvailable", new List<VMVar>()
			{
				new VMVar()
				{
					Type = VMType.String,
					ValueString = "TutorialQuest"
				}
			}), Times.Once);

			charcterMock.Verify(x => x.Execute("Level", new List<VMVar>()), Times.Once);
		}
	}
}

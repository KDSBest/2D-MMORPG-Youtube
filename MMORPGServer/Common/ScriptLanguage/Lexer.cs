using Common.ScriptLanguage.Tokens;
using System.Collections.Generic;

namespace Common.ScriptLanguage
{

	/// <summary>
	/// Creates a StringToken Stream Out of a Source Code "String".
	/// The Lexer follows the Keep It Simple Stupid Prinicle.
	/// It just detects unterminated Strings.
	/// It's not a class on it's own, it just extends the compiler 
	/// with Extension Method to reduce the complexity of the Compiler class.
	/// </summary>
	public class Lexer
	{
		private string currentToken = string.Empty;

		private int lineNumber;

		public List<LexerToken> Tokens { get; private set; }

		private List<string> reservedWords = new List<string> { "function", "class",
												  "string", "number", "object", "void",
												  "new",
												  "this", "VM",
												  "if", "while", "for", "return" };

		private string singleLineComment = "//";

		private string multiLineCommentStart = "/*";

		private string multiLineCommentEnd = "*/";

		private List<char> ignoredToken = new List<char> { ' ', '\t', '\r' };

		private List<char> specialTokens = new List<char> { '(', ')', '{', '}', '[', ']', ';', '.', ',' };

		private List<char> operators = new List<char>{
											 '-', '+', '*', '/', '=',
											 '&', '|', '!', '%', '<', '>'
										 };

		private Dictionary<char, char> escapeSequenzes = new Dictionary<char, char> {
													{'n', '\n' },
													{'\\', '\\' },
													{'t', '\t' },
													{'r', '\r' },
													{'"', '\"' }
												};
		private char stringQuote = '"';

		private char escapeChar = '\\';

		private char newLine = '\n';

		private bool IsCharInTokenList(char c, List<char> tokens)
		{
			return tokens.Contains(c);
		}

		private bool IsIgnoredToken(char c)
		{
			return IsCharInTokenList(c, ignoredToken);
		}

		private bool IsSpecialToken(char c)
		{
			return IsCharInTokenList(c, specialTokens);
		}

		private bool IsOperator(char c)
		{
			return IsCharInTokenList(c, operators);
		}

		private bool IsCharDigit(char c)
		{
			if (c != '0' && c != '1' && c != '2' && c != '3' && c != '4' && c != '5' &&
				c != '6' && c != '7' && c != '8' && c != '9')
				return false;
			return true;
		}

		private char GetEscapeSequenze(char c)
		{
			if (!escapeSequenzes.ContainsKey(c))
				throw new LexerException($"Escape Sequenz is unknown.", lineNumber);

			return escapeSequenzes[c];
		}

		private void ValidateNumber(LexerToken token)
		{
			bool dot = false;

			for (int i = 0; i < token.Token.Length; i++)
			{
				if (token.Token[i] == '.')
				{
					if (dot)
						throw new LexerException(string.Format("Invalid Number '{0}' should only contain one '.'.", token.Token), token.LineNumber);
					else
						dot = true;
				}
				else if (!IsCharDigit(token.Token[i]))
					throw new LexerException(string.Format("Invalid Number '{0}' should only contain digits and one '.'.", token.Token), token.LineNumber);
			}
		}

		private void ValidateIdentifier(LexerToken token)
		{
			string lowerToken = token.Token.ToLower();
			for (int i = 0; i < lowerToken.Length; i++)
			{
				if (lowerToken[i] != '_' && lowerToken[i] != 'a' && lowerToken[i] != 'b' && lowerToken[i] != 'c' &&
					 lowerToken[i] != 'd' && lowerToken[i] != 'e' && lowerToken[i] != 'f' && lowerToken[i] != 'g' &&
					 lowerToken[i] != 'h' && lowerToken[i] != 'i' && lowerToken[i] != 'j' && lowerToken[i] != 'k' &&
					 lowerToken[i] != 'l' && lowerToken[i] != 'm' && lowerToken[i] != 'n' && lowerToken[i] != 'o' &&
					 lowerToken[i] != 'p' && lowerToken[i] != 'q' && lowerToken[i] != 'r' && lowerToken[i] != 's' &&
					 lowerToken[i] != 't' && lowerToken[i] != 'u' && lowerToken[i] != 'v' && lowerToken[i] != 'w' &&
					 lowerToken[i] != 'x' && lowerToken[i] != 'y' && lowerToken[i] != 'z' && !IsCharDigit(lowerToken[i]))
					throw new LexerException(string.Format("Identifier '{0}' has an invalid Name.", token.Token), token.LineNumber);
			}
		}

		private bool IsReservedWord(LexerToken token)
		{
			return reservedWords.Contains(token.Token);
		}

		private List<LexerToken> SplitSourceCode(string sourceCode)
		{
			this.Tokens = new List<LexerToken>();

			this.currentToken = "";
			this.lineNumber = 1;

			// Traverse Source Code by char
			for (int i = 0; i < sourceCode.Length; i++)
			{
				if (IsSpecialToken(sourceCode[i]))
				{
					FinishPreviousTokenIfNeeded();

					LexSpecialToken(sourceCode, i);
				}
				else if (IsIgnoredToken(sourceCode[i]))
				{
					FinishPreviousTokenIfNeeded();
				}

				// NOTE: this also handles comments because / is an operator
				else if (IsOperator(sourceCode[i]))
				{
					i = LexOperatorToken(sourceCode, i);
				}
				else if (sourceCode[i] == newLine)
				{
					FinishPreviousTokenIfNeeded();
					lineNumber++;
				}
				else if (sourceCode[i] == stringQuote)
				{
					FinishPreviousTokenIfNeeded();

					i = LexStringToken(sourceCode, i);
				}
				else
				{
					currentToken += sourceCode[i];
				}
			}

			FinishPreviousTokenIfNeeded();
			return Tokens;
		}

		private int LexOperatorToken(string sourceCode, int i)
		{
			FinishPreviousTokenIfNeeded();

			currentToken = "" + sourceCode[i];

			int ii = i + 1;
			if (ii >= sourceCode.Length)
			{
				throw new LexerException("Source Code can't end on an operator", lineNumber);
			}

			if (IsOperator(sourceCode[ii]))
			{
				// NOTE: ignore next character it is an operator so we got a 2 char operator
				i++;

				currentToken += sourceCode[ii];

				// Remove Single Line Comment
				if (currentToken == singleLineComment)
				{
					i = RemoveSingleLineComment(sourceCode, i);
				}
				else if (currentToken == multiLineCommentStart)
				{
					i = RemoveMultilineComment(sourceCode, i);
				}
				else
				{
					// add the operator to tokens
					Tokens.Add(new LexerToken(currentToken, lineNumber));
				}
			}
			else
			{
				// 1 char operator we add it
				Tokens.Add(new LexerToken(currentToken, lineNumber));
			}

			currentToken = "";

			return i;
		}

		private int RemoveMultilineComment(string sourceCode, int i)
		{
			for (++i; i < sourceCode.Length - 1; i++)
			{
				if (sourceCode[i] == multiLineCommentEnd[0] && sourceCode[i + 1] == multiLineCommentEnd[1])
				{
					// ignore the multiLineCommentEnd Part 2
					i++;
					break;
				}
				if (sourceCode[i] == '\n')
					lineNumber++;
			}

			return i;
		}

		private int RemoveSingleLineComment(string sourceCode, int i)
		{
			for (++i; i < sourceCode.Length; i++)
			{
				if (sourceCode[i] == newLine)
					break;
			}

			// Komment ends on new line preserve the new Line
			lineNumber++;
			return i;
		}

		private int LexStringToken(string sourceCode, int i)
		{
			// Note: 
			// we copy the "" in the string token, 
			// so the 2. stage lexer can identify a string or an identifier
			currentToken = "" + stringQuote;
			int ii = i + 1;
			for (; ii < sourceCode.Length && sourceCode[ii] != '"'; ii++)
			{
				if (sourceCode[ii] == newLine)
				{
					throw new LexerException("String is not terminated.", lineNumber);
				}
				else if (sourceCode[ii] == escapeChar)
				{
					int iii = ii + 1;
					if (iii < sourceCode.Length)
					{
						currentToken += "" + GetEscapeSequenze(sourceCode[iii]);
						ii++;
					}
					else
					{
						throw new LexerException("Source Code ends in an escape sequenz.", lineNumber);
					}
				}
				else
				{
					currentToken += "" + sourceCode[ii];
				}
			}

			// Check if we just reached the end of the source code.
			if (ii == sourceCode.Length)
				throw new LexerException("String is not terminated.", lineNumber);

			currentToken += stringQuote;

			// We point to the end of the string the '"'
			i = ii;


			Tokens.Add(new LexerToken(currentToken, lineNumber));
			currentToken = "";
			return i;
		}

		private void LexSpecialToken(string sourceCode, int i)
		{
			Tokens.Add(new LexerToken("" + sourceCode[i], lineNumber));
		}

		private void FinishPreviousTokenIfNeeded()
		{
			if (currentToken != "")
			{
				Tokens.Add(new LexerToken(currentToken, lineNumber));
				currentToken = "";
			}
		}

		public List<TokenBase> Lex(string sourceCode)
		{
			List<LexerToken> stringToken = SplitSourceCode(sourceCode);
			List<TokenBase> token = new List<TokenBase>();

			for (int i = 0; i < stringToken.Count; i++)
			{
				LexerToken sToken = stringToken[i];

				if (IsReservedWord(sToken))
				{
					token.Add(new ReservedWordToken(sToken.Token, sToken.LineNumber));
				}
				else if (IsOperator(sToken.Token[0]))
				{
					token.Add(new OperatorToken(sToken.Token, sToken.LineNumber));
				}
				else if (IsCharDigit(sToken.Token[0]))
				{
					if (stringToken.Count > i + 2)
					{
						// if digit + "." + digit, we have a double number
						if (stringToken[i + 1].Token == "." && IsCharDigit(stringToken[i + 2].Token[0]))
						{
							sToken.Token += stringToken[i + 1].Token + stringToken[i + 2].Token;
							i += 2;
						}
					}
					ValidateNumber(sToken);
					token.Add(new NumberToken(sToken.Token, sToken.LineNumber));
				}
				else if (sToken.Token[0] == '"')
				{
					token.Add(new StringToken(sToken.Token.Substring(1, sToken.Token.Length - 2), sToken.LineNumber));
				}
				else if (IsSpecialToken(sToken.Token[0]))
				{
					token.Add(new SpecialToken(sToken.Token, sToken.LineNumber));
				}
				else
				{
					ValidateIdentifier(sToken);
					token.Add(new IdentifierToken(sToken.Token, sToken.LineNumber));
				}
			}

			return token;
		}
	}
}

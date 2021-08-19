using Common.ScriptLanguage.AST;
using Common.ScriptLanguage.Tokens;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Common.ScriptLanguage
{
	public class Parser
    {
        private int currentToken;
        private List<TokenBase> currentTokens;
        private string currentClassName;

        private ASTNode ParseFunctionCallOrParameterAccess(string scope)
        {
            string functionNameOrParameter = currentTokens[currentToken].Token;
            currentToken++;
            if (currentTokens[currentToken].Token != "(")
            {
                return new ASTClassParameter()
                {
                    Scope = scope,
                    Parameter = new ASTStatement()
                    {
                        Statement = new ASTString()
                        {
                            String = functionNameOrParameter
                        }
                    }
                };
            }
            currentToken++;

            ASTFunctionCall ret = new ASTFunctionCall()
            {
                Scope = scope,
                FunctionName = functionNameOrParameter
            };

            while (currentTokens[currentToken].Token != ")")
            {
                ret.Parameter.Add(ParseStatement());
                if (currentTokens[currentToken].Token == ",")
                    currentToken++;
            }
            currentToken++;
            return ret;
        }

        private ASTNode ParseSingleStatement()
        {
            ASTNode ret = null;

            if (currentTokens[currentToken] is StringToken)
            {
                ret = new ASTString { String = currentTokens[currentToken].Token };
                currentToken++;
            }
            else if (currentTokens[currentToken] is NumberToken)
            {
                double dResult;
                if (double.TryParse(currentTokens[currentToken].Token, NumberStyles.Any, CultureInfo.GetCultureInfo("en"), out dResult))
                {
                    ret = new ASTNumber { Number = dResult };
                }
                else
                {
                    throw new ParserException(string.Format("Number {0} is not valid.", currentTokens[currentToken].Token), currentTokens[currentToken].LineNumber);
                }
                currentToken++;
            }
            else if (currentTokens[currentToken] is ReservedWordToken)
            {
                if (currentTokens[currentToken].Token == "this" || currentTokens[currentToken].Token == "VM")
                {
                    string scope = currentTokens[currentToken].Token;
                    currentToken++;
                    if (currentTokens[currentToken] is SpecialToken && currentTokens[currentToken].Token == ".")
                    {
                        currentToken++;
                        ret = ParseFunctionCallOrParameterAccess(scope);
                    }
                    else
                    {
                        throw new ParserException(string.Format("Expected '.' instead of '{0}'.", currentTokens[currentToken].Token), currentTokens[currentToken].LineNumber);
                    }
                }
                else if (currentTokens[currentToken].Token == "object" ||
                    currentTokens[currentToken].Token == "string" ||
                    currentTokens[currentToken].Token == "number" ||
                    currentTokens[currentToken].Token == "void")
				{
					VMType type = GetVMType(currentTokens[currentToken].Token);
					currentToken++;
					if (currentTokens[currentToken] is IdentifierToken)
					{
						ret = new ASTLocalVariable()
						{
							Type = type,
							Name = currentTokens[currentToken].Token
						};
					}
					else
					{
						throw new ParserException("Expected Identifier.", currentTokens[currentToken].LineNumber);
					}
					currentToken++;
				}
				else if (currentTokens[currentToken].Token == "new")
                {
                    currentToken++;
                    ASTNode createClass = ParseFunctionCallOrParameterAccess("constructor");
                    if (createClass is ASTFunctionCall)
                    {
                        ASTFunctionCall fc = (ASTFunctionCall)createClass;
                        ret = new ASTCreateClass()
                        {
                            Scope = fc.FunctionName,
                            FunctionName = fc.FunctionName,
                            Parameter = fc.Parameter
                        };
                    }
                    else
                    {
                        throw new ParserException("Error with new.", currentTokens[currentToken].LineNumber);
                    }
                }
                else if (currentTokens[currentToken].Token == "return")
                {
                    currentToken++;
                    ret = new ASTReturn()
                    {
                        Return = ParseStatement()
                    };

                    // Point to ';' again!!!
                    --currentToken;
                }
                else
                {
                    throw new ParserException(string.Format("Reserved Word {0} is not legal in Statement.", currentTokens[currentToken].Token), currentTokens[currentToken].LineNumber);
                }
            }
            else if (currentTokens[currentToken] is IdentifierToken)
            {
                string identifierOrScope = currentTokens[currentToken].Token;
                currentToken++;
                if (currentTokens[currentToken] is SpecialToken)
                {
                    if (currentTokens[currentToken].Token == ".")
                    {
                        currentToken++;
                        TokenBase isFunction = currentTokens[currentToken + 1];
                        ret = ParseFunctionCallOrParameterAccess(identifierOrScope);
                    }
                    else if (currentTokens[currentToken].Token == "(")
                    {
                        // Set focus again to the function name
                        --currentToken;
                        ret = ParseFunctionCallOrParameterAccess("this");
                    }
                    else if (currentTokens[currentToken].Token == "[")
                    {
                        currentToken++;
                        ASTStatement statement = ParseStatement();
                        if (statement == null)
                            throw new ParserException("Expected Statement for Parameter Access.", currentTokens[currentToken].LineNumber);
                        ret = new ASTClassParameter()
                        {
                            Scope = identifierOrScope,
                            Parameter = statement
                        };
                    }
                    else
                    {
                        ret = new ASTIdentifier() { Identifier = identifierOrScope };
                    }
                }
                else
                {
                    ret = new ASTIdentifier() { Identifier = identifierOrScope };
                }
            }

            if (ret == null)
                throw new ParserException($"Problems parsing the Identifier/Function Call/String/Number {currentTokens[currentToken].Token}.", currentTokens[currentToken].LineNumber);
            return ret;
        }

		private VMType GetVMType(string token)
		{
            switch(token)
			{
                case "number":
                    return VMType.Number;
                case "string":
                    return VMType.String;
                case "object":
                    return VMType.Object;
                default:
                    return VMType.Void;
			}
		}

		private ASTStatement ParseStatement()
        {
            List<ASTNode> nodes = new List<ASTNode>();
            ASTNode currentNode = null;

            int countClauses = 0;

            for (; currentToken < currentTokens.Count; currentToken++)
            {
                if (currentTokens[currentToken] is SpecialToken)
                {
                    if (
                        // use case new code block aka statement ends
                        currentTokens[currentToken].Token == "{" 
                        
                        // next parameter aka Character.HasLevel(1, 2) then the , is not part of the statement
                        || currentTokens[currentToken].Token == ",")
                    {
                        break;
                    }

                    // ; and ] end statement but are skipped for further parsing
                    if (currentTokens[currentToken].Token == ";" || currentTokens[currentToken].Token == "]")
                    {
                        currentToken++;
                        break;
                    }

                    if (currentTokens[currentToken].Token == "(")
                    {
                        countClauses++;
                        currentNode = new ASTOperator()
                        {
                            Operator = "(",
                            Precedence = -1,
                            Left = null,
                            Right = null
                        };
                    }
                    else if (currentTokens[currentToken].Token == ")")
                    {
                        // use case Character.HasLevel(1 + 3) then the clauses () are part of the function call not the statement
                        if (countClauses <= 0)
                            break;

                        --countClauses;
                    }
                }

                if (currentTokens[currentToken] is OperatorToken)
                {
                    // 2 Operators means at least one is an unary one
                    if (currentTokens.Count > currentToken + 1 && currentTokens[currentToken + 1] is OperatorToken)
                    {
                        // Example case "1++ - 2"
                        if (currentTokens[currentToken].Token == "++" || currentTokens[currentToken].Token == "--")
                        {
                            nodes.RemoveAt(nodes.Count - 1);
                            ASTNode left = currentNode;
                            currentNode = new ASTOperator()
                            {
                                Operator = currentTokens[currentToken].Token,
                                Precedence = 0,
                                Left = left,
                                Right = null
                            };
                            nodes.Add(currentNode);

                            // 2. Operator
                            currentToken++;
                            string op = currentTokens[currentToken].Token;
                            int precedence = ((OperatorToken)currentTokens[currentToken]).Precedence;
                            currentNode = new ASTOperator()
                            {
                                Operator = op,
                                Precedence = precedence,
                                Left = null,
                                Right = null
                            };
                        }
                        // case "1 - ++2"
                        else
                        {
                            currentNode = new ASTOperator()
                            {
                                Operator = currentTokens[currentToken].Token,
                                Precedence = 0,
                                Left = null,
                                Right = null
                            };
                            nodes.Add(currentNode);

                            // 2. Operator
                            currentToken++;
                            string op = currentTokens[currentToken].Token;
                            int precedence = ((OperatorToken)currentTokens[currentToken]).Precedence;
                            currentToken++;
                            currentNode = new ASTOperator()
                            {
                                Operator = op,
                                Precedence = precedence,
                                Left = ParseSingleStatement(),
                                Right = null
                            };

                            //for will increase and ParseSingleStatement will do also
                            --currentToken;
                        }
                    }
                    // Statement starts with an unary operator example ("-1 * 2") || "(-1+2)*5"
                    else if (currentToken == 0 || (currentTokens[currentToken - 1] is SpecialToken && currentTokens[currentToken - 1].Token == "("))
                    {
                        string op = currentTokens[currentToken].Token;
                        currentToken++;
                        currentNode = new ASTOperator()
                        {
                            Operator = op,
                            Precedence = 0,
                            Left = ParseSingleStatement(),
                            Right = null
                        };
                        //for will increase and ParseSingleStatement will do also
                        --currentToken;
                    }
                    else
                    {
                        currentNode = new ASTOperator()
                        {
                            Operator = currentTokens[currentToken].Token,
                            Precedence = ((OperatorToken)currentTokens[currentToken]).Precedence,
                            Left = null,
                            Right = null
                        };
                    }
                }
                else
                {
                    currentNode = ParseSingleStatement();

                    //for will increase and ParseSingleStatement will do also
                    --currentToken;
                }
                nodes.Add(currentNode);
            }

            // Prevent being stuck at a ;
            //            if (currentTokens[currentToken].Token == ";")
            //                currentToken++;

            if (nodes.Count > 1)
			{
				return ApplyOperatorPrecedences(nodes);
			}
			else if (nodes.Count == 1)
            {
                return new ASTStatement() { Statement = nodes[0] };
            }
            else
            {
                return null;
            }
        }

		private ASTStatement ApplyOperatorPrecedences(List<ASTNode> nodes)
		{
            // Example Precedences
            // +: 3
            // *: 2

            // Algorithm to take care of operator precedence
            // Rule 1: We process the statement right to left so in 2 - (1 + 2) * 3 we start with the number 3
            // Rule 2: We add Statements and Operators to a Stack for each
            // Rule 3: ) are ignored
            // Rule 4: If we reach ( then we "Process the Operator Stack"
            // Rule 5: If a the last operator precedense was lower than ours then we "Process the Operator Stack"
            // Rule 6: end of statement we "Process the Operator Stack"
            // Rule 7: 1++ or i++ will result in starting with an operator instead of a parameter, so we have to handle that with adding a tree like "++ - 1" or "++ - i" as parameter
            // Rule 8: If not Rule 7 applies then we have Parameter and Operators after each other starting with a parameter

            // Process the Operator Stack means we Build the tree of operators from left to right (because we use a LIFO Stack)
            // e.g. 1 * 2 + 3 creates a stack like
            // Parameters: 3, 2, 1
            // Operators: +, *
            // First operator is * so tree starts like this
            // * -  1
            //   |- 2
            //
            // this is added as Parameter
            // Next operator is + which results to a tree like
            // + -  *  -  1
            //   |     |- 2
            //   |- 3
            //
            // e.g. 3 + 2 * 1 should result in the same tree
            // since + has a higher precedence than * when we reach the + we process the stack which is
            // Parameters: 2, 3
            // Operator: *
            // New Parameter is added as
            // * -  1
            //   |- 2
            //
            // Next it will process till end and the stack will look like this
            // Parameters: * (tree), 3
            // Operators: +
            // + - 3
            //   |- * -  1
            //      |--  2
            //

            Stack<ASTNode> parameters = new Stack<ASTNode>();
			Stack<ASTOperator> operators = new Stack<ASTOperator>();
			int lastPrecedence = int.MinValue;
			bool expectOperator = false;

			for (int i = nodes.Count - 1; i >= 0; i--)
			{
				if (nodes[i] is ASTOperator && ((ASTOperator)nodes[i]).Operator == "(")
				{
					// Build Operator Tree
					ProcessOperatorStack(parameters, operators);

					// After a ( always has to come an operator or nothing
					expectOperator = true;
				}
				else
				{
					if (expectOperator)
					{
						if (nodes[i] is ASTOperator)
						{
							ASTOperator op = (ASTOperator)nodes[i];
							if (lastPrecedence < op.Precedence)
							{
								ProcessOperatorStack(parameters, operators);
							}

							lastPrecedence = op.Precedence;

							operators.Push(op);
						}
						else
						{
                            // Special case for e.g. i++ where we start with an operator then the operator got pushed as an parameter and we have to switch the expectation
							if (parameters.Count == 1)
							{
								ASTNode node = parameters.Pop();

								if (node is ASTOperator)
								{
									ASTOperator op = (ASTOperator)node;
									op.Left = nodes[i];
									parameters.Push(op);
									continue;
								}
							}

							// This should never ever happen, because else earlier a exception should be raised
							throw new ParserException("Expected an operator.", -1);
						}
					}
					else
					{
						parameters.Push(nodes[i]);
					}
					expectOperator = !expectOperator;
				}
			}

			ProcessOperatorStack(parameters, operators);

			return new ASTStatement() { Statement = parameters.Pop() };
		}

		private List<ASTNode> ParseCodeBlockOrSingleLineStatement()
        {
            List<ASTNode> CodeBlock;
            if (currentTokens[currentToken].Token == "{")
            {
                CodeBlock = ParseCodeBlock();
                if (currentTokens[currentToken].Token == "}")
                    currentToken++;
            }
            else
            {
                CodeBlock = new List<ASTNode>();
                CodeBlock.Add(ParseStatement());
            }
            return CodeBlock;
        }

        private ASTWhile ParseWhile()
        {
            currentToken++;

            if (currentTokens[currentToken].Token == "(")
            {
                currentToken++;
                ASTStatement condition = ParseStatement();
                List<ASTNode> codeBlock;
                currentToken++;
                codeBlock = ParseCodeBlockOrSingleLineStatement();

                return new ASTWhile()
                {
                    Condition = condition,
                    CodeBlock = codeBlock
                };
            }
            else
            {
                throw new ParserException("Expected ( after if.", currentTokens[currentToken].LineNumber);
            }
        }

        private ASTIf ParseIf()
        {
            currentToken++;

            if (currentTokens[currentToken].Token == "(")
            {
                currentToken++;
                ASTStatement ifCondition = ParseStatement();
                List<ASTNode> ifCodeBlock;
                List<ASTNode> elseCodeBlock;
                currentToken++;
                ifCodeBlock = ParseCodeBlockOrSingleLineStatement();
                if (currentTokens[currentToken].Token == "else")
                {
                    currentToken++;
                    elseCodeBlock = ParseCodeBlockOrSingleLineStatement();
                }
                else
                {
                    elseCodeBlock = null;
                }

                return new ASTIf()
                {
                    Condition = ifCondition,
                    CodeBlock = ifCodeBlock,
                    ElseCodeBlock = elseCodeBlock
                };
            }
            else
            {
                throw new ParserException("Expected ( after if.", currentTokens[currentToken].LineNumber);
            }
        }

        private ASTFor ParseFor()
        {
            currentToken++;

            if (currentTokens[currentToken].Token == "(")
            {
                currentToken++;
                ASTStatement forInitialise = null;
                ASTStatement forCondition = null;
                ASTStatement forCount = null;
                List<ASTNode> codeBlock;


                if (currentTokens[currentToken].Token != ";")
                {
                    forInitialise = ParseStatement();
                }
                else
                {
                    currentToken++;
                }

                if (currentTokens[currentToken].Token != ";")
                {
                    forCondition = ParseStatement();
                }
                else
                {
                    currentToken++;
                }

                if (currentTokens[currentToken].Token != ")")
                {
                    forCount = ParseStatement();
                }
                currentToken++;

                codeBlock = ParseCodeBlockOrSingleLineStatement();

                return new ASTFor()
                {
                    Initialise = forInitialise,
                    Condition = forCondition,
                    Counter = forCount,
                    CodeBlock = codeBlock
                };
            }
            else
            {
                throw new ParserException("Expected ( after for.", currentTokens[currentToken].LineNumber);
            }
        }

        private List<ASTNode> ParseCodeBlock()
        {
            List<ASTNode> codeBlock = new List<ASTNode>();
            if (currentToken < currentTokens.Count)
            {
                if (currentTokens[currentToken].Token == "{")
                {
                    currentToken++;

                    while (currentToken < currentTokens.Count && currentTokens[currentToken].Token != "}")
                    {
                        if (currentTokens[currentToken] is ReservedWordToken && currentTokens[currentToken].Token == "if")
                        {
                            ASTIf ifNode = ParseIf();
                            codeBlock.Add(ifNode);
                        }
                        else if (currentTokens[currentToken] is ReservedWordToken && currentTokens[currentToken].Token == "for")
                        {
                            ASTFor forNode = ParseFor();
                            codeBlock.Add(forNode);
                        }
                        else if (currentTokens[currentToken] is ReservedWordToken && currentTokens[currentToken].Token == "while")
                        {
                            ASTWhile whileNode = ParseWhile();
                            codeBlock.Add(whileNode);
                        }
                        else
                        {
                            ASTStatement statement = ParseStatement();
                            if (statement != null)
                                codeBlock.Add(statement);
                        }
                    }

                    if (currentToken == currentTokens.Count)
                        throw new ParserException("SourceCode can't end in Code Block.", -1);

                    return codeBlock;
                }
                // No '{' means just one line of code
                else
                {
                    ASTStatement statement = ParseStatement();

                    if (statement != null)
                        codeBlock.Add(statement);

                    return codeBlock;
                }
            }
            throw new ParserException("Expected Code Block found End of File.", -1);
        }

        private List<ASTNode> ParseFunctionParameter()
        {
            if (currentTokens[currentToken].Token == "(")
            {
                currentToken++;
                List<ASTNode> parameter = new List<ASTNode>();
                while (currentTokens[currentToken].Token != ")")
                {
                    if (currentTokens[currentToken].Token == ",")
                        currentToken++;
                    switch (currentTokens[currentToken].Token)
                    {
                        case "number":
                            currentToken++;
                            parameter.Add(new ASTFunctionParameter()
                            {
                                Name = currentTokens[currentToken].Token,
                                Type = VMType.Number
                            });
                            currentToken++;
                            break;
                        case "string":
                            currentToken++;
                            parameter.Add(new ASTFunctionParameter()
                            {
                                Name = currentTokens[currentToken].Token,
                                Type = VMType.String
                            });
                            currentToken++;
                            break;
                        case "object":
                            currentToken++;
                            parameter.Add(new ASTFunctionParameter()
                            {
                                Name = currentTokens[currentToken].Token,
                                Type = VMType.Object
                            });
                            currentToken++;
                            break;
                        default:
                            throw new ParserException("Expected parameter type.", currentTokens[currentToken].LineNumber);
                    }
                }
                currentToken++;
                return parameter;
            }
            else
            {
                throw new ParserException("Expected '('.", currentTokens[currentToken].LineNumber);
            }
        }

        private ASTFunction ParseFunction()
        {
            VMType returnType = VMType.None;
            if (currentTokens[currentToken] is ReservedWordToken)
            {
                returnType = GetVMType(currentTokens[currentToken].Token);

                currentToken++;
            }
            //else it is a constructor

            if (currentTokens[currentToken] is IdentifierToken)
            {
                string name = currentTokens[currentToken].Token;
                currentToken++;
                List<ASTNode> parameter = ParseFunctionParameter();
                List<ASTNode> codeBlock = ParseCodeBlock();
                currentToken++;

                if (returnType == VMType.None)
                {
                    if (name != currentClassName)
                    {
                        throw new ParserException(string.Format("Constructor '{0}' needs the same name as Class '{1}'.", name, currentClassName), currentTokens[currentToken].LineNumber);
                    }
                    return new ASTConstructor()
                    {
                        Name = name,
                        Parameter = parameter,
                        CodeBlock = codeBlock,
                        ReturnValue = returnType
                    };
                }

                return new ASTFunction()
                {
                    Name = name,
                    Parameter = parameter,
                    CodeBlock = codeBlock,
                    ReturnValue = returnType
                };
            }
            else
            {
                throw new ParserException("Expected Function Name.", currentTokens[currentToken].LineNumber);
            }
        }

        public ASTStatement ParseInlineStatement(List<TokenBase> tokens)
		{
            currentTokens = tokens;
            currentToken = 0;
            return ParseStatement();
		}

        public List<ASTClass> Parse(List<TokenBase> tokens)
        {
            currentTokens = tokens;
            currentToken = 0;
            List<ASTClass> classes = new List<ASTClass>();
            while (currentToken < currentTokens.Count)
            {
                if (currentTokens[currentToken].Token == "class")
                {
                    currentToken++;

                    currentClassName = currentTokens[currentToken].Token;
                    currentToken++;

                    if (currentTokens[currentToken].Token != "{")
                        throw new ParserException("Expected '{'.", currentTokens[currentToken].LineNumber);
                    currentToken++;

                    List<ASTConstructor> constructors = new List<ASTConstructor>();
                    List<ASTFunction> functions = new List<ASTFunction>();
                    List<ASTClassAttribute> attributes = new List<ASTClassAttribute>();
                    while (currentToken < currentTokens.Count)
                    {
                        if (currentTokens[currentToken].Token == "function")
                        {
                            currentToken++;
                            ASTFunction func = ParseFunction();
                            if (func is ASTConstructor)
                            {
                                constructors.Add((ASTConstructor)func);
                            }
                            else
                            {
                                functions.Add(func);
                            }
                        }
                        else if (currentTokens[currentToken].Token == "number" ||
                            currentTokens[currentToken].Token == "string" ||
                            currentTokens[currentToken].Token == "object")
                        {
                            ASTNode node = ParseSingleStatement();
                            if (node is ASTLocalVariable)
                            {
                                ASTLocalVariable lv = (ASTLocalVariable)node;
                                attributes.Add(new ASTClassAttribute()
                                {
                                    Type = lv.Type,
                                    Name = lv.Name
                                });

                                if (currentTokens[currentToken].Token == ";")
                                {
                                    currentToken++;
                                }
                                else
                                {
                                    throw new ParserException("Expected ';'.", currentTokens[currentToken].LineNumber);
                                }
                            }
                            else
                            {
                                throw new ParserException("Expected '}' or attribute or function.", currentTokens[currentToken].LineNumber);
                            }
                        }
                        else if (currentTokens[currentToken].Token == "}")
                        {
                            currentToken++;

                            classes.Add(new ASTClass()
                            {
                                Name = currentClassName,
                                Constructors = constructors,
                                Functions = functions,
                                Attributes = attributes
                            });

                            break;
                        }
                        else
                        {
                            throw new ParserException("Expected '}' or attribute or function.", currentTokens[currentToken].LineNumber);
                        }
                    }
                }
                else
                {
                    throw new ParserException("Expected class.", currentTokens[currentToken].LineNumber);
                }
            }
            return classes;
        }

        /// <summary>
        /// Is explained in detail in ApplyOperatorPrecedences.
        /// Creates a tree based on the parameters and operators and adds it as Parameter back to the stack
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="operators"></param>
        private void ProcessOperatorStack(Stack<ASTNode> parameters, Stack<ASTOperator> operators)
        {
            while (operators.Count > 0)
            {
                ASTNode left = parameters.Pop();
                ASTNode right = null;
                if (parameters.Count > 0)
                    right = parameters.Pop();
                ASTOperator op = operators.Pop();
                op.Left = left;
                op.Right = right;
                parameters.Push(op);
            }
        }
    }
}

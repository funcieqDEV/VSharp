using System;
using System.Linq;
using System.Collections.Generic;

namespace VSharp
{
    public class Interpreter
    {
        private Dictionary<string, object> _variables = new Dictionary<string, object>(); 
        private Dictionary<string, FuncStatementNode> _functions = new Dictionary<string, FuncStatementNode>(); 
        private Dictionary<string, object> _localVariables = new Dictionary<string, object>(); 

        public void Interpret(ProgramNode program)
        {
            foreach (var statement in program.Statements)
            {
                ExecuteStatement(statement);
            }
        }

        private void ExecuteStatement(ASTNode node)
        {
            switch (node)
            {
                case SetStatementNode setStmt:
                    ExecuteSetStatement(setStmt);
                    break;

                case PrintStatementNode printStmt:
                    ExecutePrintStatement(printStmt);
                    break;
                case InputStatementNode inputStmt:
                    ExecuteInputStatement(inputStmt);
                    break;
                case IfNode ifStmt:
                    ExecuteIfStatement(ifStmt);
                    break;
                case PrintlnStatementNode p:
                    ExecutePrintlnStatement(p);
                    break;
                case WhileStatementNode w:
                    ExecuteWhileStatemnt(w);
                    break;
                case ConvertToIntStatamentNode cti:
                    ExecuteConvertToIntStatement(cti);
                    break;
                case ForgroundColorStatementNode color:
                    ExecuteForgroundColorStatement(color);
                    break;
                case FuncStatementNode func:
                    ExecuteFuncStatement(func);
                    break;
                case FuncCallNode funcCall:
                    ExecuteFuncCall(funcCall);
                    break;
                default:
                    
                    break;
            }
        }

        private void ExecuteFuncCall(FuncCallNode funcCall)
        {
            var funcName = funcCall.funcName;

         
            if (!_functions.ContainsKey(funcName))
            {
                throw new Exception($"Undefined function: {funcName}");
            }

            var func = _functions[funcName];
            var funcArgs = func.args.names;
            var callArgs = funcCall.args;

            if (funcArgs.Count != callArgs.Count)
            {
                throw new Exception("Argument count mismatch.");
            }

       
            _localVariables = new Dictionary<string, object>();

         
            for (int i = 0; i < funcArgs.Count; i++)
            {
                var argName = funcArgs[i];
                var argValue = EvaluateExpression(callArgs[i]);
                _localVariables[argName] = argValue;
            }

            foreach (var stmt in func.Block.Statements)
            {
                ExecuteStatement(stmt);
            }

         
            _localVariables.Clear();
        }


        private void ExecuteFuncSetNode(ASTNode v, string fName)
        {
            var setStmt = (SetStatementNode)v;
            var varName = setStmt.VariableName;
            _localVariables[varName] = EvaluateExpression(setStmt.Expression);
        }

        private void ExecuteFuncStatement(FuncStatementNode funcStmt)
        {
            _functions[funcStmt.name] = funcStmt;
        }

        private void ExecuteSetStatement(SetStatementNode setStmt)
        {
            object value = EvaluateExpression(setStmt.Expression);
            _variables[setStmt.VariableName] = value;
        }

        private void ExecutePrintStatement(PrintStatementNode printStmt)
        {
            object value = EvaluateExpression(printStmt.Expression);
            Console.Write(value);
        }

        private void ExecuteInputStatement(InputStatementNode inputStmt)
        {
            string VarName = inputStmt.VarName;
            _variables[VarName] = Console.ReadLine();
        }

        private void ExecutePrintlnStatement(PrintlnStatementNode println)
        {
            object value = EvaluateExpression(println.Expression);
            Console.WriteLine(value);
        }

        private void ExecuteWhileStatemnt(WhileStatementNode whileStmt)
        {
            bool result = EvaluateConditions(whileStmt.Conditions);
            while (result)
            {
                foreach (var statement in whileStmt.TrueBlock.Statements)
                {
                    ExecuteStatement(statement);
                }
                result = EvaluateConditions(whileStmt.Conditions);
            }
        }

        private void ExecuteIfStatement(IfNode ifStmt)
        {
            bool result = EvaluateConditions(ifStmt.Conditions);

            if (result)
            {
                foreach (var statement in ifStmt.TrueBlock.Statements)
                {
                    ExecuteStatement(statement);
                }
            }
            else
            {
                foreach (var statement in ifStmt.FalseBlock.Statements)
                {
                    ExecuteStatement(statement);
                }
            }
        }

        private void ExecuteConvertToIntStatement(ConvertToIntStatamentNode cti)
        {
            var value = EvaluateExpression(cti.Expr);
            var name = cti.VarName;
            _variables[name] = Convert.ToInt32(value);
        }

        private void ExecuteForgroundColorStatement(ForgroundColorStatementNode color)
        {
            string colorName = Convert.ToString(EvaluateExpression(color.ColorName));
            colorName = colorName.ToLower();
            switch (colorName)
            {
                case "black":
                    Console.ForegroundColor = ConsoleColor.Black;
                    break;
                case "green":
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;
                case "yellow":
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case "blue":
                    Console.ForegroundColor = ConsoleColor.Blue;
                    break;
                case "red":
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case "gray":
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                case "white":
                    Console.ForegroundColor = ConsoleColor.Black;
                    break;
                case "cyan":
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    break;
                case "magenta":
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    break;
                case "darkgray":
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    break;
            }
        }

        private bool EvaluateConditions(List<LogicalNode> conditions)
        {
            if (conditions == null || conditions.Count == 0)
            {
                return false;
            }

            bool result = EvaluateLogicalNode(conditions[0]);

            for (int i = 1; i < conditions.Count; i++)
            {
                Token op = conditions[i - 1].Operator;
                switch (op.Type)
                {
                    case TokenType.LogicalAnd:
                        result = result && EvaluateLogicalNode(conditions[i]);
                        break;
                    case TokenType.LogicalOr:
                        result = result || EvaluateLogicalNode(conditions[i]);
                        break;
                    default:
                        throw new Exception($"Unsupported logical operator: {op.Type}");
                }
            }

            return result;
        }

        private bool EvaluateLogicalNode(LogicalNode node)
        {
            object left = EvaluateExpression(node.Left);
            object right = EvaluateExpression(node.Right);

            if (!(left is IComparable) || !(right is IComparable))
            {
                throw new Exception("Operands are not comparable");
            }

            switch (node.Operator.Type)
            {
                case TokenType.LogicalAnd:
                    return Convert.ToBoolean(left) && Convert.ToBoolean(right);
                case TokenType.LogicalOr:
                    return Convert.ToBoolean(left) || Convert.ToBoolean(right);
                case TokenType.Greater:
                    return Comparer<object>.Default.Compare(left, right) > 0;
                case TokenType.Less:
                    return Comparer<object>.Default.Compare(left, right) < 0;
                case TokenType.Equal:
                    return Comparer<object>.Default.Compare(left, right) == 0;
                case TokenType.NotEqual:
                    return Comparer<object>.Default.Compare(left, right) != 0;
                case TokenType.GreaterEqual:
                    return Comparer<object>.Default.Compare(left, right) >= 0;
                case TokenType.LessEqual:
                    return Comparer<object>.Default.Compare(left, right) <= 0;
                default:
                    throw new Exception($"Unsupported logical operator: {node.Operator.Type}");
            }
        }


        private object EvaluateExpression(ASTNode node)
        {
            switch (node)
            {
                case LiteralNode literalNode:
                    return ParseLiteral(literalNode.Value);

                case IdentifierNode identifierNode:
                  
                    if (_localVariables.ContainsKey(identifierNode.Name))
                        return _localVariables[identifierNode.Name];

                    if (_variables.ContainsKey(identifierNode.Name))
                        return _variables[identifierNode.Name];

                    throw new Exception($"Undefined variable: {identifierNode.Name}");

                case BinaryOperationNode binaryOpNode:
                    return EvaluateBinaryOperation(binaryOpNode);

                default:
                    throw new Exception($"Unsupported AST node type: {node.GetType().Name}");
            }
        }

        private object EvaluateBinaryOperation(BinaryOperationNode binaryOpNode)
        {
            object left = EvaluateExpression(binaryOpNode.Left);
            object right = EvaluateExpression(binaryOpNode.Right);

            if (left is string leftStr || right is string rightStr)
            {
                if (binaryOpNode.Operator == "+")
                {
                    return left.ToString() + right.ToString();
                }
                throw new Exception($"Unsupported string operator: {binaryOpNode.Operator}");
            }

            if (left is int leftInt && right is int rightInt)
            {
                return binaryOpNode.Operator switch
                {
                    "==" => leftInt == rightInt,
                    "!=" => leftInt != rightInt,
                    ">" => leftInt > rightInt,
                    "<" => leftInt < rightInt,
                    ">=" => leftInt >= rightInt,
                    "<=" => leftInt <= rightInt,
                    "+" => leftInt + rightInt,
                    "-" => leftInt - rightInt,
                    "*" => leftInt * rightInt,
                    "/" => rightInt != 0 ? leftInt / rightInt : throw new DivideByZeroException(),
                    _ => throw new Exception($"Unsupported operator: {binaryOpNode.Operator}"),
                };
            }
            else if (left is double leftDouble && right is double rightDouble)
            {
                return binaryOpNode.Operator switch
                {
                    "==" => leftDouble == rightDouble,
                    "!=" => leftDouble != rightDouble,
                    ">" => leftDouble > rightDouble,
                    "<" => leftDouble < rightDouble,
                    ">=" => leftDouble >= rightDouble,
                    "<=" => leftDouble <= rightDouble,
                    "+" => leftDouble + rightDouble,
                    "-" => leftDouble - rightDouble,
                    "*" => leftDouble * rightDouble,
                    "/" => rightDouble != 0 ? leftDouble / rightDouble : throw new DivideByZeroException(),
                    _ => throw new Exception($"Unsupported operator: {binaryOpNode.Operator}"),
                };
            }
            else if (left is int intLeft && right is double doubleRight)
            {
                return binaryOpNode.Operator switch
                {
                    "==" => intLeft == doubleRight,
                    "!=" => intLeft != doubleRight,
                    ">" => intLeft > doubleRight,
                    "<" => intLeft < doubleRight,
                    ">=" => intLeft >= doubleRight,
                    "<=" => intLeft <= doubleRight,
                    "+" => (double)intLeft + doubleRight,
                    "-" => (double)intLeft - doubleRight,
                    "*" => (double)intLeft * doubleRight,
                    "/" => doubleRight != 0 ? (double)intLeft / doubleRight : throw new DivideByZeroException(),
                    _ => throw new Exception($"Unsupported operator: {binaryOpNode.Operator}"),
                };
            }
            else if (left is double doubleLeft && right is int intRight)
            {
                return binaryOpNode.Operator switch
                {
                    "==" => doubleLeft == intRight,
                    "!=" => doubleLeft != intRight,
                    ">" => doubleLeft > intRight,
                    "<" => doubleLeft < intRight,
                    ">=" => doubleLeft >= intRight,
                    "<=" => doubleLeft <= intRight,
                    "+" => doubleLeft + (double)intRight,
                    "-" => doubleLeft - (double)intRight,
                    "*" => doubleLeft * (double)intRight,
                    "/" => intRight != 0 ? doubleLeft / (double)intRight : throw new DivideByZeroException(),
                    _ => throw new Exception($"Unsupported operator: {binaryOpNode.Operator}"),
                };
            }
            else
            {
                throw new Exception("Type mismatch in binary operation.");
            }
        }



        private object EvaluateArithmeticOperation(int left, int right, string operatorSymbol)
        {
            return operatorSymbol switch
            {
                "+" => left + right,
                "-" => left - right,
                "*" => left * right,
                "/" => right != 0 ? left / right : throw new DivideByZeroException(),
                _ => throw new Exception($"Unsupported operator: {operatorSymbol}"),
            };
        }

        private object EvaluateArithmeticOperation(double left, double right, string operatorSymbol)
        {
            return operatorSymbol switch
            {
                "+" => left + right,
                "-" => left - right,
                "*" => left * right,
                "/" => right != 0 ? left / right : throw new DivideByZeroException(),
                _ => throw new Exception($"Unsupported operator: {operatorSymbol}"),
            };
        }

        private object ParseLiteral(string literal)
        {
            if (int.TryParse(literal, out int intValue))
            {
                return intValue;
            }
            else if (double.TryParse(literal, out double doubleValue))
            {
                return doubleValue;
            }
            else
            {
                return literal;
            }
        }
    }
}

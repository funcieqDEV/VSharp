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
                case PrintlnStatementNode printlnStmt:
                    ExecutePrintlnStatement(printlnStmt);
                    break;
                case WhileStatementNode whileStmt:
                    ExecuteWhileStatement(whileStmt);
                    break;
                case ConvertToIntStatementNode ctiStmt:
                    ExecuteConvertToIntStatement(ctiStmt);
                    break;
                case ForegroundColorStatementNode colorStmt:
                    ExecuteForegroundColorStatement(colorStmt);
                    break;
                case FuncStatementNode funcStmt:
                    ExecuteFuncStatement(funcStmt);
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
            var funcName = funcCall.FuncName;

            if (!_functions.ContainsKey(funcName))
            {
                throw new Exception($"Undefined function: {funcName}");
            }

            var func = _functions[funcName];
            var funcArgs = func.Args.Names;
            var callArgs = funcCall.Args;

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

        private void ExecuteFuncStatement(FuncStatementNode funcStmt)
        {
            _functions[funcStmt.Name] = funcStmt;
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
            string varName = inputStmt.VarName;
            _variables[varName] = Console.ReadLine();
        }

        private void ExecutePrintlnStatement(PrintlnStatementNode printlnStmt)
        {
            object value = EvaluateExpression(printlnStmt.Expression);
            Console.WriteLine(value);
        }

        private void ExecuteWhileStatement(WhileStatementNode whileStmt)
        {
            bool result = EvaluateCondition(whileStmt.Condition);
            while (result)
            {
                foreach (var statement in whileStmt.TrueBlock.Statements)
                {
                    ExecuteStatement(statement);
                }
                result = EvaluateCondition(whileStmt.Condition);
            }
        }

        private void ExecuteIfStatement(IfNode ifStmt)
        {
            bool result = EvaluateCondition(ifStmt.Condition);

            if (result)
            {
                foreach (var statement in ifStmt.TrueBlock.Statements)
                {
                    ExecuteStatement(statement);
                }
            }
            else
            {
                if (ifStmt.FalseBlock != null)
                {
                    foreach (var statement in ifStmt.FalseBlock.Statements)
                    {
                        ExecuteStatement(statement);
                    }
                }
            }
        }

        private void ExecuteConvertToIntStatement(ConvertToIntStatementNode ctiStmt)
        {
            var value = EvaluateExpression(ctiStmt.Expr);
            var name = ctiStmt.VarName;
            _variables[name] = Convert.ToInt32(value);
        }

        private void ExecuteForegroundColorStatement(ForegroundColorStatementNode colorStmt)
        {
            string colorName = Convert.ToString(EvaluateExpression(colorStmt.ColorName));
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
                    Console.ForegroundColor = ConsoleColor.White;
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
                default:
                    throw new Exception($"Unsupported color: {colorName}");
            }
        }

        private bool EvaluateCondition(ASTNode condition)
        {
            return condition switch
            {
                LogicalNode logicalNode => EvaluateLogicalNode(logicalNode),
                BinaryOperationNode binaryOpNode => Convert.ToBoolean(EvaluateBinaryOperation(binaryOpNode)),
                _ => throw new Exception($"Unsupported condition type: {condition.GetType().Name}")
            };
        }

        private bool EvaluateLogicalNode(LogicalNode node)
        {
            bool left = Convert.ToBoolean(EvaluateExpression(node.Left));
            bool right = Convert.ToBoolean(EvaluateExpression(node.Right));

            return node.Operator.Type switch
            {
                TokenType.LogicalAnd => left && right,
                TokenType.LogicalOr => left || right,
                _ => throw new Exception($"Unsupported logical operator: {node.Operator.Type}")
            };
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

            if (left is string || right is string)
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

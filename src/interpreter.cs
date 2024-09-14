using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.ComponentModel;
using System.Data;

namespace VSharp
{

    interface Invokable
    {
        object? Invoke(List<object?> args, Interpreter interpreter);
    }


    public class VariableNotFoundError : Exception 
    {
        public VariableNotFoundError(string message) : base(message)
        {
        }
    }

    public class Variables
    {
        private Variables? _parent;

        private Dictionary<string, object?> _variables;

        public Variables() 
        {
            _parent = null; 
            _variables = new Dictionary<string, object?>();
        }

        public bool HasVar(string name)
        {
            return _variables.ContainsKey(name) || (_parent?.HasVar(name) ?? false);
        }

        public void SetVar(string name, object? value) 
        {
            if (!_variables.ContainsKey(name) && (_parent?.HasVar(name) ?? false)) 
            {
                _parent.SetVar(name, value);
                return;
            }
            _variables[name] = value;
        }

        public object? GetVar(string name) 
        {
            if (_variables.ContainsKey(name)) 
            {
                return _variables[name];
            }

            if (_parent != null) 
            {
                return _parent.GetVar(name);
            }

            throw new VariableNotFoundError(name);
        }

        public Variables Child()
        {
            return new Variables { _parent = this, _variables = new Dictionary<string, object?>() };
        }
    };

    public class Function : Invokable {
        public required List<string> Args { get; set;}
        public required Expression Body {get; set;}

        public required Variables CurriedScope { get; set;}

        public object? Invoke(List<object?> args, Interpreter interpreter)
        {
            if (args.Count != Args.Count) 
            {
                throw new Exception("Invalid arg count");
            }
            Variables child = CurriedScope.Child();

            foreach (var (name, value) in Args.Zip(args)) 
            { 
                child.SetVar(name ?? "", value);
            }
            return interpreter.EvaluateExpression(Body, child);
        }
    }

    public class NativeFunc : Invokable {
        Func<List<object?>, object?> _closure;

        public static NativeFunc FromClosure(Func<List<object?>, object?> closure)
        {
            return new NativeFunc { _closure = closure };
        }

        public object? Invoke(List<object?> args, Interpreter interpreter)
        {
            return _closure(args);
        }
    }

    public class Interpreter 
    {
        public void Interpret(ProgramNode program)
        {
            Variables variables = StdLibFactory.StdLib();
            foreach (var statement in program.Statements)
            {
                ExecuteStatement(statement, variables);
            }
        }

        object? ExecuteStatement(ASTNode node, Variables variables)
        {
            switch (node)
            {
                case SetStatementNode setStmt:
                    ExecuteSetStatement(setStmt, variables);
                    break;

                case PrintStatementNode printStmt:
                    ExecutePrintStatement(printStmt, variables);
                    break;
                case InputStatementNode inputStmt:
                    ExecuteInputStatement(inputStmt, variables);
                    break;
    
                case PrintlnStatementNode printlnStmt:
                    ExecutePrintlnStatement(printlnStmt, variables);
                    break;
                case WhileStatementNode whileStmt:
                    ExecuteWhileStatement(whileStmt, variables);
                    break;
                case ConvertToIntStatementNode ctiStmt:
                    ExecuteConvertToIntStatement(ctiStmt, variables);
                    break;
                case ForegroundColorStatementNode colorStmt:
                    ExecuteForegroundColorStatement(colorStmt, variables);
                    break;
                case FuncStatementNode funcStmt:
                    ExecuteFuncStatement(funcStmt, variables);
                    break;
                case ExprStatement exprStatement:
                    return EvaluateExpression(exprStatement.Expression, variables);
                default:
                    throw new Exception("Unhandled statement" + node);
            }
            return null;
        }

        void ExecuteFuncStatement(FuncStatementNode funcStatement, Variables variables) 
        {
            Function function =  new Function { Args = funcStatement.Args.Names, Body = funcStatement.Block, CurriedScope= variables};
            variables.SetVar(funcStatement.Name, function);
        }


        void ExecuteSetStatement(SetStatementNode setStmt, Variables variables)
        {
            object? value = EvaluateExpression(setStmt.Expression, variables);
            variables.SetVar(setStmt.VariableName, value);
        }

        void ExecutePrintStatement(PrintStatementNode printStmt, Variables variables)
        {
            object? value = EvaluateExpression(printStmt.Expression, variables);
            Console.Write(value);
        }

        void ExecuteInputStatement(InputStatementNode inputStmt, Variables variables)
        {
            string varName = inputStmt.VarName;
            variables.SetVar(inputStmt.VarName, Console.ReadLine());
        }

        void ExecutePrintlnStatement(PrintlnStatementNode printlnStmt, Variables variables)
        {
            object? value = EvaluateExpression(printlnStmt.Expression, variables);
            Console.WriteLine(value);
        }

        void ExecuteWhileStatement(WhileStatementNode whileStmt, Variables variables)
        {
            while (EvaluateExpression(whileStmt.Condition, variables) as bool? ?? false)
            {
                EvaluateExpression(whileStmt.TrueBlock, variables);
            }
        }

        object? ExecuteIfStatement(IfNode ifStmt, Variables variables)
        {
            bool cond = EvaluateExpression(ifStmt.Condition, variables) as bool? ?? false;
            object? result = null;
            if (cond)
            {
                result = EvaluateExpression(ifStmt.TrueBlock, variables);
            }
            else
            {
                if (ifStmt.FalseBlock != null)
                {
                    result = EvaluateExpression(ifStmt.FalseBlock, variables);
                }
            }
            return result;
        }

        void ExecuteConvertToIntStatement(ConvertToIntStatementNode ctiStmt, Variables variables)
        {

            var value = EvaluateExpression(ctiStmt.Expr, variables);
            var name = ctiStmt.VarName;
            variables.SetVar(name, Convert.ToInt32(value));
        }

        void ExecuteForegroundColorStatement(ForegroundColorStatementNode colorStmt, Variables variables)
        {
            string colorName = EvaluateExpression(colorStmt.ColorName, variables) as string ?? "";
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

    

        public object? EvaluateExpression(Expression node, Variables variables)
        {
            switch (node)
            {
                case IdentifierNode identifierNode:
                    return variables.GetVar(identifierNode.Name);
                case BinaryOperationNode binaryOpNode:
                    return EvaluateBinaryOperation(binaryOpNode, variables);
                case ConstArray array: 
                    return LoadConstArray(array, variables);
                case ConstInt i:
                    return i.Value;
                case ConstDouble d:
                    return d.Value;
                case ConstString s:
                    return s.Value;
                case Invokation i:
                    return ExecuteInvokeOperation(i, variables);
                case BlockNode n:
                    return EvaluateBlockNode(n, variables);
                case IfNode i:
                    return ExecuteIfStatement(i, variables);
                default:
                    throw new Exception($"Unsupported AST node type: {node.GetType().Name}");
            }
        }


        object? EvaluateBlockNode(BlockNode block, Variables variables) 
        {
            object? result = null;
            foreach(var item in block.Statements)
            {
                result = ExecuteStatement(item, variables);
            }
            return result;
        }

        object? ExecuteInvokeOperation(Invokation invoke, Variables variables)
        {
            Invokable? parent = EvaluateExpression(invoke.Parent, variables) as Invokable;
            if (parent == null) {
                throw new Exception("Cannot invoke " + parent);
            }
            List<object?> evaluatedArgs = invoke.Args.Select(it => EvaluateExpression(it, variables)).ToList();
            return parent.Invoke(evaluatedArgs, this);
        }

        List<object?> LoadConstArray(ConstArray array, Variables variables)
        {
            List<object?> list = new List<object?>();
            foreach (Expression expr in array.Expressions) {
                list.Add(EvaluateExpression(expr, variables));
            }
            return list;
        }

        object EvaluateBinaryOperation(BinaryOperationNode binaryOpNode, Variables variables)
        {
            object? left = EvaluateExpression(binaryOpNode.Left, variables);
            object? right = EvaluateExpression(binaryOpNode.Right, variables);

            if (left is string leftString && right is string rightString)
            {
                return binaryOpNode.Operator switch
                {
                    "==" => leftString == rightString,
                    "!=" => leftString != rightString,
                    ">" => string.Compare(leftString, rightString) > 0,
                    "<" => string.Compare(leftString, rightString) < 0,
                    ">=" => string.Compare(leftString, rightString) >= 0,
                    "<=" => string.Compare(leftString, rightString) <= 0,
                    "+" => leftString + rightString, // Concatenation
                    _ => throw new Exception($"Unsupported operator for strings: {binaryOpNode.Operator}"),
                };
            }

            if (left is string || right is string)
            {
                if (binaryOpNode.Operator == "+")
                {
                    return left?.ToString() + right?.ToString();
                }
                throw new Exception($"Unsupported operator for mixed types involving strings: {binaryOpNode.Operator}");
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
    
    }

}
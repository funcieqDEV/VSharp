using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.ComponentModel;
using System.Data;
using System.Reflection;
using System.Dynamic;
using System.Runtime.InteropServices;

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

    public class VSharpObject
    {
        public required Dictionary<object, object?> Entries {get; set;}
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
                case PropertyAssignment pas:
                    ExecutePropertyAssignment(pas, variables);
                    break;
                case ForLoop loop:
                    ExecuteForLoop(loop, variables);
                    break;
                default:
                    throw new Exception("Unhandled statement" + node);
            }
            return null;
        }

        void ExecuteForLoop(ForLoop loop, Variables variables)
        {
            object parent = EvaluateExpression(loop.Parent, variables) ?? throw new Exception("Cannot iterate over null");
            if (parent is IEnumerable<object?> iter)
            {
                foreach (var item in iter)
                {
                    Variables child = variables.Child();
                    child.SetVar(loop.ItemName, item);
                    EvaluateExpression(loop.Body, child);
                }
            } else {
                throw new Exception($"Cannot iterate over {parent}");
            }
        }

        void ExecutePropertyAssignment(PropertyAssignment pas, Variables variables)
        {
            object parent = EvaluateExpression(pas.Parent, variables) ?? throw new Exception("Cannot set property on null");
            object? value = EvaluateExpression(pas.Value, variables);
            if (parent is VSharpObject vso) {
                vso.Entries[pas.Name] = value;
                return;
            }

            PropertyInfo info = parent.GetType().GetProperty(pas.Name) ?? throw new Exception("Property doesnt exist (on strict object)");
            info.SetValue(parent, value);
        }

        void ExecuteFuncStatement(FuncStatementNode funcStatement, Variables variables) 
        {
            Function function = new Function { Args = funcStatement.Args, Body = funcStatement.Block, CurriedScope= variables};
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
                EvaluateExpression(whileStmt.TrueBlock, variables.Child());
            }
        }

        object? ExecuteIfStatement(IfNode ifStmt, Variables variables)
        {
            bool cond = EvaluateExpression(ifStmt.Condition, variables) as bool? ?? false;
            object? result = null;
            if (cond)
            {
                result = EvaluateExpression(ifStmt.TrueBlock, variables.Child());
            }
            else
            {
                if (ifStmt.FalseBlock != null)
                {
                    result = EvaluateExpression(ifStmt.FalseBlock, variables.Child());
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
                case ConstObject o:
                    return LoadConstObject(o, variables);
                case MethodCall mc:
                    return EvaluateMethodCall(mc, variables);
                case Invokation i:
                    return ExecuteInvokeOperation(i, variables);
                case BlockNode n:
                    return EvaluateBlockNode(n, variables);
                case IfNode i:
                    return ExecuteIfStatement(i, variables);
                case PropertyAccess pa:
                    return EvaluatePropertyAccess(pa, variables);
                case ConstFunction func:
                    return new Function { Args = func.Args, Body = func.Body, CurriedScope = variables };
                case Indexing indexing:
                    return EvaluateIndexing(indexing, variables);
                default:
                    throw new Exception($"Unsupported AST node type: {node.GetType().Name}");
            }
        }

        object? EvaluateIndexing(Indexing indexing, Variables variables)
        {
            object parent = EvaluateExpression(indexing.Parent, variables) ?? throw new Exception("Cannot index into null");
            object index = EvaluateExpression(indexing.Index, variables) ?? throw new Exception("Cannot have null as the index");

            if (parent is List<object?> list && index is int i)
            {
                return list[i];
            }

            if (parent is VSharpObject obj)
            {
                return obj.Entries[index];
            }

            if (index is string s)
            {
                PropertyInfo info = parent.GetType().GetProperty(SnakeToPascal(s)) ?? throw new Exception("Property with given name doesnt exist");
                return info.GetValue(parent);
            }

            throw new Exception($"Cannot index {parent}[{index}]");
        }

        object? EvaluatePropertyAccess(PropertyAccess pa, Variables variables)
        {
            object? parent = EvaluateExpression(pa.Parent, variables);
            if (parent is VSharpObject o) 
            {
                return o.Entries[pa.Name];
            }

            Type type = parent?.GetType() ?? throw new Exception("Cannot access property on null");
            PropertyInfo info = type.GetProperty(SnakeToPascal(pa.Name)) ?? throw new Exception($"Property with given name {pa.Name} doesnt exist");

            return info.GetValue(parent);
        }

        static string SnakeToPascal(string snakeCaseString)
        {
            // Split the snake case string by underscores
            string[] words = snakeCaseString.Split('_');
            
            // Convert each word to Pascal case (capitalize first letter)
            for (int i = 0; i < words.Length; i++)
            {
                words[i] = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(words[i]);
            }
            
            // Join the words back together into a single string
            return string.Join("", words);
        }


        object? EvaluateMethodCall(MethodCall call, Variables variables)
        {
            object parent = EvaluateExpression(call.Parent, variables) 
                ?? throw new Exception("Cannot call method on null");

            // Evaluate arguments
            object?[] arguments = call.Args.Select(it => EvaluateExpression(it, variables)).ToArray();

            if (parent is VSharpObject obj) 
            {
                Invokable function = (obj.Entries[call.Name] as Invokable) ?? throw new Exception("No method found");
                return function.Invoke(arguments.ToList(), this);
            }

            // Convert method name from snake_case to PascalCase
            string methodName = SnakeToPascal(call.Name);

            // Get the argument types
            Type[] argTypes = arguments.Select(arg => arg?.GetType() ?? typeof(object)).ToArray();

            // Get all methods with the specified name
            MethodInfo[] methods = parent.GetType().GetMethods()
                .Where(m => m.Name == methodName)
                .ToArray();

            // Find the method that matches the argument types
            MethodInfo? methodInfo = methods.FirstOrDefault(m =>
            {
                ParameterInfo[] parameters = m.GetParameters();
                
                // Check if the parameter count matches
                if (parameters.Length != arguments.Length)
                    return false;

                // Check if each argument can be assigned to the corresponding parameter
                for (int i = 0; i < parameters.Length; i++)
                {
                    Type paramType = parameters[i].ParameterType;

                    // Check if the argument type is assignable to the parameter type, handle nulls as object
                    if (arguments[i] != null && !paramType.IsAssignableFrom(arguments[i]!.GetType()))
                    {
                        return false;
                    }
                }
                return true;
            });

            // If no matching method is found, throw an exception
            if (methodInfo == null)
                throw new Exception($"No method found with name {methodName} that matches the argument types on {parent}.");

            // Invoke the method and return the result
            return methodInfo.Invoke(parent, arguments);
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

        VSharpObject LoadConstObject(ConstObject obj, Variables variables)
        {
            Dictionary<object, object?> objectEntries = obj.Entries.ToDictionary(
                kvp => (object) kvp.Key, 
                kvp => EvaluateExpression(kvp.Value, variables)
            );
            return new VSharpObject { Entries = objectEntries };
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
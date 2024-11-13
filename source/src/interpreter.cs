using System.Globalization;
using System.Data;
using System.Reflection;
using System.Runtime.Serialization;

namespace VSharp
{

    interface Invokable
    {
        object? Invoke(List<object?> args, Interpreter interpreter);
    }


    public class VariableNotFoundError(string message) : Exception(message) 
    {
    }


    public abstract record TypeObject 
    {
        public abstract bool IsValid(TypeObject[] generics, object? value);
        public abstract TypeObject Unwind(TypeObject[] generics);

        private static Dictionary<string, Type> Primitives = new() {
            { "str", typeof(string) },
            { "i32", typeof(int) },
            { "int", typeof(int) },
            { "i64", typeof(long) },
            { "bool", typeof(bool) },
            { "f32", typeof(float) },
            { "f64", typeof(double) },
        };

        public static TypeObject FromVType(VType tp, IVariables variables, Interpreter interpreter, Dictionary<string, int> genericPos)
        {
            switch (tp)
            {
            case VType.Union union: 
                TypeObject[] unionTypes = union.Types.Select(it => FromVType(it, variables, interpreter, genericPos)).ToArray();
                return new Union(unionTypes);
            case VType.Intersection intersection: 
                TypeObject[] intersectionTypes = intersection.Types.Select(it => FromVType(it, variables, interpreter, genericPos)).ToArray();
                return new Intersection(intersectionTypes);
            case VType.Object obj:
                Dictionary<string, TypeObject> types = obj.Entires.ToDictionary(
                    kvp => kvp.Key,
                    kvp => FromVType(kvp.Value, variables, interpreter, genericPos)
                );
                return new Object(types);
            case VType.Array arr:
                return new Array(FromVType(arr.ItemType, variables, interpreter, genericPos));
            case VType.Normal normal:

                //in case its a generic
                if (normal.Type.Length == 1 )
                {
                    string name = normal.Type[0];
                    if (genericPos.ContainsKey(name))
                    {
                        return new Generic(genericPos[name]);
                    }
                    if (Primitives.ContainsKey(name))
                    {
                        return new Nominal(Primitives[name]);
                    }
                }

                TypeObject[] generics = normal.Generics.Select(it => FromVType(it, variables, interpreter, genericPos)).ToArray();

                TypeObject? typeObj = Resolve(normal.Type, variables, interpreter);
                if (typeObj != null)
                {
                    return typeObj.Unwind(generics);
                }

                string signature = string.Join(".", normal.Type);
                Type? result = Type.GetType(signature) ?? throw new Exception($"Invalid type signature `{signature}`");
                return new Nominal(result);
            default:
                throw new Exception("Unhadled type");
            }
        }

        static TypeObject? Resolve(string[] names, IVariables variables, Interpreter interpreter)
        {
            Expression node = new IdentifierNode(names[0]);

            if (names.Length >= 1)
            {
                foreach(var name in names[1..])
                {
                    node = new PropertyAccess { Name = name, Parent = node };
                }
            }
           
            try 
            {
                return interpreter.EvaluateExpression(node, variables) as TypeObject;
            } catch(Exception)
            {
                return null;
            }
        }


        public record Array(TypeObject ItemType) : TypeObject
        {
            public override bool IsValid(TypeObject[] generics, object? value)
            {
                if (value is not List<object?> list)
                {
                    return false;
                }

                return list.All(it => ItemType.IsValid(generics, it));
            }

            public override TypeObject Unwind(TypeObject[] generics)
            {
                return new Array(ItemType.Unwind(generics));
            }
        }

        public record Nominal(Type Type) : TypeObject
        {
            public override bool IsValid(TypeObject[] generics, object? value)
            {
                return Type.IsInstanceOfType(value);
            }

            public override TypeObject Unwind(TypeObject[] generics)
            {
                return this;
            }
        }

        public record Union(TypeObject[] Types) : TypeObject
        {
            public override bool IsValid(TypeObject[] generics, object? value)
            {
                foreach (var type in Types)
                {
                    if (type.IsValid(generics, value))
                    {
                        return true;
                    }
                }
                return false;
            }

            public override TypeObject Unwind(TypeObject[] generics)
            {
                return new Union(Types.Select(it => it.Unwind(generics)).ToArray());
            }
        }

        public record Intersection(TypeObject[] Types) : TypeObject
        {
            public override bool IsValid(TypeObject[] generics, object? value)
            {
                foreach (var type in Types)
                {
                    if (!type.IsValid(generics, value))
                    {
                        return false;
                    }
                }
                return true;
            }

            public override TypeObject Unwind(TypeObject[] generics)
            {
                return new Intersection(Types.Select(it => it.Unwind(generics)).ToArray());
            }
        }

        public record Object(Dictionary<string, TypeObject> Entries) : TypeObject
        {
            public override bool IsValid(TypeObject[] generics, object? value)
            {
                return value switch
                {
                    VSharpObject obj => ValidateObj(generics, obj),
                    object o => ValidateUnkown(generics, o),
                    null => false
                };
            }

            public override TypeObject Unwind(TypeObject[] generics)
            {
                return new Object(Entries.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Unwind(generics)
                ));
            }

            bool ValidateObj(TypeObject[] generics, VSharpObject obj)
            {
                foreach(var (name, tp) in Entries)
                {
                    if (!obj.Has(name))
                    {
                        return false;
                    }
                    var result = obj.Get(name);
                    if (!tp.IsValid(generics, result))
                    {
                        return false;
                    }
                }
                return true;
            }

            bool ValidateUnkown(TypeObject[] generics, object obj)
            {
                Type type = obj.GetType();
                foreach(var (name, tp) in Entries)
                {
                    PropertyInfo? property = type.GetProperty(name);
                    if (property == null)
                    {
                        return false;
                    }
                    object? value = property.GetValue(obj);
                    if (!tp.IsValid(generics, value))
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        public record Generic(int Idx) : TypeObject
        {
            public override bool IsValid(TypeObject[] generics, object? value)
            {
                return generics[Idx].IsValid(generics, value);
            }

            public override TypeObject Unwind(TypeObject[] generics)
            {
                return generics[Idx];
            }
        }
    }


    [Serializable]
    public class VSharpObject : ISerializable, IVariables
    {

        public VSharpObject() 
        {
            Entries = []; 
        }

        public object? Get(object key)
        {
            return Entries[key];
        }

        public bool Has(object key)
        {
            return Entries.ContainsKey(key);
        }

        public void Set(object key, object? value)
        {
            Entries[key] = value;
        }

        public required Dictionary<object, object?> Entries {get; set;}

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            foreach (var (name, value) in Entries)
            {
                info.AddValue(
                    name as string ?? throw new Exception("Serializable objects must only have strings as keys"), 
                    value
                );
            }
        }

        public bool HasVar(string name)
        {
            return Entries.ContainsKey(name);
        }

        public void SetVar(string name, object? value)
        {
            Entries[name] = value;
        }

        public object? GetVar(string name)
        {
            return Entries[name];
        }
    }

    public abstract class VSharpInterrupt : Exception {}

    public class ReturnInterrupt(object? value) : VSharpInterrupt 
    {
        public object? Value { get; } = value;
    }

    public class BreakInterrupt(object? value) : VSharpInterrupt 
    {
        public object? Value { get; } = value;
    }

    public class ContinueInterrupt : VSharpInterrupt
    {
    }


    public interface IVariables
    {
        public bool HasVar(string name);
        public void SetVar(string name, object? value);
        public object? GetVar(string name);

        public IVariables Child()
        {
            return new Variables(this);
        }
    }

    public class Variables : IVariables
    {
        private IVariables? _parent;

        public Dictionary<string, object?> _variables;

        public Variables() 
        {
            _parent = null; 
            _variables = [];
        }

        public Variables(IVariables parent) 
        {
            _parent = parent; 
            _variables = [];
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

    };

    public class Function : Invokable {
        public required List<(string, VType?)> Args { get; set;}
        public required Expression Body {get; set;}

        public required IVariables CurriedScope { get; set;}

        public object? Invoke(List<object?> args, Interpreter interpreter)
        {
            if (args.Count != Args.Count) 
            {
                throw new Exception($"Invalid arg count expected {Args.Count} got {args.Count}");
            }
            IVariables child = CurriedScope.Child();

            foreach (var ((name, _), value) in Args.Zip(args)) 
            { 
                child.SetVar(name ?? "", value);
            }
            try 
            {
                return interpreter.EvaluateExpression(Body, child);
            } catch(ReturnInterrupt ri) {
                return ri.Value;
            }
        }
    }

    public class NativeFunc(Func<List<object?>, object?> closure) : Invokable {
        readonly Func<List<object?>, object?> _closure = closure;

        public static NativeFunc FromClosure(Func<List<object?>, object?> closure)
        {
            return new NativeFunc(closure);
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
            IVariables variables = StdLibFactory.StdLib(this);
            foreach (var statement in program.Statements)
            {
                ExecuteStatement(statement, variables);
            }
        }

        object? ExecuteStatement(ASTNode node, IVariables variables)
        {
            switch (node)
            {
                case SetStatementNode setStmt:
                    ExecuteSetStatement(setStmt, variables);
                    break;
                case WhileStatementNode whileStmt:
                    ExecuteWhileStatement(whileStmt, variables);
                    break;
                case FuncStatementNode funcStmt:
                    ExecuteFuncStatement(funcStmt, variables);
                    break;
                case ExprStatement exprStatement:
                    return EvaluateExpression(exprStatement.Expression, variables);
                case PropertyAssignment pas:
                    ExecutePropertyAssignment(pas, variables);
                    break;
                case IndexAssignment indexAssignment:
                    ExecuteIndexAssignment(indexAssignment, variables);
                    break;
                case ForLoop loop:
                    ExecuteForLoop(loop, variables);
                    break;
                case Return ret:
                    ExecuteReturntStatement(ret, variables);
                    break;
                case Break brk:
                    ExecuteBreakStatement(brk, variables);
                    break;
                case Continue:
                    ExecuteContinueStatement();
                    break;
                case TypeStatement ts:
                    ExecuteTypeStatement(ts, variables);
                    break;
                case ImportStatement importStmt:
                    ExecuteImportStatement(importStmt, variables);
                    break;
                case LibStatement libStmt:
                    ExecuteLibStatement(libStmt, variables);
                    break;
                default:
                    throw new Exception("Unhandled statement" + node);
            }
            return null;
        }

        void ExecuteLibStatement(LibStatement stmt, IVariables vars)
        {
            string importPath = EvaluateExpression(stmt.path, vars) as string ?? throw new Exception("Expected string path");

            // Load the assembly from the specified path
            Assembly assembly = Assembly.LoadFrom(importPath);

            VSharpObject libraryExports = new VSharpObject { Entries = [] };

            foreach (Type type in assembly.GetExportedTypes())
            {
          
                if (type.IsAbstract && type.IsSealed)
                {
                    foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Static))
                    {
                        libraryExports.Entries[method.Name] = NativeFunc.FromClosure(args =>
                        {
                            var parameters = method.GetParameters();
                            if (args.Count != parameters.Length)
                            {
                                throw new Exception($"Method {method.Name} expects {parameters.Length} parameters but got {args.Count}");
                            }

                     
                            object?[] convertedArgs = new object[parameters.Length];
                            for (int i = 0; i < parameters.Length; i++)
                            {
                                if (args[i] == null && !parameters[i].ParameterType.IsClass)
                                {
                                    throw new Exception($"Cannot pass null to parameter of type {parameters[i].ParameterType}");
                                }

                                try
                                {
                                    convertedArgs[i] = args[i] == null ? null :
                                        Convert.ChangeType(args[i], parameters[i].ParameterType);
                                }
                                catch (Exception)
                                {
                                    throw new Exception($"Cannot convert argument {args[i]} to type {parameters[i].ParameterType}");
                                }
                            }

                            return method.Invoke(null, convertedArgs);
                        });
                    }
                }
                else
                {
                    var constructor = type.GetConstructor(Type.EmptyTypes);
                    if (constructor != null)
                    {
                        var instance = constructor.Invoke(null);
                        var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance);

                        VSharpObject methodsObj = new VSharpObject { Entries = [] };
                        foreach (var method in methods)
                        {
                            methodsObj.Entries[method.Name] = NativeFunc.FromClosure(args =>
                            {
                                var parameters = method.GetParameters();
                                if (args.Count != parameters.Length)
                                {
                                    throw new Exception($"Method {method.Name} expects {parameters.Length} parameters but got {args.Count}");
                                }

                             
                                object?[] convertedArgs = new object[parameters.Length];
                                for (int i = 0; i < parameters.Length; i++)
                                {
                                    if (args[i] == null && !parameters[i].ParameterType.IsClass)
                                    {
                                        throw new Exception($"Cannot pass null to parameter of type {parameters[i].ParameterType}");
                                    }

                                    try
                                    {
                                        convertedArgs[i] = args[i] == null ? null :
                                            Convert.ChangeType(args[i], parameters[i].ParameterType);
                                    }
                                    catch (Exception)
                                    {
                                        throw new Exception($"Cannot convert argument {args[i]} to type {parameters[i].ParameterType}");
                                    }
                                }

                                return method.Invoke(instance, convertedArgs);
                            });
                        }
                        libraryExports.Entries[type.Name] = methodsObj;
                    }
                }
            }

            // Set the library exports in the variables with the specified name or in the current scope
            if (stmt.name != null)
            {
                vars.SetVar(stmt.name, libraryExports);
            }
            else
            {
                foreach (var entry in libraryExports.Entries)
                {
                    vars.SetVar(entry.Key.ToString(), entry.Value);
                }
            }
        }

        void ExecuteImportStatement(ImportStatement stmt, IVariables variables)
        {
            string importPath = EvaluateExpression(stmt.Path, variables) as string ?? throw new Exception("Expected string path");

            if (!Path.IsPathRooted(importPath))
            {
                string currentDirectory = Path.GetDirectoryName(Program.Path) ?? throw new Exception("Program path is not valid");
                importPath = Path.Combine(currentDirectory, importPath);
            }

            string code = File.ReadAllText(importPath);
            Lexer tokenizer = new(code);
            List<Token> tokens = tokenizer.Tokenize();
            Parser parser = new(tokens);
            ProgramNode program = parser.Parse();

            // Create new scope with stdlib
            IVariables importScope = StdLibFactory.StdLib(this);

            // If name is provided, create a new object to hold the exports
            if (stmt.Name != null)
            {
                var exportScope = new VSharpObject { Entries = [] };
                variables.SetVar(stmt.Name, exportScope);
                // Make the export scope inherit from stdlib scope
                importScope = new Variables(importScope) { };
            }
            else
            {
                // If no name provided, make the current scope inherit from stdlib scope
                importScope = new Variables(importScope) { };
            }

            foreach (var statement in program.Statements)
            {
                ExecuteStatement(statement, importScope);
            }

            // If no name was provided, copy all definitions to the parent scope
            if (stmt.Name == null)
            {
                if (importScope is Variables vars)
                {
                    foreach (var entry in vars._variables)
                    {
                        variables.SetVar(entry.Key, entry.Value);
                    }
                }
            }
        }


        void ExecuteTypeStatement(TypeStatement ts, IVariables variables)
        {
            Dictionary<string, int> genericPos = ts.Generics
                .Select((value, index) => new { value, index })
                .ToDictionary(item => item.value, item => item.index);

            TypeObject result = TypeObject.FromVType(ts.Type, variables, this, genericPos);
            variables.SetVar(ts.Name, result);
        }

        void ExecuteReturntStatement(Return ret, IVariables variables)
        {
            object? value = null;
            if (ret.Expr != null) value = EvaluateExpression(ret.Expr, variables);

            throw new ReturnInterrupt(value);
        }

        void ExecuteBreakStatement(Break ret, IVariables variables)
        {
            object? value = null;
            if (ret.Expr != null) value = EvaluateExpression(ret.Expr, variables);
            throw new ReturnInterrupt(value);
        }

        void ExecuteContinueStatement()
        {
            throw new ContinueInterrupt();
        }


        void ExecuteIndexAssignment(IndexAssignment indexAssignment, IVariables variables)
        {
            object parent = EvaluateExpression(indexAssignment.Parent, variables) ?? throw new Exception("Cannot set property on null");
            object index = EvaluateExpression(indexAssignment.Index, variables) ?? throw new Exception("Index cannot be null");
            object? value = EvaluateExpression(indexAssignment.Value, variables);

            if (parent is VSharpObject vso) 
            {
                vso.Entries[index] = value;
                return;
            } 
            if (parent is List<object?> list && index is int i)
            {
                list[i] = value;
                return;
            }
            if (index is string name)
            {
                PropertyInfo info = parent.GetType().GetProperty(SnakeToPascal(name)) ?? throw new Exception("Property doesnt exist (on strict object)");
                info.SetValue(parent, value);
                return;
            }

           throw new Exception("Indexing operation failed");
        }

        void ExecuteForLoop(ForLoop loop, IVariables variables)
        {
            object parent = EvaluateExpression(loop.Parent, variables) ?? throw new Exception("Cannot iterate over null");
            if (parent is IEnumerable<object?> iter)
            {
                foreach (var item in iter)
                {
                    IVariables child = variables.Child();
                    child.SetVar(loop.ItemName, item);
                    try 
                    {
                        EvaluateExpression(loop.Body, child);
                    } catch(BreakInterrupt) 
                    {
                        return;
                    } catch(ContinueInterrupt)
                    {}
                }
            } else {
                throw new Exception($"Cannot iterate over {parent}");
            }
        }

        void ExecutePropertyAssignment(PropertyAssignment pas, IVariables variables)
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

        void ExecuteFuncStatement(FuncStatementNode funcStatement, IVariables variables) 
        {
            Function function = new Function { Args = funcStatement.Args, Body = funcStatement.Block, CurriedScope= variables};
            variables.SetVar(funcStatement.Name, function);
        }


        void ExecuteSetStatement(SetStatementNode setStmt, IVariables variables)
        {
            object? value = EvaluateExpression(setStmt.Expression, variables);
            variables.SetVar(setStmt.VariableName, value);
        }

        void ExecuteWhileStatement(WhileStatementNode whileStmt, IVariables variables)
        {
            while (EvaluateExpression(whileStmt.Condition, variables) as bool? ?? false)
            {
                try 
                {
                    EvaluateExpression(whileStmt.TrueBlock, variables.Child());
                } catch(ContinueInterrupt) 
                {
                    
                } catch(BreakInterrupt) 
                {   
                    return;
                }
            }
        }

        object? ExecuteIfStatement(IfNode ifStmt, IVariables variables)
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

    

        public object? EvaluateExpression(Expression node, IVariables variables)
        {
            return node switch
            {
                IdentifierNode identifierNode => variables.GetVar(identifierNode.Name),
                BinaryOperationNode binaryOpNode => EvaluateBinaryOperation(binaryOpNode, variables),
                ConstArray array => LoadConstArray(array, variables),
                ConstBool b => b.Value,
                ConstInt i => i.Value,
                ConstDouble d => d.Value,
                ConstString s => s.Value,
                ConstObject o => LoadConstObject(o, variables),
                MethodCall mc => EvaluateMethodCall(mc, variables),
                Invokation i => ExecuteInvokeOperation(i, variables),
                BlockNode n => EvaluateBlockNode(n, variables),
                IfNode i => ExecuteIfStatement(i, variables),
                PropertyAccess pa => EvaluatePropertyAccess(pa, variables),
                ConstFunction func => new Function { Args = func.Args, Body = func.Body, CurriedScope = variables },
                Indexing indexing => EvaluateIndexing(indexing, variables),
                TypeCheck check => EvaluateTypeCheck(check, variables),
                _ => throw new Exception($"Unsupported AST node type: {node.GetType().Name}"),
            };
        }

        bool EvaluateTypeCheck(TypeCheck check, IVariables variables)
        {
            object? value = EvaluateExpression(check.Item, variables);
            TypeObject type = TypeObject.FromVType(check.Type, variables, this, new(){});
            return type.IsValid(Array.Empty<TypeObject>(), value);
        }

        object? EvaluateIndexing(Indexing indexing, IVariables variables)
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

        object? EvaluatePropertyAccess(PropertyAccess pa, IVariables variables)
        {
            object? parent = EvaluateExpression(pa.Parent, variables);
            if (parent is VSharpObject o) 
            {
                return o.Entries[pa.Name];
            }

            Type type = parent?.GetType() ?? throw new Exception("Cannot access property on null");
            PropertyInfo info = type.GetProperty(SnakeToPascal(pa.Name)) ?? throw new Exception($"Property with given name {pa.Name} doesnt exist on {parent}");

            return info.GetValue(parent);
        }

        static string SnakeToPascal(string snakeCaseString)
        {

            return snakeCaseString;
        }


        object? EvaluateMethodCall(MethodCall call, IVariables variables)
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


        object? EvaluateBlockNode(BlockNode block, IVariables variables) 
        {
            object? result = null;
            foreach(var item in block.Statements)
            {
                result = ExecuteStatement(item, variables);
            }
            return result;
        }

        object? ExecuteInvokeOperation(Invokation invoke, IVariables variables)
        {
            Invokable? parent = EvaluateExpression(invoke.Parent, variables) as Invokable;
            if (parent == null) {
                throw new Exception("Cannot invoke " + parent);
            }
            List<object?> evaluatedArgs = invoke.Args.Select(it => EvaluateExpression(it, variables)).ToList();
            return parent.Invoke(evaluatedArgs, this);
        }

        List<object?> LoadConstArray(ConstArray array, IVariables variables)
        {
            List<object?> list = new List<object?>();
            foreach (Expression expr in array.Expressions) {
                list.Add(EvaluateExpression(expr, variables));
            }
            return list;
        }

        VSharpObject LoadConstObject(ConstObject obj, IVariables variables)
        {
            Dictionary<object, object?> objectEntries = obj.Entries.ToDictionary(
                kvp => (object) kvp.Key, 
                kvp => EvaluateExpression(kvp.Value, variables)
            );
            return new VSharpObject { Entries = objectEntries };
        }

        object EvaluateBinaryOperation(BinaryOperationNode binaryOpNode, IVariables variables)
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
                throw new Exception($"Type mismatch in binary operation.");
            }
        }
    
    }

}

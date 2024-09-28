using System.Reflection;
using System.Text;

namespace VSharp 
{
    static class StdLibFactory 
    {
        public static Variables StdLib(Interpreter interpreter)
        {
            Variables vars = new Variables();

            vars.SetVar("int", NativeFunc.FromClosure((args) =>
            {
                return args[0] switch {
                    int i => i,
                    string s => int.Parse(s),
                    _ => throw new Exception("Cannot cast to int")
                };
            }));


            vars.SetVar("str", NativeFunc.FromClosure((args) =>
            {
                return args[0]?.ToString() ?? "null";
            }));

            var types = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(it => it.Namespace == "VSharpLib" && Attribute.IsDefined(it, typeof(Module)))
                .Select(it => (it.Name, InstantiateModule(it, interpreter)))
                .ToArray();

            foreach (var (name, instance) in types)
            {
                vars.SetVar(ToLowerSnakeCase(name), instance);
            }
            return vars;
        }

        public static object InstantiateModule(Type moduleType, Interpreter interpreter)
        {

            // Try to find a constructor that takes an Interpreter as an argument
            ConstructorInfo? constructor = moduleType.GetConstructor(new[] { typeof(Interpreter) });


            if (constructor != null)
            {
                // If a matching constructor exists, invoke it with the interpreter instance
                return constructor.Invoke(new object[] { interpreter });
            }
            else
            {
                return Activator.CreateInstance(moduleType) ?? throw new Exception("Could not instantiate");
            }
        }

        public static string ToLowerSnakeCase(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            var sb = new StringBuilder();
            foreach (char c in input)
            {
                if (char.IsUpper(c) && sb.Length > 0)
                {
                    sb.Append('_');
                }
                sb.Append(char.ToLower(c));
            }

            return sb.ToString();
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class Module : Attribute
    {
    }
}

namespace VSharpLib 
{
    using System.Collections;
    using System.Diagnostics;
    using System.Net.Http.Headers;
    using System.Net.Http.Json;
    using System.Text.Json;
    using System.Text.Json.Nodes;
    using VSharp;

    [Module]
    class Io 
    {
        public void Println(object? arg)
        {
            Console.WriteLine(arg?.ToString() ?? "null");
        }

        public string? Input(object? message)
        {
            Console.WriteLine(message);
            return Console.ReadLine();
        }


        public string? Input()
        {
            return Console.ReadLine();
        }


        public string ReadFile(string name)
        {
            return File.ReadAllText(name);
        }

    }

    [Module]
    class Object 
    {
        public VSharpObject New()
        {
            return new VSharpObject { Entries = new Dictionary<object, object?>() };
        }
    }

    [Module]
    class Error 
    {
        public void Throw(object? reason)
        {
            throw new Exception(reason?.ToString());
        }
    }

    [Module]
    class Json
    {
        public object? Parse(string content) 
        {
            return ParseElement(JsonDocument.Parse(content).RootElement);
        }

        public string ToString(object? json)
        {
            if (json == null) 
            {
                throw new Exception("Cannot serialize null");
            }
            return JsonSerializer.Serialize(json);
        }

        public static object? ParseElement(JsonElement element)
        {
            // Handle JSON object
            if (element.ValueKind == JsonValueKind.Object)
            {
                var dict = new Dictionary<object, object?>();
                foreach (JsonProperty prop in element.EnumerateObject())
                {
                    dict[prop.Name] = ParseElement(prop.Value);
                }
                return new VSharpObject { Entries = dict };
            }
            
            // Handle JSON array
            if (element.ValueKind == JsonValueKind.Array)
            {
                var list = new List<object?>();
                foreach (JsonElement arrayElement in element.EnumerateArray())
                {
                    list.Add(ParseElement(arrayElement));
                }
                return list;
            }

            // Handle primitive values
            switch (element.ValueKind)
            {
                case JsonValueKind.String:
                    return element.GetString();
                case JsonValueKind.Number:
                    if (element.TryGetInt32(out int intValue))
                        return intValue;
                    else
                        return element.GetDouble(); // For non-integer numbers
                case JsonValueKind.True:
                case JsonValueKind.False:
                    return element.GetBoolean();
                case JsonValueKind.Null:
                    return null;
                default:
                    return element.ToString(); // Fallback for other types
            }
        }
    }

    [Module]
    public class Range
    {
        public RangeObj New(int upper) {
            return new RangeObj(0, upper);
        }

        public RangeObj New(int lower, int upper) {
            return new RangeObj(lower, upper);
        }
    }

    public class RangeObj : IEnumerable<object>, IEnumerator<object> {
        public int Lower { get; }
        public int Upper { get; }

        public object Current => current;

        int current;

        public RangeObj(int lower, int upper) {
            this.Lower = lower;
            this.Upper = upper;
            current = Lower;
        }

        public bool MoveNext()
        {
            if (current < Upper) {
                current++;
                return true;
            }
            return false;
        }

        public void Reset()
        {
            current = Lower;
        }


        public void Dispose()
        {
            current = Lower;
        }

        IEnumerator<object> IEnumerable<object>.GetEnumerator()
        {
            return this;
        }

        public IEnumerator GetEnumerator()
        {
            return this;
        }
    }
}


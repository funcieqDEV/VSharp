using System.Reflection;
using System.Text;

namespace VSharp 
{
    static class StdLibFactory 
    {
        public static IVariables StdLib(Interpreter interpreter)
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

          
            ConstructorInfo? constructor = moduleType.GetConstructor(new[] { typeof(Interpreter) });


            if (constructor != null)
            {
                
                return constructor.Invoke(new object[] { interpreter });
            }
            else
            {
                return Activator.CreateInstance(moduleType) ?? throw new Exception("Could not instantiate");
            }
        }

        public static string ToLowerSnakeCase(string input)
        {
            return input;
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


using System.Reflection;
using System.Text;

namespace VSharp 
{
    static class StdLibFactory 
    {
        public static Variables StdLib()
        {
            Variables vars = new Variables();

            vars.SetVar("println", NativeFunc.FromClosure((args) => {
                foreach(var arg in args) {
                    Console.Write(arg);
                }
                Console.Write("\n");
                return null;
            }));

            vars.SetVar("input", NativeFunc.FromClosure((args) =>
            {
                return Console.ReadLine();
            }));

            vars.SetVar("int", NativeFunc.FromClosure((args) =>
            {
                return args[0] switch {
                    int i => i,
                    string s => int.Parse(s),
                    _ => throw new Exception("Cannot cast to int")
                };
            }));

            var types = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(it => it.Namespace == "VSharpLib")
                .Select(it => (it.Name, Activator.CreateInstance(it)))
                .ToArray();

            foreach (var (name, instance) in types)
            {
                vars.SetVar(ToLowerSnakeCase(name), instance);
            }
            return vars;
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

    
}

namespace VSharpLib 
{
    class Io 
    {
        public void Println(object? arg)
        {
            Console.WriteLine(arg);
        }

        public string? Input(object? message)
        {
            Console.WriteLine(message);
            return Console.ReadLine();
        }

        public string ReadFile(string name)
        {
            return File.ReadAllText(name);
        }

    }


}

using System.Reflection;
using System.Text;

namespace VSharp
{
    static class StdLibFactory
    {
        public static Variables StdLib()
        {
            Variables vars = new Variables();

            vars.SetVar("int", NativeFunc.FromClosure((args) =>
            {
                return args[0] switch
                {
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
                .Select(it => (it.Name, Activator.CreateInstance(it)))
                .ToArray();

            foreach (var (name, instance) in types)
            {
                vars.SetVar(name, instance);
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

    [AttributeUsage(AttributeTargets.Class)]
    public class Module : Attribute
    {
    }
}

namespace VSharpLib
{
    using System.Collections;
    using System.Net.Http.Json;
    using System.Text.Json;
    using VSharp;

    [Module]
    class IO
    {
        public void Println(object? arg)
        {
            Console.WriteLine(arg);
        }

        public void Print(object? arg)
        {
            Console.Write(arg);
        }

       

        public string? Input(object? message)
        {
            Console.Write(message);
            return Console.ReadLine();
        }


        public string? Input()
        {
            return Console.ReadLine();
        }


        
    }

    [Module]
    class Math
    {
        public int? RandInt(int min, int max)
        {
            Random rnd = new Random();
            return rnd.Next(min, max);
        }

        public double GetPI()
        {
            return System.Math.PI;
        }

        public double Abs(double value)
        {
            return System.Math.Abs(value);
        }

        public double Max(double a, double b)
        {
            return System.Math.Max(a, b);
        }

        public double Min(double a, double b)
        {
            return System.Math.Min(a, b);
        }

        public double Pow(double x, double y)
        {
            return System.Math.Pow(x, y);
        }

        public double Sqrt(double value)
        {
            return System.Math.Sqrt(value);
        }

        public double Sin(double angle)
        {
            return System.Math.Sin(angle);
        }

        public double Cos(double angle)
        {
            return System.Math.Cos(angle);
        }

        public double Tan(double angle)
        {
            return System.Math.Tan(angle);
        }

        public double Asin(double value)
        {
            return System.Math.Asin(value);
        }

        public double Acos(double value)
        {
            return System.Math.Acos(value);
        }

        public double Atan(double value)
        {
            return System.Math.Atan(value);
        }

        public double Round(double value)
        {
            return System.Math.Round(value);
        }

        public double Ceiling(double value)
        {
            return System.Math.Ceiling(value);
        }
    }

    [Module]
    class Convert
    {
        public int? ToInt(object? num)
        {
            return System.Convert.ToInt32(num);
        }
        public string? ToString(object? s)
        {
            return System.Convert.ToString(s);
        }
    }

    [Module]
    class File
    {
        public string? ReadFile(object name)
        {
            return System.IO.File.ReadAllText(name.ToString());
        }

        public void WriteFile(object name, object value)
        {
            System.IO.File.WriteAllText(name.ToString(), value.ToString());
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
    class Http
    {
        public HttpResponse Get(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = client.GetAsync(url).Result;
                return new HttpResponse(response);
            }
        }
    }


    class HttpResponse
    {
        HttpResponseMessage response;

        public HttpResponse(HttpResponseMessage response)
        {
            this.response = response;
        }

        public int StatusCode()
        {
            return (int)response.StatusCode;
        }

        public string Content()
        {
            return response.Content.ReadAsStringAsync().Result;
        }

        public object? Json()
        {
            string content = Content();
            if (content.StartsWith("["))
            {
                var result = JsonSerializer.Deserialize<List<object>>(content);
                return JsonWrapper.WrapObject(result);
            }
            else
            {
                var result = JsonSerializer.Deserialize<Dictionary<string, object>>(content);
                return JsonWrapper.WrapObject(result);
            }
        }
    }


    public class JsonWrapper
    {
        public static object? WrapObject(object? obj)
        {
 
            if (obj == null)
            {
                return null;
            }

          
            return obj switch
            {
           
                Dictionary<string, object?> dict => new VSharpObject
                {
                    Entries =
                    dict.ToDictionary(
                        kvp => (object)kvp.Key,
                        kvp => WrapObject(kvp.Value) 
                    )
                },

             
                IList list => list.Cast<object?>().Select(WrapObject).ToList(),

           
                _ => obj
            };
        }
    }

}
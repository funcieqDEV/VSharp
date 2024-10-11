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
    class io 
    {
        public void println(object? arg)
        {
            Console.WriteLine(arg?.ToString() ?? "null");
        }

      

        public string? input(object? message)
        {
            Console.Write(message);
            return Console.ReadLine();
        }


        public string? input()
        {
            return Console.ReadLine();
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

        public float? ToFloat(object? num)
        {
            return System.Convert.ToSingle(num);
        }

        public bool? ToBool(object? value)
        {
            return System.Convert.ToBoolean(value);
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
public class Array
    {
        public int Length(List<object> list)
        {
            return list.Count;
        }

        public bool IsEmpty(List<object> list)
        {
            return list.Count == 0;
        }

        public object GetElementAt(List<object> list, int index)
        {
            if (index < 0 || index >= list.Count)
            {
                throw new ArgumentOutOfRangeException("Index out of bounds.");
            }
            return list[index];
        }

        public void AddElement(List<object> list, object element)
        {
            list.Add(element);
        }

        public void RemoveElementAt(List<object> list, int index)
        {
            if (index < 0 || index >= list.Count)
            {
                throw new ArgumentOutOfRangeException("Index out of bounds.");
            }
            list.RemoveAt(index);
        }

        public void Clear(List<object> list)
        {
            list.Clear();
        }

        public bool Contains(List<object> list, object element)
        {
            return list.Contains(element);
        }

        public int IndexOf(List<object> list, object element)
        {
            return list.IndexOf(element);
        }

     
        public List<object> Sort(List<object> list)
        {
            List<object> sortedList = new List<object>(list); 
            QuickSort(sortedList, 0, sortedList.Count - 1);
            return sortedList; 
        }

        private void QuickSort(List<object> list, int low, int high)
        {
            if (low < high)
            {
                int pivotIndex = Partition(list, low, high);
                QuickSort(list, low, pivotIndex - 1);  
                QuickSort(list, pivotIndex + 1, high); 
            }
        }

        private int Partition(List<object> list, int low, int high)
        {
            object pivot = list[high]; 
            int i = low - 1; 

            for (int j = low; j < high; j++)
            {
          
                if (list[j] is IComparable comparableElement)
                {
                    if (comparableElement.CompareTo(pivot) <= 0)
                    {
                        i++;
                        Swap(list, i, j);
                    }
                }
                else
                {
                    throw new ArgumentException("something went wrong");
                }
            }
            Swap(list, i + 1, high);
            return i + 1;
        }

        private void Swap(List<object> list, int i, int j)
        {
            object temp = list[i];
            list[i] = list[j];
            list[j] = temp;
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
        public object? parse(string content) 
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
      
            if (element.ValueKind == JsonValueKind.Object)
            {
                var dict = new Dictionary<object, object?>();
                foreach (JsonProperty prop in element.EnumerateObject())
                {
                    dict[prop.Name] = ParseElement(prop.Value);
                }
                return new VSharpObject { Entries = dict };
            }
            
       
            if (element.ValueKind == JsonValueKind.Array)
            {
                var list = new List<object?>();
                foreach (JsonElement arrayElement in element.EnumerateArray())
                {
                    list.Add(ParseElement(arrayElement));
                }
                return list;
            }

         
            switch (element.ValueKind)
            {
                case JsonValueKind.String:
                    return element.GetString();
                case JsonValueKind.Number:
                    if (element.TryGetInt32(out int intValue))
                        return intValue;
                    else
                        return element.GetDouble(); 
                case JsonValueKind.True:
                case JsonValueKind.False:
                    return element.GetBoolean();
                case JsonValueKind.Null:
                    return null;
                default:
                    return element.ToString(); 
            }
        }
    }

    [Module]

    class Time
    {
        public DateTime Now()
        {
            return DateTime.Now;
        }

        public DateTime Date()
        {
           return DateTime.Today;
        }
    }


    [Module]

    class Str
    {
        public int Length(string? value)
        {
            return value.Length;
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


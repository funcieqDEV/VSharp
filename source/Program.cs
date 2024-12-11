using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using VSharp;

public class Program
{
    public static string _Path = String.Empty;
    public static void Main(String[] args)
    {
        string input = String.Empty;
       
        string version = "0.3.5";
        Interpreter interpreter = new Interpreter();
        if (args.Length > 0)
        {
            if (args[0] == "--v")
            {
                Console.WriteLine("VSharp - " + version);
            }
            else if (args[0] == "run")
            {
                try
                {
                    string exedir = Environment.CurrentDirectory;
                    Environment.CurrentDirectory = new FileInfo(args[1]).Directory!.FullName;
                    input = File.ReadAllText(args[1]);
                    Program._Path = args[1];
                    Lexer lexer = new Lexer(input);
                    List<Token> tokens = lexer.Tokenize();

                    Parser parser = new Parser(tokens);
                    ProgramNode program = parser.Parse();
                    interpreter.Interpret(program);
                    Environment.CurrentDirectory = exedir;
                }
                catch (Exception e)
                {
                    Console.WriteLine("ERROR: " + e);
                }
            }
            else
            {
                Console.WriteLine("usage: ");
                Console.WriteLine(" --v             display your V# version");
                Console.WriteLine(" run             run the project");
            }
        }
        else
        {
            Console.WriteLine("usage: ");
            Console.WriteLine(" --v             display your V# version");
            Console.WriteLine(" run             run the project");
        }




    }
}


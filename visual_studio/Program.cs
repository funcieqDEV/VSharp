using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.IO;
using VSharp;

public class Program
{
    public static void Main(String[] args)
    {
        string input = String.Empty;
        string version = "0.2.1";
        Interpreter interpreter = new Interpreter();
        if (args.Length > 0)
        {
            if (args[0] == "--version")
            {
                Console.WriteLine("VSharp version: " + version);
            }
            else if (args[0] == "run")
            {
                try
                {
                    input = File.ReadAllText(args[1]);
                    Lexer lexer = new Lexer(input);
                    List<Token> tokens = lexer.Tokenize();

                    lexer = null;
                    Parser parser = new Parser(tokens);
                    ProgramNode program = parser.Parse();
                    parser = null;
                    interpreter.Interpret(program);
                }
                catch (Exception e)
                {
                    Console.WriteLine("ERROR: " + e);
                }
            }
            else
            {
                Console.WriteLine("usage: ");
                Console.WriteLine(" --version       display your V# version");
                Console.WriteLine(" run             run the project");
            }
        }
        else
        {
            Console.WriteLine("usage: ");
            Console.WriteLine(" --version       display your V# version");
            Console.WriteLine(" run             run the project");
        }




    }
}


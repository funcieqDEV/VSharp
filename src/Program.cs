using VSharpLib;
using System;
using IO = System.IO;

namespace VSharp;

public sealed class Program 
{
    public static int Main(string[] args) 
    {
        if (args.Length != 1){
            var ret = ExitWithError("Source file not provided\n");
            PrintUsage();
            return ret;
        }        
        try 
        {
            var source = IO.File.ReadAllText(args[0]);
            var lexer = new Lexer(source);
            var tokens = lexer.Tokenize();
            var parser = new Parser(tokens);
            var ast = parser.Parse();

            var interpreter = new Interpreter();
            
            interpreter.Interpret(ast);
        }
        catch (Exception ex)
        {
            return ExitWithError($"An exception has occured: {ex}");
        }
        return 0;
    }

    static int ExitWithError(string err) 
    {
        Console.Error.WriteLine(err);
        return 1;
    }

    static void PrintUsage() 
    {
        Console.WriteLine("VSharp interpreter.\n" +
        "Usage:\n"+
        "<SOURCE>");
    }
}
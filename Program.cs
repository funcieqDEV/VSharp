using System;
using System.Collections.Generic;
using System.IO;
using VSharp;

public class Program
{
    public static string _Path = string.Empty;
    private const string Version = "0.4.1 LTS";

    public static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            PrintLogo();
            PrintUsage();
            return;
        }

        switch (args[0])
        {
            case "--version":
            case "-v":
                Console.WriteLine($"VSharp version {Version}");
                break;

            case "run":
                if (args.Length < 2)
                {
                    Console.WriteLine("ERROR: No file specified to run.");
                    return;
                }
                RunFile(args[1]);
                break;

            case "--help":
            case "-h":
                PrintLogo();
                PrintUsage();
                break;

            case "info":
                PrintInfo();
                break;

            case "new":
                if (args.Length < 2)
                {
                    Console.WriteLine("ERROR: No project name specified.");
                    return;
                }
                CreateNewProject(args[1]);
                break;
            default:
                Console.WriteLine($"ERROR: Unknown command '{args[0]}'\n");
                PrintUsage();
                break;
        }
    }

    private static void PrintLogo()
    {
        Console.WriteLine("GitHub: https://github.com/funcieqDEV/VSharp");
        Console.WriteLine("YouTube: www.youtube.com/@funcieq_");
        Console.WriteLine();
    }

    private static void PrintUsage()
    {
        Console.WriteLine("Usage:");
        Console.WriteLine("  --version, -v     Display VSharp version");
        Console.WriteLine("  run <file>        Run the specified script file");
        Console.WriteLine("  new <name>        Create a new VSharp project");
        Console.WriteLine("  --help, -h        Show this help message");
        Console.WriteLine("  info              Display information about VSharp");
    }

    private static void PrintInfo()
    {
        Console.WriteLine("VSharp - A simple scripting language interpreter.");
        Console.WriteLine("Version: " + Version);
    }

    private static void CreateNewProject(string projectName)
    {
        string projectPath = Path.Combine(Directory.GetCurrentDirectory(), projectName);
        if (Directory.Exists(projectPath))
        {
            Console.WriteLine($"ERROR: Project '{projectName}' already exists.");
            return;
        }
        Directory.CreateDirectory(projectPath);
        File.WriteAllText(Path.Combine(projectPath, "main.vshrp"), "// Entry point for VSharp project");
        Console.WriteLine($"New VSharp project '{projectName}' created successfully.");
    }

    private static void RunFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Console.WriteLine($"ERROR: File '{filePath}' not found.");
            return;
        }

        try
        {
            string initialDir = Environment.CurrentDirectory;
            Environment.CurrentDirectory = new FileInfo(filePath).Directory!.FullName;

            string input = File.ReadAllText(filePath);
            _Path = filePath;

            Lexer lexer = new Lexer(input);
            List<Token> tokens = lexer.Tokenize();



            Parser parser = new Parser(tokens);
            ProgramNode program = parser.Parse();

            Interpreter interpreter = new Interpreter();
            interpreter.Interpret(program);

            Environment.CurrentDirectory = initialDir;
        }
        catch (Exception e)
        {
            Console.WriteLine($"ERROR: {e.Message}");
        }
    }
}
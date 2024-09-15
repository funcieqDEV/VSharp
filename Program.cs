// See https://aka.ms/new-console-template for more information
using VSharp;

void RunCode(String path) {
    Lexer lexer = new Lexer(File.ReadAllText(path));

    List<Token> tokens = lexer.Tokenize();

    Parser parser = new Parser(tokens);
    ProgramNode program = parser.Parse();
    Console.WriteLine(program);
    new Interpreter().Interpret(program);
}


RunCode("./examples/age_checker.swift");
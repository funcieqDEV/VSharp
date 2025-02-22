using System.Linq;

namespace VSharp {
    public abstract class ASTNode { }

    public class ProgramNode : ASTNode
    {
        public List<ASTNode> Statements { get; }

        public ProgramNode() => Statements = [];
    }

    public class ExprStatement(Expression expr) : ASTNode {
        public readonly Expression Expression = expr;
    }

    public class Return : ASTNode
    {
        public Expression? Expr;

    }

    public class Break : ASTNode
    {
        public Expression? Expr;
    }

    public class Continue : ASTNode
    { }

    public class ArgNode : ASTNode
    {
        public List<string> Names { get; set; }

        public ArgNode()
        {
            Names = [];
        }
    }

    public class LibStatement(Expression path, string name) : ASTNode 
    {
        public readonly Expression path = path;
        public readonly string name = name;
    }

    public class ImportStatement(Expression path, string? name) : ASTNode
    {
        public readonly Expression Path = path;
        public readonly string? Name = name;
    }

    public class TypeStatement : ASTNode 
    {
        public required string[] Generics;
        public required string Name;
        public required VType Type;

    }
    public class FuncStatementNode : ASTNode
    {
        public string Name { get; set; }
        public List<(string, VType?)> Args { get; set; }
        public BlockNode Block { get; set; }
        public Dictionary<string, object> Vars { get; set; }
        public VType? ReturnType;
        public FuncStatementNode()
        {
            Block = new BlockNode();
        }
    }


    public class WhileStatementNode : ASTNode
    {
        public Expression Condition { get; set; }
        public Expression TrueBlock { get; set; }

        public WhileStatementNode()
        {
            TrueBlock = new BlockNode();
        }
    }

    public class ForLoop : ASTNode 
    {
        public required Expression Body;
        public required Expression Parent;
        public required string ItemName;
    }

    public class SetStatementNode : ASTNode
    {
        public string VariableName { get; }
        public Expression Expression { get; }

        public SetStatementNode(string variableName, Expression expression)
        {
            VariableName = variableName;
            Expression = expression;
        }
    }

    public class PropertyAssignment : ASTNode 
    {
        public required Expression Parent;
        public required string Name;
        public required Expression Value;
    }

    public class IndexAssignment : ASTNode 
    {
        public required Expression Parent;
        public required Expression Index;
        public required Expression Value;
    }

    public abstract class Expression {}

    public class ConstString : Expression
    {
        public required string Value { get; set; }
    }

    public class ConstInt : Expression
    {
        public required int Value { get; set; }
    }

    public class ConstBool : Expression 
    {
        public required bool Value { get; set; }
    }

    public class ConstDouble : Expression
    {
        public required double Value { get; set; }
    }

    public class Not : Expression 
    {
        public required Expression Value {get; set;}
    }

    public class HasElementCheck : Expression
    {
        public required Expression Item;
        public required Expression Container;
    }

    public class TypeCheck : Expression
    {
        public required Expression Item;
        public required VType Type;
    }


    public class IdentifierNode : Expression
    {
        public string Name { get; }

        public IdentifierNode(string name)
        {
            Name = name;
        }
    }

    public class ConstArray : Expression 
    {
        public List<Expression> Expressions;

        public ConstArray()
        {
            Expressions = new List<Expression>();
        }

        public ConstArray(List<Expression> expresions)
        {
            Expressions = expresions;
        }
    }

    public class ConstObject : Expression 
    {
        public required Dictionary<object, Expression> Entries {get; set;}
        
    }

    public class ConstFunction : Expression 
    {
        public required List<(string, VType?)> Args;
        public required Expression Body;
        public required string[] Generics;
        public VType? ReturnType;
    }


    public class BinaryOperationNode : Expression
    {
        public Expression Left { get; }
        public string Operator { get; }
        public Expression Right { get; }

        public BinaryOperationNode(Expression left, string operatorSymbol, Expression right)
        {
            Left = left;
            Operator = operatorSymbol;
            Right = right;
        }
    }

    public class PropertyAccess : Expression 
    {
        public required Expression Parent;
        public required string Name;
    }


    public class MethodCall : Expression
    {
        public required Expression Parent {get; set; }
        public required string Name {get; set; }
        public required List<Expression> Args { get; set;}
    }

    public class LogicalNode : Expression
    {
        public Expression Left { get; set; }
        public Token Operator { get; set; }
        public Expression Right { get; set; }

        public LogicalNode(Expression left, Token operatorSymbol, Expression right)
        {
            Left = left;
            Operator = operatorSymbol;
            Right = right;
        }
    }

    
    public class Invokation : Expression
    {
        public required List<Expression> Args { get; set; }
        public  required Expression Parent { get; set; }

    }

    
    public class IfNode : Expression
    {
        public Expression Condition { get; set; }
        public Expression TrueBlock { get; set; } //allow for not only blocks to be if bodies
        public Expression FalseBlock { get; set; }

        public IfNode()
        {
            TrueBlock = new BlockNode();
            FalseBlock = new BlockNode();
        }
    }

    public class Indexing : Expression
    {
        public required Expression Parent;
        public required Expression Index;
    }

    public class BlockNode : Expression
    {
        public List<ASTNode> Statements { get; }

        public BlockNode()
        {
            Statements = new List<ASTNode>();
        }

        public BlockNode(List<ASTNode> statements)
        {
            Statements = statements;
        }
    }

    public abstract record VType {

        public VType Join(VType other)
        {
            if (this == other)
            {
                return this;
            }
            if (this is Union u1 && other is Union u2) 
            {
                return new Union(u1.Types.Concat(u2.Types).ToHashSet());
            } 
            if (this is Union u) 
            {
                return new Union(u.Types.Concat(Enumerable.Repeat(other, 1)).ToHashSet());
            }
            if (other is Union union) 
            {
                return new Union(union.Types.Concat(Enumerable.Repeat(this, 1)).ToHashSet());
            }
            return new Union(new() { this, other });
        }

        public VType Intersect(VType other)
        {
            if (this == other)
            {
                return this;
            }
            if (this is Intersection i1 && other is Intersection i2)
            {
                return new Intersection(i1.Types.Concat(i1.Types).ToHashSet());
            }
            if (this is Intersection i3)
            {
                return new Intersection(i3.Types.Concat(Enumerable.Repeat(other, 1)).ToHashSet());
            }
            if (other is Intersection i4)
            {
                return new Intersection(i4.Types.Concat(Enumerable.Repeat(this, 1)).ToHashSet());
            }

            return new Intersection(new() { this, other });
        }
        public record Union(HashSet<VType> Types) : VType;

        public record Intersection(HashSet<VType> Types): VType;
        public record Normal(string[] Type, VType[] Generics): VType;

        public record Func(VType[] Args, VType ReturnType): VType;

        public record Array(VType ItemType) : VType;
        public record Object(Dictionary<string, VType> Entires): VType;

    }


}

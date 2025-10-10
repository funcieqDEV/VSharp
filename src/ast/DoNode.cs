using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSharp;
namespace vsharp.src.ast
{
    public class DoNode : ASTNode
    {
        public BlockNode Body { get; set; }
        public Expression Condition { get; set; }
        public DoNode(BlockNode body, Expression condition)
        {
            Body = body;
            Condition = condition;
        }
    }
}

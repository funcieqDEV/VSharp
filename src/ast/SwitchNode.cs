using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSharp;

namespace vsharp.src.ast
{
    public class SwitchNode : ASTNode
    {
        public Expression Expr { get; set; }
        public List<(Expression,BlockNode)> Cases { get; set; }
        public BlockNode Default { get; set; }

        public SwitchNode(Expression expr, List<(Expression,BlockNode)> cases, BlockNode @default)
        {
            Expr = expr;
            Cases = cases;
            Default = @default;
        }
    }
}

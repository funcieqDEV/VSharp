using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSharp;

namespace vsharp.src.ast
{
    public class UseStatement: ASTNode
    {
        public Expression name;
        public string NameSpace;
    }

}

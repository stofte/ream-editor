using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Core.CodeAnalysis.Models
{
    public enum MemberKind
    {
        Field = 0,
        Property,
        Method,
        ExtensionMethod
    }
}

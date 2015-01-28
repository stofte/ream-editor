using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Core.CodeAnalysis.Models
{
    /// <summary>
    /// Denotes the type of edit
    /// </summary>
    /// <remarks>
    /// Values derives from Roslyn class names for syntax node types.
    /// </remarks>
    public enum EditContext
    {
        /// <summary>
        /// Accessing an objects members.
        /// </summary>
        MemberAccessExpression,
        /// <summary>
        /// Inside string constant.
        /// </summary>
        StringConstant,
        /// <summary>
        /// Could not infer type
        /// </summary>
        Unknown
    }
}

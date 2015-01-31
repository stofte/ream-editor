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
    public enum EditContext
    {
        /// <summary>
        /// Accessing an objects members.
        /// </summary>
        MemberAccessExpression,
        /// <summary>
        /// Could not infer type
        /// </summary>
        Unknown
    }
}


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

using LinqEditor.Core.CodeAnalysis.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Core.CodeAnalysis.Editor
{
    public interface IValidation
    {
        void Initialize(string source);
        void UpdateSource(string source);
        ValidationResults Validate();
    }
}

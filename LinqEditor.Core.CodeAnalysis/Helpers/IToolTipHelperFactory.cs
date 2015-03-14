using LinqEditor.Core.Models.Analysis;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Core.CodeAnalysis.Helpers
{
    public interface IToolTipHelperFactory
    {
        IToolTipHelper Create(SemanticModel model, ExtensionMethodCollection extensionMethods);
        void Release(IToolTipHelper instance);
    }
}

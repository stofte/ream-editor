using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LinqEditor.Core.CodeAnalysis.Services
{
    public interface IDocumentationService
    {
        XElement GetDocumentation(string memberName);
    }
}

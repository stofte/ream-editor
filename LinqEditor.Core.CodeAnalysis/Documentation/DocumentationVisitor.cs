using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Core.CodeAnalysis.Documentation
{
    public class DocumentationVisitor : NuDoq.Visitor
    {
        public string Description { get; set; }

        string _id;

        public DocumentationVisitor(string id)
        {
            _id = id;
        }

        public override void VisitType(NuDoq.TypeDeclaration type)
        {
            if (type.Id == _id)
            {

            }
        }
    }
}

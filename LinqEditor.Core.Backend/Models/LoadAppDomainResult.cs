using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Core.Backend.Models
{
    [Serializable]
    public class LoadAppDomainResult
    {
        public Exception Error { get; set; }
    }
}

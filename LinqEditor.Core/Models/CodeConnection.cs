using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Core.Models
{
    public class CodeConnection : Connection
    {
        public override string ConnectionString
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public override bool IsValidConnectionString
        {
            get { throw new NotImplementedException(); }
        }

        public override string ToString()
        {
            return "Code";
        }
    }
}

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Backend.Dumper
{
    public class DumperResult
    {
        private readonly List<DataTable> _dumps = new List<DataTable>();

        public IDictionary<string, IDictionary<string, int>> SqlColumns { get; set; }

        public DumperResult()
        {
            SqlColumns = new Dictionary<string, IDictionary<string, int>>();
        }

        public void AddTable(DataTable data)
        {
            _dumps.Add(data);
        }

        public IEnumerable<DataTable> Get()
        {
            return _dumps.AsEnumerable();
        }
    }
}

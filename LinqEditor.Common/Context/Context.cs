using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Common.Context
{
    public class Context : IContext
    {
        private string _path;
        private string _schema;

        //public string AssemblyPath {
        //    get
        //    {
        //        return _path;
        //    } 
        //    set 
        //    {
        //        _path = value;
        //        ContextUpdated(_path, _schema);
        //    } 
        //}
        
        //public string SchemaNamespace 
        //{ 
        //    get
        //    {
        //        return _schema;
        //    }
        //    set
        //    {
        //        _schema = value;
        //        ContextUpdated(_path, _schema);
        //    }
        //}

        public event Action<string, string> ContextUpdated;


        public void UpdateContext(string assemblyPath, string schemaNamespace)
        {
            _path = assemblyPath;
            _schema = schemaNamespace;
            ContextUpdated(_path, _schema);
        }
    }
}

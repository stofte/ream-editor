using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Core.Settings
{
    public interface IConnectionStore
    {
        void Add(Connection conn);
        void Update(Connection conn);
        void Delete(Connection conn);
        IEnumerable<Connection> Connections { get; }
    }

    public class ConnectionStore : ApplicationSettings, IConnectionStore
    {
        public static ConnectionStore Instance
        {
            get 
            {
                ConnectionStore connStore = Read<ConnectionStore>();
                if (connStore == null)
                {
                    connStore = new ConnectionStore() { _connections = new List<Connection>() };
                }
                return connStore;
            }
        }

        [JsonProperty]
        private IList<Connection> _connections; // ignore naming for json rendering

        public void Add(Connection conn)
        {
            if (conn.Id == Guid.Empty) throw new ArgumentException("connection must have id");
            if (_connections.Where(x => x.Id == conn.Id).Count() == 0)
            {
                _connections.Add(conn);
                Save();
            }
        }

        public void Update(Connection conn)
        {
            foreach (var c in _connections)
            {
                if (c.Id == conn.Id)
                {
                    c.CachedSchemaFileName = conn.CachedSchemaFileName;
                    c.CachedSchemaNamespace = conn.CachedSchemaNamespace;
                    c.DisplayName = conn.DisplayName;
                    c.ConnectionString = conn.ConnectionString;
                }
            }
        }

        public void Delete(Connection conn)
        {
            _connections = _connections.Where(x => x.Id != conn.Id).ToList();
        }

        [JsonIgnore]
        public IEnumerable<Connection> Connections
        {
            get { return _connections; }
        }
    }
}

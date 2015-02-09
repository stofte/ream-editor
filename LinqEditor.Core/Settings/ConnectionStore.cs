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
        public ConnectionStore() : base()
        {
            var instance = Read<ConnectionStore>();
            if (instance != null)
            {
                _connections = instance._connections;
            }
            else
            {
                _connections = new List<Connection>();
            }
        }

        [JsonProperty]
        private IList<Connection> _connections; // ignore naming for json rendering

        public void Add(Connection conn)
        {
            if (!_connections.Contains(conn, new Connection()))
            {
                _connections.Add(conn);
                Save();
            }
        }

        public void Update(Connection conn)
        {

        }

        public void Update(Connection conn)
        {

        }

        [JsonIgnore]
        public IEnumerable<Connection> Connections
        {
            get { return _connections; }
        }
    }
}

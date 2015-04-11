using LinqEditor.Core.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LinqEditor.Core.Settings
{
    public class ConnectionStore : ApplicationSettings, IConnectionStore
    {
        [JsonIgnore]
        public static Guid CodeId = Guid.Parse("0E46086D-FCEB-4FF9-82B4-F99678E31EE5");

        // use a factory property, so we dont mess up our constructor with file reading logic
        public static ConnectionStore Instance
        {
            get 
            {
                ConnectionStore connStore = null;
                Exception exn = null;
                try
                {
                    connStore = Read<ConnectionStore>();
                }
                catch(Exception e)
                {
                    exn = e;
                }

                if (connStore == null)
                {
                    connStore = new ConnectionStore() { _sqlServerConnections = new List<SqlServerConnection>() };

                    if (exn != null)
                    {
                        connStore._errorLoading = true;
                    }
                }
                return connStore;
            }
        }

        public event Action ConnectionsUpdated;
        
        [JsonProperty]
        private IList<SqlServerConnection> _sqlServerConnections;

        public void Add(Connection conn)
        {
            if (conn.Id == Guid.Empty) throw new ArgumentException("connection must have id");
            if (conn.Id == CodeId) throw new ArgumentException("connection cannot use code id");

            if (conn is SqlServerConnection && _sqlServerConnections.Where(x => x.Id == conn.Id).Count() == 0)
            {
                _sqlServerConnections.Add(conn as SqlServerConnection);
                Save();

                if (ConnectionsUpdated != null)
                {
                    ConnectionsUpdated();
                }
            }
            else
            {
                throw new ArgumentException("Connection id already added");
            }
        }

        public void Update(Connection conn)
        {
            if (conn == null) throw new ArgumentNullException("Connection cannot be null");
            if (conn.Id == CodeId) throw new ArgumentException("Cannot update code connection");

            foreach (var c in _sqlServerConnections)
            {
                if (c.Id == conn.Id)
                {
                    // these are changed by the user. since the event is a hack mainly for the ui, we dont fire
                    // the event if the changes was by code, since we risk firing event handlers in the ui.
                    var uiEvent = c.DisplayName != conn.DisplayName || c.ConnectionString != conn.ConnectionString;

                    c.CachedSchemaFileName = conn.CachedSchemaFileName;
                    c.CachedSchemaNamespace = conn.CachedSchemaNamespace;
                    c.DisplayName = conn.DisplayName;
                    c.ConnectionString = conn.ConnectionString;
                    Save();
                    if (ConnectionsUpdated != null && uiEvent)
                    {
                        ConnectionsUpdated();
                    }
                    break;
                }
            }
        }

        public void Delete(Connection conn)
        {
            if (conn == null) throw new ArgumentNullException("Connection cannot be null");
            if (conn.Id == CodeId) throw new ArgumentException("Cannot delete code connection");

            _sqlServerConnections = _sqlServerConnections.Where(x => x.Id != conn.Id).ToList();
            Save();
            if (ConnectionsUpdated != null)
            {
                ConnectionsUpdated();
            }
        }

        [JsonIgnore]
        public IEnumerable<Connection> Connections
        {
            get {
                return (new Connection[] { new CodeConnection 
                    {
                        Id = CodeId,
                        DisplayName = "Code"
                    }})
                    .Concat(_sqlServerConnections.Where(x => x.Id != CodeId).OrderBy(x => x.DisplayName)
                    .Select(x => new SqlServerConnection
                    {
                        Id = x.Id,
                        DisplayName = x.DisplayName,
                        ConnectionString = x.ConnectionString,
                        CachedSchemaFileName = x.CachedSchemaFileName,
                        CachedSchemaNamespace = x.CachedSchemaNamespace,
                    })); 
            }
        }
    }
}

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Core.Settings
{
    /// <summary>
    /// JSON backed settings class that easily works with rich objects
    /// </summary>
    [Serializable]
    public class ApplicationSettings
    {
        private string _path = PathUtility.ApplicationDirectory + FileName;

        [JsonProperty]
        private IList<Connection> Connections; // ignore naming for json rendering

        public static string FileName = "settings.json";
        
        public ApplicationSettings()
        {
            ApplicationSettings settings = null;
            if (File.Exists(_path))
            {
                var fileData = File.ReadAllText(_path);
                try
                {
                    settings = JsonConvert.DeserializeObject<ApplicationSettings>(fileData);
                }
                catch { }
            }

            if (settings == null)
            {
                Connections = new List<Connection>();
            }
            else
            {
                Connections = settings.Connections;
            }
        }

        private void Save()
        {
            var str = JsonConvert.SerializeObject(this);
            File.WriteAllText(_path, str);
        }

        public void AddConnection(Connection conn)
        {
            if (!Connections.Contains(conn, new Connection()))
            {
                Connections.Add(conn);
                Save();
            }
        }
    }
}

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
    public abstract class ApplicationSettings
    {
        //public static string FileName = "settings.json";
        //protected static string path = (PathUtility.ApplicationDirectory + FileName);
        protected ApplicationSettings _storedInstance;
        
        protected static T Read<T>()
        {
            var path = FileName(typeof(T));
            if (File.Exists(path))
            {
                var fileData = File.ReadAllText(path);
                try
                {
                    return JsonConvert.DeserializeObject<T>(fileData);
                }
                catch { }
            }
            return default(T);
        }
        
        protected void Save()
        {
            var path = FileName(GetType());
            var str = JsonConvert.SerializeObject(this);
            File.WriteAllText(path, str);
        }

        public static string FileName(Type t)
        {
            return PathUtility.ApplicationDirectory + t.Name + ".json";
        }
    }
}

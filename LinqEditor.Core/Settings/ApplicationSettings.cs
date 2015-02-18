using Newtonsoft.Json;
using System;
using System.IO;

namespace LinqEditor.Core.Settings
{
    /// <summary>
    /// JSON backed settings class that easily works with rich objects
    /// </summary>
    [Serializable]
    public abstract class ApplicationSettings
    {
        protected ApplicationSettings _storedInstance;
        protected bool _errorLoading;

        [JsonIgnore]
        public bool LoadingError { get { return _errorLoading; } }
        
        protected static T Read<T>()
        {
            var path = FileName(typeof(T));
            if (File.Exists(path))
            {
                var fileData = File.ReadAllText(path);
                return JsonConvert.DeserializeObject<T>(fileData);
            }
            return default(T);
        }
        
        protected void Save()
        {
            var path = FileName(GetType());
            var str = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(path, str);
        }

        public static string FileName(Type t)
        {
            return PathUtility.ApplicationDirectory + t.Name + ".json";
        }
    }
}

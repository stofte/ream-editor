using LinqEditor.Core.Schema.Models;
using LinqEditor.Utility;
using LinqEditor.Utility.Helpers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Core.Backend.Settings
{
    
    public class UserSettings : ApplicationSettingsBase, IUserSettings
    {
        /// <summary>
        /// Maps from a hash string to an assembly filename
        /// </summary>
        [UserScopedSetting]
        [DefaultSettingValue("")]
        public SerializableStringDictionary CachedSchemas
        {
            get
            {
                return (SerializableStringDictionary)this["CachedSchemas"];
            }
            set
            {
                this["CachedSchemas"] = (SerializableStringDictionary)value;
            }
        }

        /// <summary>
        /// Maps from connection strings to schema hashes
        /// </summary>
        [UserScopedSetting]
        [DefaultSettingValue("")]
        public SerializableStringDictionary SchemaHashes 
        {
            get
            {
                return (SerializableStringDictionary)this["SchemaHashes"];
            }
            set
            {
                this["SchemaHashes"] = (SerializableStringDictionary)value;
            }
        }

        public string GetCachedAssembly(string connectionString)
        {
            Reset();
            if (SchemaHashes.ContainsKey(connectionString))
            {
                return CachedSchemas[SchemaHashes[connectionString]];
            }
            return null;
        }

        public void PersistSchema(string connectionString, DatabaseSchema schema, string assemblyPath)
        {
            var hash = SerializationHelper.Hash(schema);
            CachedSchemas[hash] = assemblyPath;
            SchemaHashes[connectionString] = hash;
            Save();
        }
    }
}

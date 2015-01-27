using LinqEditor.Core.Schema.Models;
using LinqEditor.Utility;
using LinqEditor.Utility.Helpers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqEditor.Core.Backend.Settings
{

    /// <summary>
    /// Persists info about connection strings, schema hashes and previously generated assemblies
    /// </summary>
    public class SchemaStore : ApplicationSettingsBase, ISchemaStore
    {
        public SchemaStore()
        {
            SynchronizeCacheData();
        }

        private void SynchronizeCacheData()
        {
            // delete unknown files and dictionary entries
            var baseDir = Utility.Utility.CachePath();
            var cached = GeneratedSchemaFile;
            var hashes = SchemaHashes;
            var knownHashes = new SerializableStringDictionary();
            var knownFiles = new SerializableStringDictionary();
            
            foreach (var file in Directory.GetFiles(baseDir, "*.dll"))
            {
                foreach (string schemaHash in cached.Keys)
                {
                    if (cached[schemaHash] == file)
                    {
                        foreach(string connStr in hashes.Keys) 
                        {
                            if (hashes[connStr] == schemaHash) 
                            {
                                knownFiles.Add(schemaHash, file);
                                knownHashes.Add(connStr, schemaHash);
                                goto nextFile; // yeeehaw
                            }
                        }
                    }
                }
                
                // else delete file
                var root = Path.GetFileNameWithoutExtension(file);
                if (File.Exists(root + ".cs"))
                {
                    File.Delete(root + ".cs");
                }
                
                if (File.Exists(root + ".pdb")) 
                {
                    File.Delete(root + ".pdb");
                }

                File.Delete(file);

            nextFile: 
                continue;
            }

            GeneratedSchemaFile = knownFiles;
            SchemaHashes = knownHashes;
        }

        /// <summary>
        /// Maps from a hash string to an assembly filename (*.dll)
        /// </summary>
        [UserScopedSetting]
        [DefaultSettingValue("")]
        public SerializableStringDictionary GeneratedSchemaFile
        {
            get
            {
                return (SerializableStringDictionary)this["GeneratedSchemaFile"];
            }
            set
            {
                this["GeneratedSchemaFile"] = (SerializableStringDictionary)value;
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
            if (SchemaHashes.ContainsKey(connectionString))
            {
                return GeneratedSchemaFile[SchemaHashes[connectionString]];
            }
            return null;
        }

        public void PersistSchema(string connectionString, DatabaseSchema schema, string assemblyPath)
        {
            var hash = SerializationHelper.Hash(schema);
            GeneratedSchemaFile[hash] = assemblyPath;
            SchemaHashes[connectionString] = hash;
            Save();
        }
    }
}

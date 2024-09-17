using ServerCloud.Config.Configuration;
using System;
using System.IO;
using System.Xml.Linq;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace ServerCloud.Config
{
    internal class FileConfig
    {
        // -----------------------------------------------------------------------------------------

        [YamlIgnore]
        private string configFile;

        public void setConfigFile(string filename) { configFile = filename; }
        public string getConfigFile() { return configFile; }

        // ------------------------------------------------------------------------------------------

        [YamlIgnore]
        private Type configType = typeof(FileConfig);

        public void setConfigType(Type type) { configType = type; }
        public Type getConfigType() { return configType; }

        // ------------------------------------------------------------------------------------------

        public FileConfig(string configFile)
        {
            this.configFile = configFile;
            createFile();
        }

        public FileConfig() { }

        /// <summary>
        /// Create the config file
        /// </summary>
        /// <param name="configFile"></param>
        /// <param name="config"></param>
        public void createFile()
        {
            char dSep = Path.DirectorySeparatorChar;
            if (!Directory.Exists($"config")) Directory.CreateDirectory($"config");
            if (!File.Exists($"config{dSep}" + configFile))
            {
                File.Create($"config{dSep}" + configFile).Close();
                save();
            }
        }

        /// <summary>
        /// Loads the ConfigFile
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Load<T>() where T : FileConfig
        {
            //CamelCaseNamingConvention.Instance
            IDeserializer deserializer = new DeserializerBuilder().WithNamingConvention(HyphenatedNamingConvention.Instance).Build();
            //string x = File.ReadAllText(this.getConfigFile());

            char dSep = Path.DirectorySeparatorChar;
            string x = File.ReadAllText($"config{dSep}" + getConfigFile());


            T config = deserializer.Deserialize<T>(x);
            config.setConfigFile(configFile);
            return config;
        }

        /// <summary>
        /// saves the config file
        /// </summary>
        /// <param name="config"></param>
        public void save()
        {
            char dSep = Path.DirectorySeparatorChar;

            ISerializer serializer = new SerializerBuilder().WithNamingConvention(HyphenatedNamingConvention.Instance).Build();
            File.WriteAllText($"config{dSep}" + getConfigFile(), serializer.Serialize(this));
        }
    }
}

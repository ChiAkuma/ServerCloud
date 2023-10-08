using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace ServerCloud.Config
{
    internal class YAML
    {
        private static readonly string conf = "config.yml";

        public YAML()
        {
            createFile();
        }
        

        public void createFile()
        {
            if (!File.Exists(conf))
            {
                File.Create(conf).Close();
                ConfigFile config = new ConfigFile();
                save(config);
            }
        }

        public ConfigFile load()
        {
            //CamelCaseNamingConvention.Instance
            IDeserializer deserializer = new DeserializerBuilder().WithNamingConvention(HyphenatedNamingConvention.Instance).Build();
            ConfigFile config = deserializer.Deserialize<ConfigFile>(File.ReadAllText(conf));

            return config;
        }

        public void save(ConfigFile config)
        {
            ISerializer serializer = new SerializerBuilder().WithNamingConvention(HyphenatedNamingConvention.Instance).Build();
            File.WriteAllText(conf, serializer.Serialize(config));
        }

    }
}

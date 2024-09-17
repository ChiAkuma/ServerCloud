using ServerCloud.Config.Configuration;
using ServerCloud.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace ServerCloud.Config
{
    internal class YAML
    {
        public static readonly string defaultConfig = "config.yml";
        public static readonly string workDirConfig = "workdir.yml";

        private static readonly List<FileConfig> files = new List<FileConfig>();

        public YAML()
        {
            registerConfig(new DefaultConfig(defaultConfig));
            registerConfig(new WorkDir(workDirConfig));
        }

        public FileConfig? find(string name)
        {
            foreach (FileConfig file in files)
            {
                if (file.getConfigFile() == name) return file;
            }
            return null;
        }

        public FileConfig find<T>() where T : new()
        {
            T type = new T();
            foreach (FileConfig file in files)
            {
                if (file.GetType() == type.GetType()) return file;
            }
            throw new NoWhereBug("NoWhere TYPE Bug: Config does not exist or other errors! Please contact the developer if you see this message ;)", String.Join("\n", files) + "\n== " + type + " ==");
        }

        public void registerConfig(FileConfig configClass)
        {
            FileConfig configInstance = configClass;
            files.Add(configInstance);
        }
    }
}

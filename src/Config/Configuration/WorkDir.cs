using System.IO;
using ServerCloud.Config;
using YamlDotNet.Serialization;

namespace ServerCloud.Config.Configuration
{
    internal class WorkDir : FileConfig
    {
        [YamlMember(Alias = "working-dir", Description = "---------------------------------------- #\n" +
                          " Changes the directory the Program uses  #\n" +
                          "---------------------------------------- #")]
        public string workingDir = Directory.GetCurrentDirectory();

        [YamlMember(Alias = "first-start")]
        public bool firstStart = true;

        public WorkDir(string configFile) : base(configFile) { }
        public WorkDir() { }
    }
}

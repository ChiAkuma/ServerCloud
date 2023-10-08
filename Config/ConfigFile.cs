using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace ServerCloud.Config
{
    internal class ConfigFile
    {
        [YamlMember(Description = "---------------------------------------- #\n" +
                                  " Changes the directory the Program uses  #\n" +
                                  "---------------------------------------- #")]
        public string workingDir = Directory.GetCurrentDirectory();
        [YamlMember(Alias = "first-start")]
        public bool firstStart = true;

        [YamlMember(Description = "---------------------------------------- #\n" +
                                  "Configure the connection to SQL Database #\n" +
                                  "---------------------------------------- #")]
        public string sqlServer = "";
        public string sqlUser = "";
        public string sqlPass = "";
        public string sqlDb = "";

    }
}

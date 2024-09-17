using ServerCloud.Config;
using YamlDotNet.Serialization;

namespace ServerCloud.Config.Configuration
{
    internal class DefaultConfig : FileConfig
    {
        [YamlMember(Alias = "sql-server", Description = "---------------------------------------- #\n" +
                                  "Configure the connection to SQL Database #\n" +
                                  "---------------------------------------- #")]
        public string sqlServer = "";
        [YamlMember(Alias = "sql-user")]
        public string sqlUser = "";
        [YamlMember(Alias = "sql-pass")]
        public string sqlPass = "";
        [YamlMember(Alias = "sql-db")]
        public string sqlDb = "";

        public DefaultConfig(string configFile) : base(configFile) { }

        public DefaultConfig() { }
    }
}

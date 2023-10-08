using MySqlConnector;
using PPlus;
using ServerCloud.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCloud.Database
{
    internal class DBConnector
    {
        public DBConnector()
        {
            ConfigFile config = Application.yaml.load();
            if (config.sqlServer == "" || config.sqlUser == "" || config.sqlPass == "" || config.sqlDb == "")
            {
                SQLInformation(config);
            }

            bool isClosed = true;
            MySqlConnection connection = new MySqlConnection();
            while (isClosed)
            {
                try
                {
                    connection.ConnectionString = $"Server={config.sqlServer};User ID={config.sqlUser};Password={config.sqlPass};Database={config.sqlDb}";
                    connection.OpenAsync();
                    isClosed = false;
                    PromptPlus.WriteLine("SQL Verbindung Erfolgreich!");
                }
                catch (MySqlException e)
                {
                    PromptPlus.WriteLine($"SQL Fehler: {e.Message}");
                    SQLInformation(config);
                    isClosed = true;
                }
            }
            MySqlCommand cmd_pinfo = new MySqlCommand("CREATE TABLE IF NOT EXISTS PluginInfo (name TEXT, downloadLink TEXT, currentVersion TEXT, updateAvailable BOOL, syncActive BOOL, configSync BOOL, configUsePlaceholders BOOL);", connection);
            cmd_pinfo.ExecuteNonQueryAsync();

            MySqlCommand cmd_psync = new MySqlCommand("CREATE TABLE IF NOT EXISTS PluginSync (name TEXT, server TEXT);", connection);
            cmd_psync.ExecuteNonQueryAsync();
        }

        public ConfigFile SQLInformation(ConfigFile config)
        {
            PromptPlus.WriteLine("Gib deine SQL Daten hier an oder in der Config Datei.");
            config.sqlServer = PromptPlus.Input("SQL Server IP").Default(config.sqlServer).Run().Value;
            config.sqlUser = PromptPlus.Input("SQL Benutzer").Default(config.sqlUser).Run().Value;
            config.sqlPass = PromptPlus.Input("SQL Passwort").Default(config.sqlPass).IsSecret(new Char()).EnabledViewSecret().Run().Value;
            config.sqlDb = PromptPlus.Input("SQL Datenbank").Default(config.sqlDb).Run().Value;
            Application.yaml.save(config);
            return config;
        }
    }
}

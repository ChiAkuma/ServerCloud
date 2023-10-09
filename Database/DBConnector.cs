using MySqlConnector;
using ServerCloud.Config;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
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
                    connection.StateChange += Connection_StateChange;
                    connection.OpenAsync();
                    isClosed = false;
                    
                }
                catch (MySqlException e)
                {
                    AnsiConsole.WriteLine($"SQL Fehler: {e.Message}");
                    SQLInformation(config);
                    isClosed = true;
                }
            }
            MySqlCommand cmd_pinfo = new MySqlCommand("CREATE TABLE IF NOT EXISTS PluginInfo (name TEXT, downloadLink TEXT, currentVersion TEXT, updateAvailable BOOL, syncActive BOOL, configSync BOOL, configUsePlaceholders BOOL);", connection);
            cmd_pinfo.ExecuteNonQueryAsync();

            MySqlCommand cmd_psync = new MySqlCommand("CREATE TABLE IF NOT EXISTS PluginSync (name TEXT, server TEXT);", connection);
            cmd_psync.ExecuteNonQueryAsync();
        }

        private void Connection_StateChange(object sender, StateChangeEventArgs e)
        {
            AnsiConsole.WriteLine($"Sql Verbindung: {e.CurrentState}");
        }

        public ConfigFile SQLInformation(ConfigFile config)
        {
            AnsiConsole.WriteLine("Gib deine SQL Daten hier an oder in der Config Datei.");
            config.sqlServer = AnsiConsole.Prompt(new TextPrompt<string>("SQL Server IP").DefaultValue<string>(config.sqlServer));
            config.sqlUser = AnsiConsole.Prompt(new TextPrompt<string>("SQL Benutzer").DefaultValue<string>(config.sqlUser));
            config.sqlPass = AnsiConsole.Prompt(new TextPrompt<string>("SQL Passwort").DefaultValue<string>(config.sqlPass).Secret());
            config.sqlDb = AnsiConsole.Prompt(new TextPrompt<string>("SQL Datenbank").DefaultValue<string>(config.sqlDb));
            Application.yaml.save(config);
            return config;
        }
    }
}

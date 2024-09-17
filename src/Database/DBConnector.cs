using MySqlConnector;
using ServerCloud.Config;
using ServerCloud.Config.Configuration;
using Spectre.Console;
using System.Data;
using System.Threading;

namespace ServerCloud.Database
{
    internal class DBConnector
    {
        //The connection to the database
        MySqlConnection connection = new MySqlConnection();

        //Basic sql connector and creating of basic tables if not exists
        public DBConnector()
        {
            DefaultConfig config = Application.yaml.find(YAML.defaultConfig).Load<DefaultConfig>();
            if (config.sqlServer == "" || config.sqlUser == "" || config.sqlPass == "" || config.sqlDb == "")
            {
                SQLInformation(config);
            }

            bool isClosed = true;

            while (isClosed)
            {
                try
                {
                    connection.ConnectionString = $"server={config.sqlServer};uid={config.sqlUser};pwd={config.sqlPass};database={config.sqlDb}";

                    connection.StateChange += Connection_StateChange;
                    connection.Open();
                    isClosed = false;
                }
                catch (MySqlException e)
                {
                    AnsiConsole.WriteLine($"SQL Fehler: {e.Message}");
                    SQLInformation(config);
                    isClosed = true;
                }
            }

            //Create Tables for my Datatypes
            //Table for all plugin information /saved in choosen directory
            MySqlCommand cmd_pinfo = new MySqlCommand("CREATE TABLE IF NOT EXISTS PluginInfo (name TEXT, downloadLink TEXT, currentVersion TEXT, updateAvailable BOOL, syncActive BOOL, configSync BOOL, configUsePlaceholders BOOL);", connection);
            cmd_pinfo.ExecuteNonQuery();

            //If the plugin will be synced and to what server
            MySqlCommand cmd_psync = new MySqlCommand("CREATE TABLE IF NOT EXISTS PluginSync (name TEXT, server TEXT);", connection);
            cmd_psync.ExecuteNonQuery();
        }

        private void Connection_StateChange(object sender, StateChangeEventArgs e)
        {
            AnsiConsole.WriteLine($"Sql Status: {e.CurrentState}");
        }

        public DefaultConfig SQLInformation(DefaultConfig config)
        {
            AnsiConsole.WriteLine("Gib deine SQL Daten hier an oder in der Config Datei.");
            config.sqlServer = AnsiConsole.Prompt(new TextPrompt<string>("SQL Server IP").DefaultValue(config.sqlServer));
            config.sqlUser = AnsiConsole.Prompt(new TextPrompt<string>("SQL Benutzer").DefaultValue(config.sqlUser));
            config.sqlPass = AnsiConsole.Prompt(new TextPrompt<string>("SQL Passwort").DefaultValue(config.sqlPass).Secret());
            config.sqlDb = AnsiConsole.Prompt(new TextPrompt<string>("SQL Datenbank").DefaultValue(config.sqlDb));
            config.save();
            return config;
        }

        public void SQLInformationCheck()
        {
            bool confirm = true;
            while (confirm)
            {
                DefaultConfig config = Application.yaml.find(YAML.defaultConfig).Load<DefaultConfig>();
                AnsiConsole.WriteLine($"Hier sind deine SQL Daten: (natürlich ohne Passwort!)\n");
                AnsiConsole.WriteLine($"SQL Server IP:      {config.sqlServer}");
                AnsiConsole.WriteLine($"SQL Benutzer:       {config.sqlUser}");
                AnsiConsole.WriteLine($"SQL Datenbank:      {config.sqlDb}");

                confirm = AnsiConsole.Confirm("Möchtest du diese Daten ändern?");


                if (confirm)
                {
                    AnsiConsole.WriteLine($"");
                    SQLInformation(Application.yaml.find(YAML.defaultConfig).Load<DefaultConfig>());
                    AnsiConsole.WriteLine($"");
                }
            }
        }

        //Ja kp ob ich das brauche.
        //Reopens connection every 2 seconds if closed
        //Can be changed to a execute function that opens the connection before the execution
        public void Reconnect()
        {
            while (Application.isRunning)
            {
                Thread.Sleep(2000);
                //AnsiConsole.WriteLine("Thread ConnectionTest is running!");
                connection.Ping();
                if (connection.State == ConnectionState.Closed)
                {
                    try
                    {
                        connection.Open();
                    }
                    catch (MySqlException) { }
                }

            }
            AnsiConsole.WriteLine("SQL Hintergrund Thread beendet.");
        }
    }
}

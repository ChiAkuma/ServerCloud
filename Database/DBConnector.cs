using MySqlConnector;
using ServerCloud.Config;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServerCloud.Database
{
    internal class DBConnector
    {
        //The connection to the database
        MySqlConnection connection = new MySqlConnection();

        //Basic sql connector and creating of basic tables if not exists
        public DBConnector()
        {
            new Thread(ConnectionTest).Start();
            ConfigFile config = Application.yaml.load();
            if (config.sqlServer == "" || config.sqlUser == "" || config.sqlPass == "" || config.sqlDb == "")
            {
                SQLInformation(config);
            }

            bool isClosed = true;
            
            while (isClosed)
            {
                try
                {
                    this.connection.ConnectionString = $"Server={config.sqlServer};User ID={config.sqlUser};Password={config.sqlPass};Database={config.sqlDb}";
                    this.connection.StateChange += Connection_StateChange;
                    this.connection.Open();
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

        public void SQLInformationCheck()
        {
            ConfigFile config = Application.yaml.load();
            AnsiConsole.WriteLine($"Hier sind deine SQL Daten: (natürlich ohne Passwort!)\n");
            AnsiConsole.WriteLine($"SQL Server IP:      {config.sqlServer}");
            AnsiConsole.WriteLine($"SQL Benutzer:       {config.sqlUser}");
            AnsiConsole.WriteLine($"SQL Datenbank:      {config.sqlDb}");
        }

        //Ja kp ob ich das brauche.
        //Reopens connection every 2 seconds if closed
        //Can be changed to a execute function that opens the connection before the execution
        public void ConnectionTest()
        {
            while (Application.isRunning)
            {
                Thread.Sleep(2000);
                //AnsiConsole.WriteLine("Thread ConnectionTest is running!");
                this.connection.Ping();
                if (this.connection.State == ConnectionState.Closed)
                {
                    try
                    {
                        this.connection.Open();
                    }
                    catch (MySqlException) { }
                }
                
            }
            AnsiConsole.WriteLine("SQL Hintergrund Thread beendet.");
        }
    }
}

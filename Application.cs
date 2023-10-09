using System;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using ServerCloud.Config;
using ServerCloud.Database;
using Spectre.Console;

namespace ServerCloud
{
    internal class Application
    {
        internal static string workingDir = Directory.GetCurrentDirectory();
        internal static Application cloud = new Application();
        internal static YAML yaml = new YAML();

        static void Main(string[] args)
        {
            AnsiConsole.Clear();
            AnsiConsole.Record();
            AnsiConsole.MarkupLine("[underline #00ff00]Hello[/] World!");

            if (args.Length == 0)
            {
                cloud.workingDirectoryQuestion();
            }
            else
            {
                AnsiConsole.MarkupLine($"Initializing instance in Directory: [#ff3333]{args[0]}[/]");
                workingDir = args[0];
            }
            //Here begins the logic

            DBConnector db = new DBConnector();
            /* 
             * Modules:
             * Database: PluginSync
             * Plugin Data
             * Plugin Downloader
             * Plugin checker if updating
             * Docker integration ? docker compose
             * Minecraft Server starter
             * Minecraft Server Console
             */

            //Program is being closed
            var table = new Table().Centered();
            
            AnsiConsole.MarkupLine("[#ff00ff]Taste drücken um zu beenden...[/]");

            Console.ReadKey(true);
            
            AnsiConsole.WriteLine("Programm beendet!");
            FileStream fs = File.Create("jungle.htm");
            StreamWriter sw = new StreamWriter(fs);
            sw.Write(AnsiConsole.ExportHtml());
            sw.Flush();
            sw.Close();
            fs.Close();
            System.Environment.Exit(0);
        }

        public void workingDirectoryQuestion(bool overrideSkip = false)
        {
            ConfigFile config = yaml.load();
            if (!config.firstStart)
                workingDir = config.workingDir;
            AnsiConsole.MarkupLine($"Initializiere Datenordner: [#ff3333]{workingDir}[/]");
            if (config.firstStart || overrideSkip)
            {
                AnsiConsole.MarkupLine($"Um den Pfad zu ändern tippe jetzt den gewünschten Pfad ein:");
                workingDir = AnsiConsole.Prompt(new TextPrompt<string>("Datenordner").DefaultValue<string>(workingDir));
            };
            config.workingDir = workingDir;
            config.firstStart = false;
            yaml.save(config);
        }
    }
}
using JavaDownloaderApp;
using ServerCloud.Config;
using ServerCloud.Config.Configuration;
using ServerCloud.Database;
using Spectre.Console;
using System.IO;
using System.Threading.Tasks;

namespace ServerCloud
{
    internal class Application
    {
        internal static string workingDir = Directory.GetCurrentDirectory();
        internal static Application cloud = new Application();
        internal static YAML yaml = new YAML();
        internal static bool isRunning = true;
        private DBConnector db;

        static void Main(string[] args)
        {
            isRunning = true;
            AnsiConsole.Clear();
            AnsiConsole.Record();
            AnsiConsole.WriteLine("");
            AnsiConsole.Write(
                new FigletText("ServerCloud")
                    .Centered()
                    .Color(Color.Red));
            AnsiConsole.WriteLine("");

            if (args.Length == 0)
            {
                cloud.workingDirectoryQuestion();
            }
            else
            {
                AnsiConsole.MarkupLine($"Initializiere Datenordner: [#ff3333]{args[0]}[/]");
                workingDir = args[0];
            }
            //Here begins the logic

            cloud.db = new DBConnector();
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
            cloud.ModuleSelector();
        }

        public void ModuleSelector()
        {
            string choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("\n[#eecc33]Was möchtest du ausführen?[/]")
                    .PageSize(10)
                    .MoreChoicesText("[grey](Gehe hoch und runter um mehr optionen zu finden)[/]")
                    .AddChoices(new[] {
                        "1. Datenordner anpassen", "2. SQL Daten überprüfen" , "3. Programm Beenden", "4. Experimental",
                    }));

            Rule rule = new Rule();
            rule.Style = Style.Parse("green dim");
            AnsiConsole.Write(rule);
            AnsiConsole.MarkupLine($"[#00ff00]{choice}[/]");
            AnsiConsole.Write(rule);
            switch (int.Parse(choice.Split(".")[0]))
            {
                case 1:
                    cloud.workingDirectoryQuestion(true);
                    cloud.ModuleSelector();
                    break;
                case 2:
                    cloud.db.SQLInformationCheck();
                    cloud.ModuleSelector();
                    break;
                case 3:
                    cloud.shutdownProgram();
                    break;
                case 4:
                    JavaDownloader downloader = new JavaDownloader();
                    Task.Run(() => downloader.DownloadJavaAsync()).GetAwaiter().GetResult();
                    cloud.ModuleSelector();
                    break;
                default:
                    cloud.shutdownProgram();
                    break;
            }
        }

        public void shutdownProgram()
        {
            AnsiConsole.WriteLine("Programm wird beendet!");
            string logfile = "logs" + Path.DirectorySeparatorChar + "log.htm";
            if (!Directory.Exists("logs")) Directory.CreateDirectory("logs");
            if (!File.Exists(logfile)) File.Create(logfile).Close();
            StreamWriter sw = new StreamWriter(logfile, append: true);

            string html = AnsiConsole.ExportHtml().Replace("<pre style=\"font-size:90%;font-family:consolas,'Courier New',monospace\">", "<body style=\"margin:0;padding:0;height:100%;width:100%;background-color:black;\"><pre style=\"background-color:black;color:white;font-size:90%;font-family:consolas,'Courier New',monospace\">").Replace("</pre>", "</pre></body>");

            sw.Write(html);
            sw.Flush();
            sw.Close();

            isRunning = false;
            //System.Environment.Exit(0);
        }

        public void workingDirectoryQuestion(bool overrideSkip = false)
        {
            WorkDir config = yaml.find<WorkDir>().Load<WorkDir>();

            if (!config.firstStart)
                workingDir = config.workingDir;
            AnsiConsole.MarkupLine($"Initializiere Datenordner: [#ff3333]{workingDir}[/]");
            if (config.firstStart || overrideSkip)
            {
                AnsiConsole.MarkupLine($"Um den Pfad zu ändern tippe jetzt den gewünschten Pfad ein:");
                workingDir = AnsiConsole.Prompt(new TextPrompt<string>("Datenordner").DefaultValue<string>(workingDir));
                
                AnsiConsole.MarkupLine($"Initializiere neuen Datenordner: [#ff3333]{workingDir}[/]");
                Directory.CreateDirectory(workingDir);
                //TODO: Daten in neuen Ordner schieben?
            };
            config.workingDir = workingDir;
            config.firstStart = false;

            config.save();
        }
    }
}
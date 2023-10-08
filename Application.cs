using System;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using PPlus;
using PPlus.Controls;
using ServerCloud.Config;
using ServerCloud.Database;

namespace ServerCloud
{
    internal class Application
    {
        internal static string workingDir = Directory.GetCurrentDirectory();
        internal static Application cloud = new Application();
        internal static YAML yaml = new YAML();

        static void Main(string[] args)
        {
            PromptPlus.Clear();
            PromptPlus.Setup((cfg) =>
            {
                cfg.ColorDepth = ColorSystem.TrueColor;
                cfg.IsLegacy = true;
            });
            PromptPlus.BackgroundColor = Color.Black;
            PromptPlus.ForegroundColor = ConsoleColor.Gray;
            PromptPlus.SingleDash($"[yellow]Console Information[/]", DashOptions.DoubleBorder, 1 /*extra lines*/);
            PromptPlus.WriteLine($"IsTerminal: {PromptPlus.IsTerminal}");
            PromptPlus.WriteLine($"IsUnicodeSupported: {PromptPlus.IsUnicodeSupported}");
            PromptPlus.WriteLine($"OutputEncoding: {PromptPlus.OutputEncoding.EncodingName}");
            PromptPlus.WriteLine($"ColorDepth: {PromptPlus.ColorDepth}");
            PromptPlus.WriteLine($"BackgroundColor: {PromptPlus.BackgroundColor}");
            PromptPlus.WriteLine($"ForegroundColor: {PromptPlus.ForegroundColor}");
            PromptPlus.WriteLine($"SupportsAnsi: {PromptPlus.SupportsAnsi}");
            PromptPlus.WriteLine($"Buffers(Width/Height): {PromptPlus.BufferWidth}/{PromptPlus.BufferHeight}");
            PromptPlus.WriteLine($"PadScreen(Left/Right): {PromptPlus.PadLeft}/{PromptPlus.PadRight}\n");
            PromptPlus
                .Banner("ServerCloud")
                .Run(Color.Yellow, BannerDashOptions.DoubleBorderUpDown);
            if (args.Length == 0)
            {
                cloud.workingDirectoryQuestion();
            }
            else
            {
                PromptPlus.WriteLine($"[#ff3333 ON BLACK]Initializing instance in Directory: [/][#ff3333 ON BLACK]{args[0]}[/]");
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
             * Minecraft Server PromptPlus
             */

            //Program is being closed
            PromptPlus.WriteLine("Taste drücken um zu beenden...");
            PromptPlus.ReadKey(true);
            PromptPlus.WriteLine("Programm beendet!");
            System.Environment.Exit(0);
        }

        public void workingDirectoryQuestion(bool overrideSkip = false)
        {
            ConfigFile config = yaml.load();
            if (!config.firstStart)
                workingDir = config.workingDir;
            PromptPlus.WriteLine($"[#ffffff ON BLACK]Initializiere Datenordner: [/][#ff3333 ON BLACK]{workingDir}[/]");
            if (config.firstStart || overrideSkip)
            {
                PromptPlus.WriteLine($"[#ffffff ON BLACK]Um den Pfad zu ändern tippe jetzt den gewünschten Pfad ein:[/]");
                workingDir = PromptPlus.Input("[#ffffff ON BLACK]Datenordner[/]").Default(workingDir).Run().Value;
            }
            config.workingDir = workingDir;
            config.firstStart = false;
            yaml.save(config);
        }
    }
}
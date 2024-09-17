using Newtonsoft.Json.Linq;
using ServerCloud;
using ServerCloud.Config.Configuration;
using Spectre.Console;
using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace JavaDownloaderApp
{
    /// <summary>
    /// A class that handles downloading Java versions asynchronously.
    /// </summary>
    public class JavaDownloader
    {
        // List of valid Java versions for 2024, including a cancel option
        private readonly string[] versions = { "8", "11", "17", "21", "22", "Cancel" };

        /// <summary>
        /// Initiates the download process for the selected Java version.
        /// </summary>
        public async Task DownloadJavaAsync()
        {
            // Prompt the user to select a Java version
            string selectedVersion = SelectVersion();
            if (selectedVersion == "Cancel")
            {
                AnsiConsole.MarkupLine("[yellow]Download canceled by the user.[/]");
                return; // Exit if the user selects "Cancel"
            }
            AnsiConsole.MarkupLine($"You selected Java version [bold]{selectedVersion}[/].");

            // Get the download URL for the selected Java version
            string downloadUrl = await GetJavaDownloadUrlAsync(selectedVersion);

            // Define the local path where the downloaded file will be saved
            string directory = Application.yaml.find<WorkDir>().Load<WorkDir>().workingDir;
            char dSep = Path.DirectorySeparatorChar;
            string downloadPath = Path.Combine(Directory.GetCurrentDirectory(), $"{directory}{dSep}java-{selectedVersion}.tar.gz");

            try
            {
                // Create an HttpClient instance for downloading the file
                using HttpClient client = new HttpClient();

                // Start a progress bar using AnsiConsole
                await AnsiConsole.Progress()
                    .Columns(new ProgressColumn[]
                    {
                        new TaskDescriptionColumn(),    // Task description
                        new ProgressBarColumn(),        // Progress bar
                        new PercentageColumn(),         // Percentage
                        new RemainingTimeColumn(),      // Remaining time
                        new SpinnerColumn(),            // Spinner
                    })
                    .StartAsync(async ctx =>
                    {
                        double time = 0;
                        // Add a task to the progress bar
                        var task = ctx.AddTask("[green]Downloading Java...[/]");

                        // Perform the file download asynchronously
                        await DownloadFileAsync(client, downloadUrl, downloadPath, task);
                    }
                );

                // Inform the user that the download is complete
                AnsiConsole.MarkupLine($"[green]Download completed:[/] {downloadPath}");
            }
            catch (Exception ex)
            {
                // Handle any errors that occur during the download
                AnsiConsole.MarkupLine($"[red]Download failed: {ex.Message}[/]");
            }
        }

        /// <summary>
        /// Downloads a file from the specified URL to the given destination path, updating the progress.
        /// </summary>
        /// <param name="client">The HttpClient used for making requests.</param>
        /// <param name="url">The URL of the file to download.</param>
        /// <param name="destinationPath">The local path where the file will be saved.</param>
        /// <param name="task">The progress task used to update the progress bar.</param>
        private async Task DownloadFileAsync(HttpClient client, string url, string destinationPath, ProgressTask task)
        {
            // Send a request to get the file stream
            using var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode(); // Throw if the HTTP response is unsuccessful

            // Get the total number of bytes to download
            var totalBytes = response.Content.Headers.ContentLength ?? -1L;
            task.MaxValue = totalBytes; // Set the max value for the progress bar

            // Create a stream for reading the content
            using var contentStream = await response.Content.ReadAsStreamAsync();
            // Create a file stream for writing the content to the local file
            using var fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

            var totalRead = 0L;
            var buffer = new byte[8192];
            var isMoreToRead = true;

            // Download the content in chunks and update the progress bar
            do
            {
                // Read a chunk of data
                var read = await contentStream.ReadAsync(buffer, 0, buffer.Length);
                if (read == 0)
                {
                    isMoreToRead = false; // No more data to read
                }
                else
                {
                    // Write the chunk to the file
                    await fileStream.WriteAsync(buffer, 0, read);
                    totalRead += read; // Update the total read bytes
                    task.Value = totalRead; // Update the progress bar

                    task.Description($"[yellow]Downloaded " + (int) (totalRead / Math.Pow(1024, 2)) + " of " + (int) (task.MaxValue / Math.Pow(1024, 2)) + " MiB...[/]");
                }
            }
            while (isMoreToRead);
        }

        /// <summary>
        /// Prompts the user to select a Java version.
        /// </summary>
        /// <returns>The selected Java version as a string.</returns>
        private string SelectVersion()
        {
            // Display the version selection prompt with a cancel option
            return AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select the [green]Java version[/] to download:")
                    .AddChoices(versions));
        }

        /// <summary>
        /// Asynchronously fetches the download URL for the specified Java version.
        /// </summary>
        /// <param name="version">The Java version to download.</param>
        /// <returns>The download URL as a string.</returns>
        private async Task<string> GetJavaDownloadUrlAsync(string version)
        {
            // Construct the API endpoint URL
            string apiEndpoint = $"https://api.adoptopenjdk.net/v3/assets/feature_releases/{version}/ga?architecture=x64&heap_size=normal&image_type=jdk&os=windows";

            using HttpClient client = new HttpClient();
            // Send a request to get the JSON response
            string responseBody = await client.GetStringAsync(apiEndpoint);
            // Parse the JSON response
            JArray json = JArray.Parse(responseBody);

            // Extract the download URL from the JSON response
            foreach (var item in json)
            {
                var binaries = item["binaries"];
                foreach (var binary in binaries)
                {
                    var link = binary["package"]?["link"]?.ToString();
                    if (!string.IsNullOrEmpty(link))
                    {
                        return link; // Return the first valid download link found
                    }
                }
            }

            // Throw an exception if no URL is found
            throw new Exception("Download URL not found for the specified version and architecture.");
        }
    }
}
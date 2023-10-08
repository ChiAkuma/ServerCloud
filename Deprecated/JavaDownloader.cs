using ICSharpCode.SharpZipLib.Tar;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.GZip;
using System.Reflection.PortableExecutable;
using System.Diagnostics;
using System.Net.Http;
using System.IO;
using System.Numerics;

namespace MinecraftServerWrapper.Old
{
    [Obsolete]
    internal class JavaDownloader
    {
        public async Task<bool> downloadJava(string openJDKVersion = "17")
        {
            Console.WriteLine($"[JDK-{openJDKVersion}] Java download wird initializiert...");

            // Construct the OpenJDK download URL
            //https://download.java.net/java/GA/jdk21/fd2272bbf8e04c3dbaee13770090416c/35/GPL/openjdk-21_linux-x64_bin.tar.gz
            //https://download.java.net/java/GA/jdk20.0.2/6e380f22cbe7469fa75fb448bd903d8e/9/GPL/openjdk-20.0.2_windows-x64_bin.zip
            //https://download.java.net/openjdk/jdk19/ri/openjdk-19+36_linux-x64_bin.tar.gz
            //https://download.java.net/openjdk/jdk21/ri/openjdk-21+35_linux-x64_bin.tar.gz
            //https://download.java.net/openjdk/jdk11.0.0.1/ri/openjdk-11.0.0.1_linux-x64_bin.tar.gz
            //https://download.java.net/openjdk/jdk8u43/ri/openjdk-8u43-linux-x64.tar.gz
            //https://download.oracle.com/otn/java/jdk/11.0.19+9/56a39267b45342398c37a72026d961ab/jdk-11.0.19_linux-aarch64_bin.tar.gz?AuthParam=1696726817_1a89a74dd0ea52899ffc886a5827861b
            string openJDKDownloadUrl = $"https://download.oracle.com/java/{openJDKVersion}/latest/jdk-{openJDKVersion}_linux-x64_bin.tar.gz";

            // Specify the directory where you want to save the downloaded OpenJDK
            string downloadDirectory = $"./OpenJDKDownloads/{openJDKVersion}"; // Replace with your desired directory

            // Create the HttpClient
            using (HttpClient client = new HttpClient())
            {
                // Send an HTTP GET request to download the OpenJDK distribution
                Console.WriteLine($"[JDK-{openJDKVersion}] Java wird heruntergerladen...");
                HttpResponseMessage response = await client.GetAsync(openJDKDownloadUrl).ConfigureAwait(false);


                if (response.IsSuccessStatusCode)
                {
                    // Ensure the download directory exists
                    Directory.CreateDirectory(downloadDirectory);

                    // Save the downloaded OpenJDK distribution to a file
                    string downloadFilePath = Path.Combine(downloadDirectory, $"jdk-{openJDKVersion}_linux-x64_bin.tar.gz");
                    Console.WriteLine($"[JDK-{openJDKVersion}] {downloadFilePath}");
                    using (Stream stream = await response.Content.ReadAsStreamAsync())
                    using (FileStream fileStream = File.Create(downloadFilePath))
                    {
                        await stream.CopyToAsync(fileStream);
                    }

                    // Extract the downloaded OpenJDK distribution (assuming it's a .zip file)
                    if (downloadFilePath.EndsWith(".tar.gz", StringComparison.OrdinalIgnoreCase))
                    {
                        // Extract the .zip file
                        extractTarGz(openJDKVersion, downloadFilePath, downloadDirectory);
                        //TarFile.ExtractToDirectory(downloadFilePath, downloadDirectory, true);
                        File.Delete(downloadFilePath); // Optionally, delete the .zip file after extraction
                    }
                    else
                    {
                        Console.WriteLine($"[JDK-{openJDKVersion}] Unsupported archive format. Please handle it accordingly.");
                        return false;
                    }

                    Console.WriteLine($"[JDK-{openJDKVersion}] OpenJDK {openJDKVersion} downloaded and extracted successfully.");
                    Console.WriteLine($"[JDK-{openJDKVersion}] Automatic Testing of Java executable...");

                    // read dir content and rename them to something calculatable
                    IEnumerable<string> dirs = Directory.EnumerateDirectories(downloadDirectory);
                    Dictionary<int, string>? unorderedVerList = new Dictionary<int, string>();
                    foreach (string dcache in dirs)
                    {
                        string dir = dcache;
                        dir = dir.Replace("jdk-", "");
                        dir = dir.Replace(".", "");
                        dir = $".{dir}";
                        Console.WriteLine($"[JDK-{openJDKVersion}] {dcache} >> {dir}");

                        if (dcache != dir)
                        {
                            if (Directory.Exists(dir)) Directory.Delete(dir, true);
                            Directory.Move(dcache, dir);
                        }
                    }

                    //initializes new directory read and puts it into a dictionary
                    dirs = Directory.EnumerateDirectories(downloadDirectory);
                    foreach (string dir in dirs)
                    {
                        int number = -1;
                        bool isInt = int.TryParse(Path.GetFileName(dir), out number);
                        if (isInt) unorderedVerList.Add(number, dir);
                    }

                    //sorts the dictionary to the biggest size and uses that as the main java
                    Dictionary<int, string> verList = new Dictionary<int, string>();
                    var cache = unorderedVerList.OrderByDescending(key => key.Key);
                    verList = cache.ToDictionary((keyItem) => keyItem.Key, (valueItem) => valueItem.Value);
                    unorderedVerList = null; //deletes the list and free memory

                    //make java executable
                    ProcessStartInfo chmod = new ProcessStartInfo
                    {
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    };
                    chmod.FileName = @$"/bin/bash";
                    chmod.Arguments = $"-c \"chmod 700 {verList.First().Value}/bin/java\"";
                    Process.Start(chmod).WaitForExit();

                    //execute test jar
                    ProcessStartInfo startInfo = new ProcessStartInfo
                    {
                        FileName = @$"{verList.First().Value}/bin/java",
                        Arguments = @"-jar ./javatest.jar",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    };
                    Process proc = Process.Start(startInfo);
                    while (!proc.StandardOutput.EndOfStream)
                    {
                        string? line = proc.StandardOutput.ReadLine();
                        Console.WriteLine($"[JDK-{openJDKVersion}] Testing result: {line}");
                        if (line == "true") return true;
                    }
                }
                else
                {
                    Console.WriteLine($"[JDK-{openJDKVersion}] Failed to download OpenJDK. Status code: {response.StatusCode}");
                    return false;
                }
                return false;
            }
        }

        [Obsolete]
        public void extractTarGz(string openJDKVersion, string tarGzFile, string extractDir)
        {
            try
            {
                using (var fileStreamIn = new FileStream(tarGzFile, FileMode.Open, FileAccess.Read))
                using (var gzipStream = new GZipInputStream(fileStreamIn))
                {
                    using (var tarArchive = TarArchive.CreateInputTarArchive(gzipStream)) //deprecated?
                    {
                        tarArchive.ExtractContents(extractDir);
                    }
                }

                Console.WriteLine($"[JDK-{openJDKVersion}] Successfully extracted the tar.gz file.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[JDK-{openJDKVersion}] An error occurred: {ex.Message}");
            }
        }
    }
}
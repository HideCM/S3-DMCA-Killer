using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using IniParser;
using IniParser.Model;
using Colorful; 
using System.Drawing;

// Đảm bảo lớp Program có từ khóa 'partial' nếu bạn chia lớp
partial class Program
{
    static void Main(string[] args)
    {
        // Set the title of the console window
        System.Console.Title = "S3 DMCA Killer - Delete Files on Backblaze B2 - HideCM";

        // Display the banner
        PrintBanner();

        // Parse the config file
        var parser = new FileIniDataParser();
        IniData config = parser.ReadFile("config.ini");

        // List available profiles
        List<string> profiles = config.Sections.Where(s => s.SectionName.StartsWith("Profile_")).Select(s => s.SectionName).ToList();

        if (profiles.Count == 0)
        {
            Colorful.Console.ForegroundColor = Color.Cyan;
            Colorful.Console.WriteLine("[ERROR] No profiles found in config.ini.");
            Colorful.Console.ResetColor();
            return;
        }

        // Display the profiles list for user to choose with alternating colors
        Colorful.Console.ForegroundColor = Color.Cyan;
        Colorful.Console.WriteLine("===============================================");
        Colorful.Console.WriteLine("           ** AVAILABLE PROFILES **            ");
        Colorful.Console.WriteLine("===============================================");
        for (int i = 0; i < profiles.Count; i++)
        {
            // Alternate colors between profile items
            if (i % 2 == 0)
            {
                Colorful.Console.ForegroundColor = Color.Green; // Color for odd-numbered profiles (1st, 3rd, etc.)
            }
            else
            {
                Colorful.Console.ForegroundColor = Color.Yellow; // Color for even-numbered profiles (2nd, 4th, etc.)
            }

            Colorful.Console.WriteLine($"{i + 1}. {profiles[i].Replace("Profile_", "")}");
        }
        Colorful.Console.ResetColor();

        // Ask user for selection
        Colorful.Console.WriteLine("\nEnter the numbers of the profiles you want to delete files for (separate by commas or press Enter to select all):");
        string input = System.Console.ReadLine(); // Sử dụng System.Console

        List<string> selectedProfiles = new List<string>();

        if (string.IsNullOrEmpty(input))
        {
            // If no input, select all profiles
            selectedProfiles = profiles;
        }
        else
        {
            // Parse the input and add the selected profiles
            var indices = input.Split(',').Select(i => i.Trim()).Select(i => int.TryParse(i, out var idx) ? idx - 1 : -1).Where(i => i >= 0 && i < profiles.Count).ToList();

            if (indices.Any())
            {
                selectedProfiles = indices.Select(i => profiles[i].Replace("Profile_", "")).ToList();
            }
            else
            {
                Colorful.Console.ForegroundColor = Color.Red;
                Colorful.Console.WriteLine("[ERROR] Invalid selection.");
                Colorful.Console.ResetColor();
                return;
            }
        }

        Colorful.Console.ForegroundColor = Color.Cyan;
        Colorful.Console.WriteLine($"\nSelected Profiles: {string.Join(", ", selectedProfiles)}");
        Colorful.Console.ResetColor();

        // Wait for user to press Enter to execute the delete operation
        Colorful.Console.WriteLine("\nPress Enter to execute the deletion for selected profiles...");
        System.Console.ReadLine(); // Sử dụng System.Console

        // Iterate over each selected profile and execute deletion process
        foreach (var selectedProfile in selectedProfiles)
        {
            string profileSection = $"Profile_{selectedProfile}";
            Colorful.Console.ForegroundColor = Color.Yellow;
            Colorful.Console.WriteLine($"\n===============================================");
            Colorful.Console.WriteLine($"           ** Processing Profile: {selectedProfile} **");
            Colorful.Console.WriteLine("===============================================");
            Colorful.Console.ResetColor();

            // Load profile-specific settings
            string bucketName = config[profileSection]["bucketName"];
            string profile = config[profileSection]["profile"];
            string endpointUrl = config[profileSection]["endpointUrl"];
            string dmcaFilePath = config[profileSection]["dmcaFilePath"];
            string doneFilePath = config[profileSection]["doneFilePath"];

            // Validate required fields
            if (string.IsNullOrEmpty(bucketName) || string.IsNullOrEmpty(profile) || string.IsNullOrEmpty(endpointUrl) ||
                string.IsNullOrEmpty(dmcaFilePath) || string.IsNullOrEmpty(doneFilePath))
            {
                Colorful.Console.ForegroundColor = Color.Red;
                Colorful.Console.WriteLine($"[ERROR] Missing required configuration in profile '{selectedProfile}'.");
                Colorful.Console.ResetColor();
                continue;
            }

            // Check if the dmca.txt file exists
            if (File.Exists(dmcaFilePath))
            {
                // Read all the lines from the dmca.txt file
                List<string> filePaths = File.ReadAllLines(dmcaFilePath).ToList();

                // Create a list to store the lines that have been successfully deleted
                List<string> successfullyDeletedFiles = new List<string>();

                // Iterate through each file path and execute the delete command
                foreach (var path in filePaths)
                {
                    // Create the AWS CLI delete command for each file
                    string awsCommand = $"aws s3 rm s3://{bucketName}{path} --profile {profile} --endpoint-url {endpointUrl}";

                    // Display the current file being processed
                    Colorful.Console.ForegroundColor = Color.Cyan;
                    Colorful.Console.WriteLine($"\nProcessing file: {path}");
                    Colorful.Console.ResetColor();

                    // Execute the AWS CLI command for the file
                    bool isDeleted = ExecuteAwsCommand(awsCommand, path);

                    // If the file was deleted successfully, save it to the successfullyDeletedFiles list
                    if (isDeleted)
                    {
                        successfullyDeletedFiles.Add($"{path} - {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                    }
                }

                // Save the successfully deleted files to the done file
                if (successfullyDeletedFiles.Any())
                {
                    File.AppendAllLines(doneFilePath, successfullyDeletedFiles);
                    Colorful.Console.ForegroundColor = Color.Green;
                    Colorful.Console.WriteLine($"\nSuccessfully saved the deleted files to {doneFilePath}.");
                    Colorful.Console.ResetColor();
                }

                // Remove the successfully deleted lines from the dmca file
                if (successfullyDeletedFiles.Any())
                {
                    List<string> updatedFilePaths = filePaths.Except(successfullyDeletedFiles.Select(x => x.Split(" - ")[0])).ToList();
                    File.WriteAllLines(dmcaFilePath, updatedFilePaths);
                    Colorful.Console.ForegroundColor = Color.Green;
                    Colorful.Console.WriteLine($"\nSuccessfully updated {dmcaFilePath} by removing the deleted files.");
                    Colorful.Console.ResetColor();
                }

                Colorful.Console.ForegroundColor = Color.Green;
                Colorful.Console.WriteLine("\nAll files for this profile have been successfully processed.");
                Colorful.Console.ResetColor();
            }
            else
            {
                // Display an error if the file doesn't exist
                Colorful.Console.ForegroundColor = Color.Red;
                Colorful.Console.WriteLine($"\nThe file {dmcaFilePath} does not exist for profile {selectedProfile}. Please check the file path and try again.");
                Colorful.Console.ResetColor();
            }
        }

        // Wait for the user to press a key before exiting
        Colorful.Console.ForegroundColor = Color.Cyan;
        Colorful.Console.WriteLine("\nPress any key to exit...");
        Colorful.Console.ResetColor();
        System.Console.ReadKey(); // Sử dụng System.Console
    }

    static bool ExecuteAwsCommand(string command, string filePath)
    {
        try
        {
            // Initialize Process to run the CMD command
            ProcessStartInfo processStartInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c {command}",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true // Do not create a new CMD window
            };

            Process process = new Process
            {
                StartInfo = processStartInfo
            };

            process.Start();

            // Read the output of the command
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            // If the output contains the word "delete", we consider the file deleted successfully
            if (output.Contains("delete"))
            {
                // Success (green message)
                Colorful.Console.ForegroundColor = Color.Green;
                Colorful.Console.WriteLine($"[SUCCESS] File deleted: {filePath}");
                return true;
            }
            else
            {
                // Failure (red message)
                Colorful.Console.ForegroundColor = Color.Red;
                Colorful.Console.WriteLine($"[ERROR] Failed to delete file: {filePath}");
                return false;
            }
        }
        catch (Exception ex)
        {
            // Display an error message if there's an issue executing the command
            Colorful.Console.ForegroundColor = Color.Red;
            Colorful.Console.WriteLine($"[ERROR] Error executing command: {ex.Message}");
            Colorful.Console.ResetColor();
            return false;
        }
    }

    static void PrintBanner()
    {
        // Print a banner with a nice color
        Colorful.Console.ForegroundColor = Color.Cyan;
        Colorful.Console.WriteLine("===============================================");
        Colorful.Console.WriteLine("           ** S3 DMCA Killer **                ");
        Colorful.Console.WriteLine("        Delete files on Backblaze B2           ");
        Colorful.Console.WriteLine("===============================================");
        Colorful.Console.ResetColor();
    }
}

using Octokit;
using System.Collections;

namespace GitAnchor.Lib;
public static class Volumes
{
    public static DriveInfo[] GetVolumes()
    {
        try
        {
            return DriveInfo.GetDrives();
        }
        catch (UnauthorizedAccessException e)
        {
            Console.WriteLine("Unauthorized access to volumes. Please run as administrator.");
            Console.WriteLine(e.Message);
            return [];
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return [];
        }
    }

    private static DriveInfo? GetSelectedDrive(Hashtable options)
    {
        int inputAsInt = CommandLineTools.ReadAsInt();

        if (!options.ContainsKey(inputAsInt)) return null;

        foreach (DictionaryEntry entry in options)
        {
            if (entry.Key.Equals(inputAsInt))
            {
                return entry.Value != null ? (DriveInfo)entry.Value : null;
            }
        }

        return null;
    }

    public static DriveInfo? SelectFromList(IEnumerable<DriveInfo> volumes)
    {
        int i = 0;
        Hashtable options = [];

        // prepare table and present options to user
        Console.WriteLine("Select the drive you want to backup to (enter a number): \n");
        foreach (DriveInfo volume in volumes)
        {
            ++i;
            string option = $"{i}: {volume.Name} ({GetSizeInGB(volume)}GB available)";
            Console.WriteLine(option);
            options.Add(i, volume);
        }

        try
        {
            // parse user input and return appropiate drive info
            return GetSelectedDrive(options);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return null;
        }
    }

    public static DirectoryInfo? CreateAnchor(DirectoryInfo backupDir, string anchorName)
    {
        return backupDir.CreateSubdirectory(anchorName);
    }

    public static string? FindMainBackupFile()
    {
        // find a folder entitled `.ghbackups`
        // check the system drive first, then scan for additional drives and check the root of each

        var userFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        bool backupExistsInUserFolder = Directory.Exists($"{userFolder}/.ghbackups");

        if (backupExistsInUserFolder) return $"{userFolder}/.ghbackups";

        DriveInfo[] volumes = GetVolumes();
        foreach (DriveInfo volume in volumes)
        {
            bool backupExistsInRoot = Directory.Exists($"{volume.RootDirectory.FullName}/.ghbackups");
            if (backupExistsInRoot) return $"{volume.RootDirectory.FullName}/.ghbackups";
        }

        return null;
    }

    public static double GetSizeInGB(DriveInfo volume) => volume.TotalSize / 1024 / 1024 / 1024;

    public static HashSet<Repository> PopulateBackupDirectories(HashSet<Repository> repos, DirectoryInfo backupDir)
    {
        HashSet<Repository> newProjects = [];

        foreach (Repository repo in repos)
        {
            bool exists = Directory.Exists($"{backupDir.FullName}/{repo.Name}");
            bool hasContents = exists && Directory.EnumerateFileSystemEntries(
                $"{backupDir.FullName}/{repo.Name}").Any();

            if (exists && hasContents) continue;

            DirectoryInfo? repoDir = backupDir.CreateSubdirectory(repo.Name);
            Console.WriteLine($"Created directory for project {repoDir.FullName}");
            newProjects.Add(repo);
        }

        return newProjects;
    }
}

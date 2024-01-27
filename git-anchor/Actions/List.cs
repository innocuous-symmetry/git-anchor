using System.Diagnostics;
using GitAnchor.Lib;

namespace GitAnchor.Actions;
class List : BaseAction
{
    public static new ActivityStatusCode Run()
    {
        // get main backup directory
        var backupLocation = FileSystemTools.MainBackupFile;
        var backupDir = new DirectoryInfo(backupLocation ?? throw new Exception("No main backup found"));
        string first = "";

        // get all directories in main backup directory
        var directories = backupDir.GetDirectories();

        Console.WriteLine("Found the following backups:\n");

        // print all directories
        foreach (var dir in directories)
        {
            if (first == "") first = dir.Name;
            long size = FileSystemTools.GetBackupSize(dir);
            long sizeInGBs = Volumes.GetSizeInGB(size);
            long sizeInMBs = Volumes.GetSizeInMB(size);
            Console.WriteLine($"{dir.Name} - {(sizeInGBs > 0 ? sizeInGBs : sizeInMBs)}{(sizeInGBs > 0 ? "GB" : "MB")}");
        }

        Console.WriteLine("\nUse git-anchor pull --name=<name> to pull a backup.");
        Console.WriteLine($"Example: git-anchor pull --name={first} -v\n");

        return ActivityStatusCode.Ok;
    }

}

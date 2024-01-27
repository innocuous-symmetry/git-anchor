using System.Runtime.InteropServices;
namespace GitAnchor.Lib;

public static class FileSystemTools {
    public static string DefaultAnchorPath
    {
        get => DateTime.Now.ToString("Mddy");
    }

    public static bool MainBackupExists
    {
        get => MainBackupFile != null;
    }

    public static bool AnchorExists(string anchorName)
    {
        return MainBackupExists && Directory.Exists($"{MainBackupFile}/anchorName");
    }

    public static string? MainBackupFile
    {
        get => Volumes.FindMainBackupFile();
    }

    public static long GetBackupSize(DirectoryInfo dir)
    {
        long size = 0;
        foreach (FileInfo fi in dir.GetFiles("*", SearchOption.AllDirectories))
        {
            size += fi.Length;
        }

        return size;
    }

    public static DirectoryInfo? SetUpMainBackupFile()
    {
        IEnumerable<DriveInfo> volumes = Volumes.GetVolumes();
        DriveInfo? selection = Volumes.SelectFromList(volumes);

        if (selection == null)
        {
            Console.WriteLine("No selection found");
            return null;
        }

        if (MainBackupFile != null) return new DirectoryInfo(MainBackupFile);

        bool isWindows = OperatingSystem.IsWindows();
        bool isMacOS = OperatingSystem.IsMacOS();
        bool isLinux = OperatingSystem.IsLinux();

        if (isWindows)
            return CreateMainWindowsDirectory(selection);
        else if (isLinux || isMacOS)
            return CreateMainUnixDirectory(selection);
        else
            throw new Exception("Unsupported operating system");
    }

    public static DirectoryInfo? SetUpAnchor(DirectoryInfo backupDir, string? initialInput)
    {
        string defaultName = FileSystemTools.DefaultAnchorPath;
        string? backupName = initialInput;

        if (backupName == null)
        {
            Console.WriteLine($"Enter a name for this backup: (default {defaultName})");
            backupName = Console.ReadLine();
            if (backupName == "") backupName = defaultName;
        }

        if (backupName == null || backupName == "") throw new Exception("No backup name provided");

        // create backup file
        return Volumes.CreateAnchor(backupDir, backupName);
    }

    private static DirectoryInfo? CreateMainWindowsDirectory(DriveInfo volume, string? location = ".git-anchor")
    {
        OSPlatform platform;

        if (OperatingSystem.IsWindows())
        {
            platform = OSPlatform.Windows;
        }
        else if (OperatingSystem.IsMacOS())
        {
            platform = OSPlatform.OSX;
        }
        else if (OperatingSystem.IsLinux())
        {
            platform = OSPlatform.Linux;
        }
        else
        {
            throw new Exception("Unsupported operating system");
        }

        bool isSystemDrive;
        bool isExternalDrive;

        if (platform == OSPlatform.Windows)
        {
            isSystemDrive = volume.Name == "C:\\";
            isExternalDrive = volume.DriveType == DriveType.Removable && !isSystemDrive;
        }
        else
        {
            isSystemDrive = volume.Name == "/";
            isExternalDrive = volume.DriveType == DriveType.Removable && !isSystemDrive;
        }

        try
        {
            if (isSystemDrive)
            {
                Console.WriteLine("Using system drive. Directing you to user folder...");

                string? userPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                DirectoryInfo userDir = new(userPath);

                Console.WriteLine($"Checking for folder `.ghbackups` at `{userDir.FullName}`...");
                DirectoryInfo? backupDir = userDir.CreateSubdirectory(".ghbackups");

                return backupDir;
            }
            else
            {
                Console.WriteLine($"Using {volume.Name}. Directing you to root folder...");

                string rootPath = volume.RootDirectory.FullName;

                DirectoryInfo rootDir = new(rootPath);
                Console.WriteLine($"Checking for folder `.ghbackups` at `{rootDir.FullName}`...");

                DirectoryInfo backupDir = Directory.CreateDirectory($"{rootDir.FullName}.ghbackups");
                return backupDir;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return null;
        }
    }

    private static DirectoryInfo? CreateMainUnixDirectory(DriveInfo volume)
    {
        throw new NotImplementedException();
    }
}

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

    private static DirectoryInfo? CreateMainWindowsDirectory(DriveInfo volume, string? location = ".git-anchor")
    {
        bool isSystemDrive = volume.Name == "C:\\";

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

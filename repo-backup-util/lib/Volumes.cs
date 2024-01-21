using System.Collections;

namespace RepoBackupUtil.Lib
{
    public static class Volumes
    {
        public static IEnumerable<DriveInfo> GetVolumes()
        {
            try
            {
                return DriveInfo.GetDrives();
            } 
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return [];
            }
        }

        private static int ReadInput()
        {
            string? input = Console.ReadLine();

            while (input == null)
            {
                Console.WriteLine("Please enter a number: ");
                input = Console.ReadLine();
            }

            try
            {
                return int.Parse(input);
            }
            catch
            {
                Console.WriteLine("Invaid input. Please enter a number: ");
                return ReadInput();
            }
        }

        private static DriveInfo? GetSelection(Hashtable options)
        {
            int inputAsInt = ReadInput();

            if (!options.ContainsKey(inputAsInt))
            {
                return null;
            }

            foreach (DictionaryEntry entry in options)
            {
                if (entry.Key.Equals(inputAsInt))
                {
                    Console.WriteLine($"value: {entry.Value}");
                    Console.WriteLine($"type: {entry.Value?.GetType()}");
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
                return GetSelection(options);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }

        public static double GetSizeInGB(DriveInfo volume) => volume.TotalSize / 1024 / 1024 / 1024;

        public static DirectoryInfo? CreateBackupDirectory(DriveInfo volume)
        {
            bool isSystemDrive = volume.DriveType == DriveType.Fixed;

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

                    DirectoryInfo? rootDir = volume.RootDirectory;
                    Console.WriteLine($"Checking for folder `.ghbackups` at `{rootDir.FullName}`...");
                    DirectoryInfo? backupDir = rootDir.CreateSubdirectory(".ghbackups");

                    return backupDir;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }
    }
}

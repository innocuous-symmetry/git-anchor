using GitAnchor.Lib;
using Octokit;
using System.Diagnostics;

namespace GitAnchor.Actions;
public class Create : BaseAction
{
    public new static async Task<ActivityStatusCode> Run()
    {
        string[] args = Environment.GetCommandLineArgs();

        string? backupName = args.FirstOrDefault(arg => arg.StartsWith("--name"));
        string? token = args.FirstOrDefault(arg => arg.StartsWith("--token"));
        bool verbose = args.Any(arg => arg.StartsWith("--verbose")
            || arg.StartsWith("-v"));

        // find or configure the default backup directory
        DirectoryInfo backupDir = FileSystemTools.SetUpMainBackupFile() ?? throw new Exception("Encountered a problem creating main backup directory");

        // create backup file
        DirectoryInfo? anchorDir = SetUpAnchor(backupDir, backupName);

        if (anchorDir == null)
        {
            Console.WriteLine("Could not create anchor directory.");
            return ActivityStatusCode.Error;
        }

        // authenticate with github
        GitHubClient github = GitHub.CreateClient();
        token = GitHub.Authenticate(github, token);

        // load accessible repositories
        IEnumerable<Repository> repos = await GitHub.GetRepos(github) ?? throw new Exception("No repos found");

        // organize and ensure all repositories are unique
        HashSet<Repository> uniqueRepos = GitHub.GetUniqueRepos(repos);

        if (verbose)
        {
            // derive repository count and clone urls
            var privateRepos = uniqueRepos.Where(r => r.Private);
            var publicRepos = uniqueRepos.Where(r => !r.Private);
            var cloneUrls = uniqueRepos.Select(r => r.CloneUrl);

            Console.WriteLine($"Found {privateRepos.Count()} private repos and {publicRepos.Count()} public repos.");
        }

        // set up, update directories for hosting projects
        HashSet<Repository> newProjects = Volumes.PopulateBackupDirectories(uniqueRepos, anchorDir);

        // identify all work to be done and prepare as tasks
        List<Task> taskPool = [];

        foreach (Repository newProject in newProjects)
        {
            GitHub.CloneProjectOptions options = new()
            {
                BackupDir = anchorDir,
                Repo = newProject,
                Token = token,
                User = github.User.Current().Result,
            };

            taskPool.Add(new Task(() => GitHub.CloneProject(options)));
        }

        Console.WriteLine($"Preparing to clone {taskPool.Count} repositories. Starting...");

        // start a timer to track progress
        Stopwatch stopwatch = new();
        stopwatch.Start();

        // execute all tasks
        Parallel.ForEach(taskPool, task => task.Start());
        Task.WaitAll([.. taskPool]);

        stopwatch.Stop();
        long elapsed = stopwatch.ElapsedMilliseconds < 1000
            ? stopwatch.ElapsedMilliseconds
            : stopwatch.ElapsedMilliseconds / 1000;

        string unit = stopwatch.ElapsedMilliseconds < 1000 ? "ms" : "s";

        Console.WriteLine($"All tasks completed in {elapsed}{unit}. Exiting...");

        return ActivityStatusCode.Ok;
    }

    private static DirectoryInfo? SetUpAnchor(DirectoryInfo backupDir, string? initialInput)
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
}

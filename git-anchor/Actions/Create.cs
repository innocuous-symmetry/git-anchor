using GitAnchor.Lib;
using Octokit;
using System.Diagnostics;

namespace GitAnchor.Actions;
public class Create : BaseAction
{
    public new static async Task<ActivityStatusCode> RunAsync()
    {
        string[] args = Environment.GetCommandLineArgs();

        string? backupName = args.FirstOrDefault(arg => arg.StartsWith("--name") || arg.StartsWith("-n"));
        string? token = args.FirstOrDefault(arg => arg.StartsWith("--token"));
        bool verbose = args.Any(arg => arg.StartsWith("--verbose")
            || arg.StartsWith("-v"));

        // find or configure the default backup directory
        DirectoryInfo backupDir = FileSystemTools.SetUpMainBackupFile() ?? throw new Exception("Encountered a problem creating main backup directory");

        // create backup file
        DirectoryInfo? anchorDir = FileSystemTools.SetUpAnchor(backupDir, backupName);

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
        ProgressReporter reporter = new();
        reporter.Start();

        Parallel.ForEach(taskPool, task => task.Start());
        Task.WaitAll([.. taskPool]);

        reporter.Stop();

        if (verbose)
        {
            var report = reporter.ToString();
            Console.WriteLine(report);
        }

        return ActivityStatusCode.Ok;
    }
}

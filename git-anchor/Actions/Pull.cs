using System.Diagnostics;
using GitAnchor.Lib;
using GitAnchor.Actions;
using Octokit;

namespace GitAnchor.Actions
{
    internal class Pull : BaseAction
    {
        public new static async Task<ActivityStatusCode> RunAsync()
        {
            string[] args = Environment.GetCommandLineArgs();

            // get main backup
            var backupLocation = FileSystemTools.MainBackupFile;
            var mainBackupDir = new DirectoryInfo(backupLocation ?? throw new Exception("No main backup found"));

            // act on provided arguments
            string? token = args.FirstOrDefault(arg => arg.StartsWith("--token"));
            bool verbose = args.Any(arg => arg.StartsWith("--verbose") || arg.StartsWith("-v"));
            string? anchorName = args.FirstOrDefault(arg => arg.StartsWith("--name") || arg.StartsWith("-n"))
                ?? throw new Exception("No anchor name provided");

            // find (or create) the anchor in question
            var anchor = Volumes.GetAnchor(mainBackupDir, anchorName) ?? FileSystemTools.SetUpAnchor(mainBackupDir, anchorName);
            var projectDirectories = anchor?.GetDirectories();

            if (projectDirectories == null)
            {
                Console.WriteLine("No projects found in anchor.");
                return ActivityStatusCode.Error;
            }

            // set up our github connection
            GitHubClient github = GitHub.CreateClient();
            token = GitHub.Authenticate(github, token);

            var repoList = await GitHub.GetRepos(github);

            // set up a task pool for all pull jobs
            List<Task> taskPool = [];

            // iterate through all projects and prepare jobs
            foreach (DirectoryInfo backupDir in projectDirectories)
            {
                var repo = repoList?.Where(r => r.Name == backupDir.Name).FirstOrDefault();
                if (repo == null) continue;

                if (verbose) Console.WriteLine($"Preparing pull task for {repo.Name}");

                Task pullTask = new(() => GitHub.PullExistingRepo(repo, backupDir, verbose));
                taskPool.Add(pullTask);
            }

            // start progress tracker
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
}

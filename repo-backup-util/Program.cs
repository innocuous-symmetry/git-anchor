// See https://aka.ms/new-console-template for more information
using Octokit;
using RepoBackupUtil.Lib;
using System.Diagnostics;

static async Task Main()
{
    // initialize our github client
    ProductHeaderValue productHeaderValue = new("repo-backup-util");
    GitHubClient github = new(productHeaderValue);

    // check for user with received input
    string token = GitHub.GetCredentialsToken(github);

    string username = github.User.Current().Result.Login;
    User user = await GitHub.GetUser(username, github);

    // get all repos for user
    IEnumerable<Repository> repos = await GitHub.GetRepos(github) ?? throw new Exception("No repos found");

    // organize and ensure all repositories are unique
    HashSet<Repository> uniqueRepos = GitHub.GetUniqueRepos(repos);

    // derive repository count and clone urls
    var privateRepos = uniqueRepos.Where(r => r.Private);
    var publicRepos = uniqueRepos.Where(r => !r.Private);
    var cloneUrls = uniqueRepos.Select(r => r.CloneUrl);

    Console.WriteLine($"Found {privateRepos.Count()} private repos and {publicRepos.Count()} public repos.");

    // create backup directory
    DirectoryInfo backupDir = Volumes.SetUpBackupFile() ?? throw new Exception("Error setting up backup directory");

    // set up, update directories for hosting projects
    HashSet<Repository> newProjects = Volumes.PopulateBackupDirectories(uniqueRepos, backupDir);
    IEnumerable<Repository> projectsToUpdate = GitHub.GetProjectsToUpdate(uniqueRepos, backupDir);

    Console.WriteLine(projectsToUpdate.Count() + " projects to update.");
    Console.WriteLine(newProjects.Count + " new projects to clone.");

    // identify all work to be done and prepare as tasks
    List<Task> taskPool = [];

    foreach (Repository project in projectsToUpdate)
    {
        taskPool.Add(new Task(() => GitHub.UpdateExistingRepo(project, backupDir)));
    }

    foreach (Repository newProject in newProjects)
    {
        ICloneProject options = new GitHub.CloneProjectOptions
        {
            BackupDir = backupDir,
            Repo = newProject,
            Token = token,
            User = user
        };

        taskPool.Add(new Task(() => GitHub.CloneProject(options)));
    }

    Console.WriteLine($"Prepared {taskPool.Count} tasks. Starting...");

    // start a timer to track progress
    Stopwatch stopwatch = new();
    stopwatch.Start();

    // execute all tasks
    Parallel.ForEach(taskPool, task => task.Start());
    Task.WaitAll(taskPool.ToArray());

    stopwatch.Stop();
    double elapsed = stopwatch.ElapsedMilliseconds < 1000
         ? stopwatch.ElapsedMilliseconds
        : stopwatch.ElapsedMilliseconds / 1000;

    string unit = stopwatch.ElapsedMilliseconds < 1000 ? "ms" : "s";

    Console.WriteLine($"All tasks completed in {elapsed}{unit}. Exiting...");
}

await Main();
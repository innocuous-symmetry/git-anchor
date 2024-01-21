// See https://aka.ms/new-console-template for more information
using Octokit;
using RepoBackupUtil.Lib;

static async Task Main()
{
    // initialize our github client
    ProductHeaderValue productHeaderValue = new("repo-backup-util");
    GitHubClient github = new(productHeaderValue);

    // check for user with received input
    string username = GitHub.SetCredentialsToken(github);
    User user = await GitHub.GetUser(username, github);

    // get all repos for user
    IEnumerable<Repository> repos = await GitHub.GetRepos(github) ?? throw new Exception("No repos found");

    // organize and ensure all repositories are unique
    HashSet<Repository> uniqueRepos = GitHub.GetUniqueRepos(repos);

    // derive repository count and clone urls
    var privateRepos = uniqueRepos.Where(r => r.Private);
    var publicRepos = uniqueRepos.Where(r => !r.Private);

    Console.WriteLine($"Found {privateRepos.Count()} private repos and {publicRepos.Count()} public repos.");

    var cloneUrls = uniqueRepos.Select(r => r.CloneUrl);
}

static void DriveStuff()
{
    IEnumerable<DriveInfo> volumes = Volumes.GetVolumes();
    DriveInfo? selection = Volumes.SelectFromList(volumes);
    if (selection == null)
    {
        Console.WriteLine("No selection found");
        return;
    }

    var backupDir = Volumes.CreateBackupDirectory(selection);
}

DriveStuff();
//await Main();
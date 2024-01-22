using Octokit;
using System.Diagnostics;

namespace RepoBackupUtil.Lib
{
    public class GitHub
    {
        public static string GetCredentialsToken(GitHubClient github)
        {
            Console.WriteLine("Please enter your Github Personal Access Token: ");
            string? token = Console.ReadLine();

            if (token == null)
            {
                Console.WriteLine("Received invalid input. Try again:");
                return GetCredentialsToken(github);
            }
            else
            {
                AuthenticationType authType = AuthenticationType.Bearer;
                Credentials auth = new(token, authType);
                github.Credentials = auth;
                Console.WriteLine("Successfully authenticated with GitHub.");
                return token;
            }
        }

        public static string SetOAuthCredentials(GitHubClient github)
        {
            Console.WriteLine("Provide OAuth Client Secret: ");
            string? clientSecret = Console.ReadLine();

            if (clientSecret == null)
            {
                Console.WriteLine("Received invalid input. Try again:");
                return SetOAuthCredentials(github);
            }
            else
            {
                Credentials auth = new(clientSecret, AuthenticationType.Oauth);
                github.Credentials = auth;
                Console.WriteLine("Successfully authenticated with GitHub.");
                return github.User.Current().Result.Login;
            }
        }

        public static async Task<User> GetUser(string username, GitHubClient github)
        {
            Console.WriteLine("Checking for user...");
            User user = await github.User.Get(username) ?? throw new Exception("User does not exist");

            Console.WriteLine($"Found user {user.Login}. Loading repos...");
            return user;
        }

        public static async Task<IReadOnlyList<Repository>?> GetRepos(GitHubClient github) => await github.Repository.GetAllForCurrent();

        public static HashSet<Repository> GetUniqueRepos(IEnumerable<Repository> repos)
        {
            HashSet<Repository> uniqueRepos = [];
            int i = 0;

            foreach (var repo in repos)
            {
                bool added = uniqueRepos.Add(repo);
                Console.WriteLine($"{++i} - {(added ? "added" : null)}{repo.Name} - {(repo.Private ? "Private" : "Public")}");
            }

            return uniqueRepos;
        }

        public static IEnumerable<Repository> GetProjectsToUpdate(HashSet<Repository> allRepos, DirectoryInfo backupDir)
        {
            HashSet<Repository> projectsToUpdate = [];
            foreach (DirectoryInfo dir in backupDir.EnumerateDirectories())
            {
                bool isRepo = dir.GetDirectories(".git").Any();
                if (!isRepo) continue;
                Repository? repo = allRepos.FirstOrDefault(r => r.Name == dir.Name);
                if (repo != null) projectsToUpdate.Add(repo);
            }

            return projectsToUpdate;
        }

        public record CloneProjectOptions : ICloneProject
        {
            public User User { get; init; } = default!;
            public Repository Repo { get; init; } = default!;
            public DirectoryInfo BackupDir { get; init; } = default!;
            public string Token { get; init; } = default!;
        }

        public static void CloneProject(ICloneProject options)
        {
            string cloneString = "";
            cloneString += $"clone https://{options.Token}@github.com/";
            cloneString += $"{options.User.Login}/{options.Repo.Name} .";

            Console.WriteLine($"Cloning {options.Repo.Name}...");

            using (Process process = new())
            {
                process.StartInfo = new()
                {
                    FileName = "git",
                    Arguments = cloneString,
                    WorkingDirectory = $"{options.BackupDir}/{options.Repo.Name}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                process.Start();
                Console.WriteLine(process.StandardOutput.ReadToEnd());
                process.WaitForExit();
            };
        }

        public static void UpdateExistingRepo(Repository repo, DirectoryInfo backupDir)
        {
            Console.WriteLine($"Preparing update task for {repo.Name}...");

            using (Process process = new())
            {
                process.StartInfo = new()
                {
                    FileName = "git",
                    Arguments = "pull",
                    WorkingDirectory = $"{backupDir}/{repo.Name}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                process.Start();
                Console.WriteLine(process.StandardOutput.ReadToEnd());
                process.WaitForExit();
            };
        }
    }
}

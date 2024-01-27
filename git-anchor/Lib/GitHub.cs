using Octokit;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace GitAnchor.Lib
{
    public class GitHub
    {
        public static GitHubClient CreateClient()
        {
            ProductHeaderValue productHeaderValue = new("git-anchor");
            return new(productHeaderValue);
        }

        public static string Authenticate(GitHubClient github, string? initialToken = null)
        {
            string token = initialToken ?? CommandLineTools.ReadAccessToken(github);
            AuthenticationType authType = AuthenticationType.Bearer;
            Credentials auth = new(token, authType);
            github.Credentials = auth;
            Console.WriteLine("Successfully authenticated with GitHub.");
            return token;
        }

        /** @deprecated */
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

        public record GithubRepoOptions
        {
            public DirectoryInfo BackupDir { get; init; } = default!;
            public User? User { get; init; } = default!;
            public Repository? Repo { get; init; } = default!;
            public string? ProjectPath { get; init; }
            public string? Token { get; init; }
            public string? AnchorPath { get; init; }
            public bool? Verbose { get; init; } = false;
        }

        public static void CloneProject(GithubRepoOptions options)
        {
            if (options.User == null) throw new Exception("User not found");
            if (options.Repo == null) throw new Exception("Repository not found");

            string cloneString = "";
            cloneString += $"clone https://{options.Token}@github.com/";
            cloneString += $"{options.User.Login}/{options.Repo.Name} .";

            if (options.Verbose ?? false) Console.WriteLine($"Cloning {options.Repo.Name}...");

            using (Process process = new())
            {
                process.StartInfo = new()
                {
                    FileName = "git",
                    Arguments = cloneString,
                    WorkingDirectory = Path.Join(options.BackupDir.FullName, options.Repo.Name),
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                process.Start();
                if (options.Verbose ?? false) Console.WriteLine(process.StandardOutput.ReadToEnd());
                process.WaitForExit();
                Console.WriteLine($"Clone complete for {options.Repo.Name}");
            };
        }

        public static void PullExistingRepo(GithubRepoOptions options)
        {
            if (options.Repo == null) throw new Exception("Repository not found");
            Console.WriteLine($"Pulling {options.Repo.Name}...");

            using (Process process = new())
            {
                process.StartInfo = new()
                {
                    FileName = "git",
                    Arguments = "pull",
                    WorkingDirectory = Path.Join(options.BackupDir.FullName, options.Repo.Name),
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                process.Start();
                if (options.Verbose ?? false) Console.WriteLine(process.StandardOutput.ReadToEnd());
                process.WaitForExit();
                Console.WriteLine($"Pull complete for {options.Repo.Name}");
            };
        }

        public static void QueryStatus(GithubRepoOptions options)
        {
            using (Process process = new())
            {
                process.StartInfo = new()
                {
                    FileName = "git",
                    Arguments = "status",
                    WorkingDirectory = Path.Join(options.BackupDir.FullName, options.Repo?.Name ?? options.ProjectPath ?? throw new Exception("Unable to determine working directory")),
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

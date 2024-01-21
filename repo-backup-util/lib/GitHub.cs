using Octokit;

namespace RepoBackupUtil.Lib
{
    public static class GitHub
    {
        public static string SetCredentialsToken(GitHubClient github)
        {
            Console.WriteLine("Please enter your Github Personal Access Token: ");
            string? token = Console.ReadLine();

            if (token == null)
            {
                Console.WriteLine("Received invalid input. Try again:");
                return SetCredentialsToken(github);
            }
            else
            {
                AuthenticationType authType = AuthenticationType.Bearer;
                Credentials auth = new(token, authType);
                github.Credentials = auth;
                Console.WriteLine("Successfully authenticated with GitHub.");
                return github.User.Current().Result.Login;
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
            User? user = await github.User.Get(username);

            if (user == null)
            {
                Console.WriteLine("User does not exist");
                string newUsername = SetCredentialsToken(github);
                return await GetUser(newUsername, github);
            }
            else
            {
                Console.WriteLine($"Found user {user.Login}. Loading repos...");
                return user;
            }
        }

        public static async Task<IReadOnlyList<Repository>?> GetRepos(GitHubClient github)
        {
            Console.WriteLine("Loading repos...");
            var repos = await github.Repository.GetAllForCurrent();
            return repos;
        }

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
    }
}

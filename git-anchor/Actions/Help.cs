using System.Diagnostics;

namespace GitAnchor.Actions;

public class Help : BaseAction
{
    public static async new Task<ActivityStatusCode> Run()
    {
        Console.WriteLine("\nGitAnchor - a tool for performing scoped backups of GitHub repositories based on credentials and configuration.");
        Console.WriteLine("Usage:\n");

        Console.WriteLine("git-anchor create [--verbose, -v] [--name <name>] [--token <token>]");
        Console.WriteLine("    Creates a new backup directory and clones all repositories accessible by the provided token.");
        Console.WriteLine("    --name <name>    The name of the backup directory to create. Defaults to the current date.");
        Console.WriteLine("    --token <token>  The GitHub token to use for authentication.");
        Console.WriteLine("    --verbose        Prints verbose output.\n");
        Console.WriteLine("Example: git-anchor create -v --name=cool-new-backup --token=(your token)");

        Console.WriteLine("git-anchor pull [--name <name>] [--verbose, -v]");
        Console.WriteLine("    Pulls all repositories in an existing backup directory.");
        Console.WriteLine("    --name <name>    The name of the backup directory to pull from. Defaults to the current date.");
        Console.WriteLine("    --verbose        Prints verbose output.\n");
        Console.WriteLine("Example: git-anchor pull -v --name=cool-new-backup");

        Console.WriteLine("git-anchor list");
        Console.WriteLine("    Lists all existing backups.\n");
        Console.WriteLine("Example: git-anchor list");

        Console.WriteLine("git-anchor find [--regex, -e] [--name, -n <name>]");
        Console.WriteLine("    Finds an existing backup matching the provided pattern.");
        Console.WriteLine("    --regex, -e      Find your backup directory based on a Regex search.");
        Console.WriteLine("    --name, -n       The name of the backup directory to pull from. Defaults to the current date.\n\n");

        return ActivityStatusCode.Ok;
    }

}

using Octokit;

namespace GitAnchor.Lib;

public static class CommandLineTools {
    public static string? ReadAnchorName()
    {
        return Environment.GetCommandLineArgs().FirstOrDefault(arg => arg.StartsWith("--name") || arg.StartsWith("-n"))?
        .Split("=")[1].Trim();
    }

    public static int ReadAsInt()
    {
        string? input = Console.ReadLine();

        while (input == null)
        {
            input = Console.ReadLine();
        }

        try
        {
            return int.Parse(input);
        }
        catch
        {
            Console.WriteLine("Invaid input. Please enter a number: ");
            return ReadAsInt();
        }
    }

    public static string ReadAccessToken(GitHubClient github)
    {
        Console.WriteLine("Please enter your Github Personal Access Token: ");
        string? token = Console.ReadLine();

        if (token == null)
        {
            Console.WriteLine("Received invalid input. Try again:");
            return ReadAccessToken(github);
        }

        return token;
    }
}

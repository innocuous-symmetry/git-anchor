using System.Diagnostics;
using GitAnchor.Lib;

namespace GitAnchor.Actions;

public class Status : BaseAction
{
    public new static async Task<ActivityStatusCode> RunAsync()
    {
        string[] args = Environment.GetCommandLineArgs();

        // get main backup directory
        var backupLocation = FileSystemTools.MainBackupFile;
        var anchorName = CommandLineTools.ReadAnchorName();
        var backupDir = new DirectoryInfo(Path.Join(backupLocation, anchorName));

        // get all directories in main backup directory
        var directories = backupDir.GetDirectories();

        List<Task> taskPool = [];

        foreach (var dir in directories)
        {
            GitHub.GithubRepoOptions options = new()
            {
                AnchorPath = Path.Join(backupDir.FullName, dir.Name),
                BackupDir = backupDir,
                ProjectPath = dir.Name,
            };

            taskPool.Add(Task.Run(() => GitHub.QueryStatus(options)));
        }

        using var reporter = new ProgressReporter();
        reporter.Start();

        await Parallel.ForEachAsync(taskPool, async(task, cancellationToken) => await task);

        reporter.Stop();
        Console.WriteLine(reporter.ToString());

        return ActivityStatusCode.Ok;
    }
}

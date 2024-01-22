using Octokit;

namespace RepoBackupUtil.Lib;
public interface ICloneProject
{
    Repository Repo { get; }
    DirectoryInfo BackupDir { get; }
    User User { get; }
    string Token { get; }
}
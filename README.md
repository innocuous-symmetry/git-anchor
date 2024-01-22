# GitHub Repository Bookup Tool

This is a lightweight command line interface for backing up a GitHub user's repositories. Depending on permissions provided
during authentication, the tool can backup public and private repositories. It will also update existing backups by pulling
changes from the remote repository.

The tool is writting in C# and uses the Octokit.net library to interact with the GitHub API.

## Requirements

The tool requires a GitHub Personal Access Token to be provided as an input to the script. The token can be generated from 
Developer Settings within your GitHub account.

This tool will adapt its behavior based on the permissions of the provided token. For example, if a token is fine-grained and only
allows access to repositories owned by the account holder, repositories owned by your organizations will not be backed up.

## Usage

If dotnet is installed on the host device, the tool can be run directly using the dotnet CLI. Otherwise, the tool is available as
a self-contained executable. The tool can be run from the command line or from a batch file.

## Caveats

For now, the tool will only back up repositories to the system drive of a given device. Direct backup to USB drives and network
drives is not yet supported.

The tool does not download associated packages or artifacts (i.e. node_modules, nuget packages, etc). This is a deliberate choice
to keep the footprint of the backup small.
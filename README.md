# GitHub Repository Bookup Tool

This is a lightweight command line interface for backing up a GitHub user's repositories. Depending on permissions provided
during authentication, the tool can backup public and private repositories.

The tool is writting in C# and uses the Octokit.net library to interact with the GitHub API.

## Usage

The tool is a command line interface. It can be run from the command line or from a batch file. The tool requires a GitHub
Personal Access Token to be provided as a command line argument. The token can be generated from the GitHub website. It is
recommended that the user only provide the minimum permissions required to backup the repositories. GitHub provides a fine-grained
access token generator that makes this very simple.
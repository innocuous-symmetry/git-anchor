using System.Diagnostics;
using System.Runtime.CompilerServices;
using GitAnchor.Actions;
using GitAnchor.Lib;

string[] inputArgs = Environment.GetCommandLineArgs();

try
{
    foreach (string a in inputArgs)
    {
        string[] options = ["create", "pull", "list", "help"];
        if (options.Contains(a))
        {
            return a switch
            {
                "create" => (int)await Create.RunAsync(),
                "pull" => (int)await Pull.RunAsync(),
                "list" => (int)List.Run(),
                "help" => (int)Help.Run(),
                _ => (int)Help.Run(),
            };
        };
    }

    return (int)Help.Run();
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
    return (int)ActivityStatusCode.Error;
}

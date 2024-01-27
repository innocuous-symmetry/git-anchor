using System.Diagnostics;
using GitAnchor.Actions;

string[] inputArgs = Environment.GetCommandLineArgs();

SortedDictionary<string, CallableAction> options = new()
{
    { "create", Create.Run },
    { "pull", Pull.Run },
    { "help", Help.Run }
};

try
{
    foreach (string a in inputArgs)
    {
        if (a == "") continue;

        var result = options.TryGetValue(a, out CallableAction? action);

        if (!result) continue;
        if (action != null) return (int)await action();
    }

    return (int)await Help.Run();
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
    return (int)ActivityStatusCode.Error;
}

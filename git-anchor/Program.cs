using System.Collections;
using System.Diagnostics;
using System.Reflection;
using GitAnchor.Actions;

string[] inputArgs = Environment.GetCommandLineArgs();


// create pointers for entrypoints for each action
var createFn = typeof(Create).GetMethod("RunAsync", BindingFlags.Static | BindingFlags.Public);
var pullFn = typeof(Pull).GetMethod("RunAsync", BindingFlags.Static | BindingFlags.Public);
var statusFn = typeof(Status).GetMethod("RunAsync", BindingFlags.Static | BindingFlags.Public);
var listFn = typeof(List).GetMethod("Run", BindingFlags.Static | BindingFlags.Public);
var helpFn = typeof(Help).GetMethod("Run", BindingFlags.Static | BindingFlags.Public);

var AllOptions = new Hashtable()
{
    { "create", createFn },
    { "pull", pullFn },
    { "status", statusFn },
    { "list", listFn },
    { "help", helpFn },
};

try
{
    foreach (string a in inputArgs)
    {
        var titleCase = a.ToUpperInvariant();

        if (AllOptions.ContainsKey(a))
        {
            var func = (MethodInfo)AllOptions[a];
            var result = func?.Invoke(null, null);

            if (result is Task<ActivityStatusCode> task)
            {
                return (int)task.Result;
            }
        };
    }

    return (int)Help.Run();
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
    return (int)ActivityStatusCode.Error;
}

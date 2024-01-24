using System.Collections;
using System.Diagnostics;
using GitAnchor.Actions;

namespace GitAnchor.Lib;

public class EntryPoint
{
    public static async Task<ActivityStatusCode> Run()
    {
        string[] args = Environment.GetCommandLineArgs();

        SortedDictionary<string, Task<ActivityStatusCode>> options = new()
        {
            { "create", Create.Run() },
            { "pull", Pull.Run() },
            { "help", Help.Run() }
        };

        try
        {
            foreach (string arg in args)
            {
                var result = options.TryGetValue(arg, out Task<ActivityStatusCode>? action);

                if (!result) continue;
                if (action != null) return await action;
            }

            return await Help.Run();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return ActivityStatusCode.Error;
        }
    }
}

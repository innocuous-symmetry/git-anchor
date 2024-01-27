using System.Diagnostics;
using GitAnchor.Lib;

namespace GitAnchor.Actions;

/** represent a function type matching either of the below class members */
// public delegate Task<ActivityStatusCode> Callable();

public abstract class BaseAction {
    public virtual void Run()
    {
        throw new NotImplementedException();
    }

    public virtual async Task<ActivityStatusCode> RunAsync()
    {
        throw new NotImplementedException();
    }
}

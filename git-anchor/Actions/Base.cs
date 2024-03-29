using System.Diagnostics;
namespace GitAnchor.Actions;

public abstract class BaseAction {
    public virtual void Run()
    {
        throw new NotImplementedException();
    }

    public virtual async Task<ActivityStatusCode> RunAsync()
    {
        await Task.Delay(0);
        throw new NotImplementedException();
    }
}

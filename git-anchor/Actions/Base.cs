using System.Diagnostics;

namespace GitAnchor.Actions;

public abstract class BaseAction {
    public virtual async Task<ActivityStatusCode> Run()
    {
        throw new NotImplementedException();
    }
}

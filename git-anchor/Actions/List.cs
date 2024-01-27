using System.Diagnostics;

namespace GitAnchor.Actions;
class List : BaseAction
{
    static async new Task<ActivityStatusCode> Run()
    {
        return ActivityStatusCode.Ok;
    }

}

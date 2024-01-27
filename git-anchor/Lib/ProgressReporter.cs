using System.Diagnostics;

namespace GitAnchor.Lib;

public class ProgressReporter : Stopwatch
{
    public ProgressReporter()
    {
    }

    public override string ToString()
    {
        var elapsed = this.ElapsedMilliseconds < 1000
            ? this.ElapsedMilliseconds
            : this.ElapsedMilliseconds / 1000;

        string unit = this.ElapsedMilliseconds < 1000 ? "ms" : "s";

        return $"All tasks completed in {elapsed}{unit}. Exiting...";
    }
}

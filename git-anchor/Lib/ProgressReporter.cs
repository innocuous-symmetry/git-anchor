using System.Diagnostics;

namespace GitAnchor.Lib;

public class ProgressReporter : Stopwatch, IDisposable
{
    public ProgressReporter()
    {
        // progress = new();
    }

    public void Dispose()
    {
        this.Stop();
        // progress = null!;
        GC.SuppressFinalize(this);
    }

    public override string ToString()
    {
        // progress.ProgressChanged += (sender, message) => Console.WriteLine(message);

        var elapsed = this.ElapsedMilliseconds < 1000
            ? this.ElapsedMilliseconds
            : this.ElapsedMilliseconds / 1000;

        string unit = this.ElapsedMilliseconds < 1000 ? "ms" : "s";

        return $"All tasks completed in {elapsed}{unit}. Exiting...";
    }
}

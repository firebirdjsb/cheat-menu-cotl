using System;
using System.Threading.Tasks;

namespace CheatMenu;

/// <summary>
/// Helper class for async/await operations in Unity context.
/// Provides simple task delays for coroutine-like behavior.
/// </summary>
public static class AsyncHelper
{
    /// <summary>
    /// Creates a task that completes after the specified number of seconds.
    /// </summary>
    /// <param name="seconds">Number of seconds to wait.</param>
    /// <returns>A Task that completes after the delay.</returns>
    public static System.Threading.Tasks.Task WaitSeconds(int seconds)
    {
        return System.Threading.Tasks.Task.Delay(TimeSpan.FromSeconds(seconds));
    }
}

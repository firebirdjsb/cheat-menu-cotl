using System.Collections.Generic;

namespace CheatMenu;

/// <summary>
/// Utility class providing common helper methods used across the cheat menu.
/// Contains generic collection operations and debug mode detection.
/// </summary>
public static class CheatUtils
{
    /// <summary>
    /// Creates a shallow clone of a list by copying all elements to a new list instance.
    /// This is useful when you need to iterate over a collection while modifying the original.
    /// </summary>
    /// <typeparam name="T">The type of elements in the list.</typeparam>
    /// <param name="list">The list to clone.</param>
    /// <returns>A new list containing all elements from the input list.</returns>
    /// <remarks>
    /// This performs a shallow copy - references to objects are copied, not the objects themselves.
    /// Use this when iterating over DataManager follower lists to avoid collection modification errors.
    /// </remarks>
    public static List<T> CloneList<T>(List<T> list)
    {
        return new List<T>(list);
    }

    /// <summary>
    /// Gets whether the cheat menu is running in debug mode.
    /// </summary>
    /// <value>True if compiled with DEBUG symbol; otherwise false.</value>
    public static bool IsDebugMode
    {
        get
        {
#if DEBUG
            return true;
#else
            return false;
#endif
        }
    }

    /// <summary>
    /// Concatenates two arrays of the same type into a single combined array.
    /// </summary>
    /// <typeparam name="T">The type of elements in both arrays.</typeparam>
    /// <param name="first">The first array.</param>
    /// <param name="second">The second array to append.</param>
    /// <returns>A new array containing all elements from both input arrays.</returns>
    public static T[] Concat<T>(T[] first, T[] second)
    {
        var combined = new T[first.Length + second.Length];
        first.CopyTo(combined, 0);
        second.CopyTo(combined, first.Length);
        return combined;
    }
}

using System;
using System.IO;
using BepInEx.Logging;

namespace CheatMenu;

/// <summary>
/// Structured logging system for Cheat Menu mod.
/// Provides categorized logging with detailed error information and stack traces.
/// </summary>
public static class Logger
{
    // Log category prefixes
    public const string INIT = "[INIT]";
    public const string CHEAT = "[CHEAT]";
    public const string SAVE = "[SAVE]";
    public const string FLAGS = "[FLAGS]";
    public const string PATCH = "[PATCH]";
    public const string GUI = "[GUI]";

    private static ManualLogSource _logger;
    private static bool _isInitialized;

    /// <summary>
    /// Initializes the Logger with the BepInEx logger reference.
    /// </summary>
    /// <param name="logger">The BepInEx ManualLogSource to use for logging.</param>
    public static void Initialize(ManualLogSource logger)
    {
        _logger = logger;
        _isInitialized = true;
    }

    /// <summary>
    /// Gets the current timestamp in a readable format.
    /// </summary>
    private static string GetTimestamp()
    {
        return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
    }

    /// <summary>
    /// Core logging method for info messages.
    /// </summary>
    public static void Info(string category, string message)
    {
        try
        {
            if (!_isInitialized || _logger == null)
            {
                Console.WriteLine($"{GetTimestamp()} {category} {message}");
                return;
            }
            _logger.LogInfo($"{category} {message}");
        }
        catch
        {
            // Fail-safe: never throw from logger
            try { Console.WriteLine($"{GetTimestamp()} {category} {message}"); } catch { }
        }
    }

    /// <summary>
    /// Core logging method for warning messages.
    /// </summary>
    public static void Warning(string category, string message)
    {
        try
        {
            if (!_isInitialized || _logger == null)
            {
                Console.WriteLine($"{GetTimestamp()} {category} WARNING: {message}");
                return;
            }
            _logger.LogWarning($"{category} {message}");
        }
        catch
        {
            // Fail-safe: never throw from logger
            try { Console.WriteLine($"{GetTimestamp()} {category} WARNING: {message}"); } catch { }
        }
    }

    /// <summary>
    /// Core logging method for error messages.
    /// </summary>
    public static void Error(string category, string message)
    {
        try
        {
            if (!_isInitialized || _logger == null)
            {
                Console.WriteLine($"{GetTimestamp()} {category} ERROR: {message}");
                return;
            }
            _logger.LogError($"{category} {message}");
        }
        catch
        {
            // Fail-safe: never throw from logger
            try { Console.WriteLine($"{GetTimestamp()} {category} ERROR: {message}"); } catch { }
        }
    }

    /// <summary>
    /// Core logging method for exception with full stack trace.
    /// </summary>
    public static void Error(string category, Exception ex)
    {
        if (ex == null) return;
        
        string stackTrace = Environment.StackTrace;
        string errorMessage = $"{category} Exception: {ex.GetType().Name}: {ex.Message}\nStackTrace: {stackTrace}";
        
        try
        {
            if (!_isInitialized || _logger == null)
            {
                Console.WriteLine($"{GetTimestamp()} {errorMessage}");
                return;
            }
            _logger.LogError(errorMessage);
        }
        catch
        {
            // Fail-safe: never throw from logger
            try { Console.WriteLine($"{GetTimestamp()} {errorMessage}"); } catch { }
        }
    }

    /// <summary>
    /// Core logging method for exception with message and full stack trace.
    /// </summary>
    public static void Error(string category, string message, Exception ex)
    {
        if (ex == null)
        {
            Error(category, message);
            return;
        }
        
        string stackTrace = Environment.StackTrace;
        string errorMessage = $"{category} {message}\nException: {ex.GetType().Name}: {ex.Message}\nStackTrace: {stackTrace}";
        
        try
        {
            if (!_isInitialized || _logger == null)
            {
                Console.WriteLine($"{GetTimestamp()} {errorMessage}");
                return;
            }
            _logger.LogError(errorMessage);
        }
        catch
        {
            // Fail-safe: never throw from logger
            try { Console.WriteLine($"{GetTimestamp()} {errorMessage}"); } catch { }
        }
    }

    // ========== Convenience Methods for Each Category ==========

    /// <summary>
    /// Logs an initialization message with [INIT] prefix.
    /// </summary>
    public static void Init(string message)
    {
        Info(INIT, message);
    }

    /// <summary>
    /// Logs a cheat-related message with [CHEAT] prefix.
    /// </summary>
    public static void Cheat(string message)
    {
        Info(CHEAT, message);
    }

    /// <summary>
    /// Logs a save-related message with [SAVE] prefix.
    /// </summary>
    public static void Save(string message)
    {
        Info(SAVE, message);
    }

    /// <summary>
    /// Logs a flag-related message with [FLAGS] prefix.
    /// </summary>
    public static void Flags(string message)
    {
        Info(FLAGS, message);
    }

    /// <summary>
    /// Logs a patch-related message with [PATCH] prefix.
    /// </summary>
    public static void Patch(string message)
    {
        Info(PATCH, message);
    }

    /// <summary>
    /// Logs a GUI-related message with [GUI] prefix.
    /// </summary>
    public static void Gui(string message)
    {
        Info(GUI, message);
    }

    // ========== Error Variants with Stack Traces ==========

    /// <summary>
    /// Logs a cheat execution error with the cheat name and full stack trace.
    /// Used when reflection fails during cheat execution.
    /// </summary>
    /// <param name="cheatName">Name of the cheat that failed.</param>
    /// <param name="ex">The exception that occurred.</param>
    public static void CheatError(string cheatName, Exception ex)
    {
        if (ex == null) return;
        
        string stackTrace = Environment.StackTrace;
        string errorMessage = $"Failed executing {cheatName}\nStackTrace: {stackTrace}";
        
        try
        {
            if (!_isInitialized || _logger == null)
            {
                Console.WriteLine($"{GetTimestamp()} {CHEAT} {errorMessage}");
                return;
            }
            _logger.LogError($"{CHEAT} {errorMessage}");
        }
        catch
        {
            // Fail-safe: never throw from logger
            try { Console.WriteLine($"{GetTimestamp()} {CHEAT} {errorMessage}"); } catch { }
        }
    }

    /// <summary>
    /// Logs a save-related error with message and full stack trace.
    /// </summary>
    /// <param name="message">Description of the save error.</param>
    /// <param name="ex">The exception that occurred.</param>
    public static void SaveError(string message, Exception ex)
    {
        if (ex == null)
        {
            Error(SAVE, message);
            return;
        }
        
        string stackTrace = Environment.StackTrace;
        string errorMessage = $"{message}\nStackTrace: {stackTrace}";
        
        try
        {
            if (!_isInitialized || _logger == null)
            {
                Console.WriteLine($"{GetTimestamp()} {SAVE} ERROR: {errorMessage}");
                return;
            }
            _logger.LogError($"{SAVE} {errorMessage}");
        }
        catch
        {
            // Fail-safe: never throw from logger
            try { Console.WriteLine($"{GetTimestamp()} {SAVE} ERROR: {errorMessage}"); } catch { }
        }
    }
}

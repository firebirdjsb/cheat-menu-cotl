namespace CheatMenu;

/// <summary>
/// Harmony patches for follower spawning mechanics.
/// </summary>
/// <remarks>
/// Global spawn patches were removed to avoid stripping the Spy trait from
/// legitimately spawned spy followers. Spy trait removal is now handled per
/// cheat call instead of globally to preserve legitimate gameplay mechanics.
/// This ensures that spy followers spawned through normal game events retain
/// their spy trait while cheats that spawn followers can remove it as needed.
/// </remarks>
public static class FollowerSpawnPatches {
    /// <summary>
    /// Initializes follower spawn patch system.
    /// Currently a placeholder as global patches were removed.
    /// </summary>
    [Init]
    public static void Init(){
    }
}

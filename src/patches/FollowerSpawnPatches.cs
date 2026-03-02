namespace CheatMenu;

// Global spawn patches removed to avoid stripping the Spy trait from
// legitimately spawned spy followers. Spy removal is handled per cheat call.
public static class FollowerSpawnPatches {
    [Init]
    public static void Init(){
    }
}

namespace CheatMenu;

[CheatCategory(CheatCategoryEnum.FOLLOWER)]
public class FollowerDefinitions : IDefinition{
    [CheatDetails("Spawn Follower (Worker)", "Spawns and auto-indoctrinates a follower as a worker")]
    public static void SpawnWorkerFollower(){
        CultUtils.SpawnFollower(FollowerRole.Worker);
        CultUtils.PlayNotification("Worker follower spawned!");
    }

    [CheatDetails("Spawn Follower (Worshipper)", "Spawns and auto-indoctrinates a follower as a worshipper")]
    public static void SpawnWorkerWorshipper(){
        CultUtils.SpawnFollower(FollowerRole.Worshipper);
        CultUtils.PlayNotification("Worshipper follower spawned!");
    }

    [CheatDetails("Spawn 'Arrived' Follower", "Spawns a follower ready for indoctrination")]
    public static void SpawnArrivedFollower(){
        FollowerManager.CreateNewRecruit(FollowerLocation.Base, NotificationCentre.NotificationType.NewRecruit);
        CultUtils.PlayNotification("New follower arrived!");
    }

    [CheatDetails("Turn all Followers Young", "Changes the age of all followers to young")]
    [CheatWIP]
    public static void TurnAllFollowersYoung(){
        var followers = DataManager.Instance.Followers;
        foreach(var follower in followers)
        {
            CultUtils.TurnFollowerYoung(follower);
        }
        CultUtils.PlayNotification("All followers are young now!");
    }

    [CheatDetails("Turn all Followers Old", "Changes the age of all followers to old")]
    [CheatWIP]
    public static void TurnAllFollowersOld(){
        var followers = DataManager.Instance.Followers;
        foreach(var follower in followers)
        {
           CultUtils.TurnFollowerOld(follower);
        }
        CultUtils.PlayNotification("All followers are old now!");
    }

    [CheatDetails("Kill All Followers", "Kills all followers at the Base")]
    public static void KillAllFollowers(){
        var followers = DataManager.Instance.Followers;
        foreach (var follower in followers)
        {
            CultUtils.KillFollower(CultUtils.GetFollower(follower), false);
        }
        CultUtils.PlayNotification("All followers killed!");
    }

    [CheatDetails("Revive All Followers", "Revive all currently dead followers")]
    public static void ReviveAllFollowers(){
        var followers = CheatUtils.CloneList(DataManager.Instance.Followers_Dead);
        foreach (var follower in followers)
        {
            CultUtils.ReviveFollower(follower);
        }
        CultUtils.PlayNotification("All followers revived!");
    }

    [CheatDetails("Remove Sickness", "Clears sickness from all followers, cleanups any vomit, poop or dead bodies and clears outhouses")]
    public static void RemoveSickness(){
        CultUtils.ClearPoop();
        CultUtils.ClearBodies();
        CultUtils.ClearVomit();
        CultUtils.ClearOuthouses();
        foreach (var follower in DataManager.Instance.Followers)
        {
            CultUtils.CureIllness(follower);
        }
        CultUtils.PlayNotification("Cured all followers :)");
    }

    [CheatDetails("Convert Dissenting Followers", "Converts dissenting followers back to regular followers")]
    public static void ConvertAllDissenting(){
        foreach (var follower in DataManager.Instance.Followers)
        {
            CultUtils.ConvertDissenting(follower);
        }
        CultUtils.PlayNotification("Converted all followers :)");
    }

    [CheatDetails("Clear Faith", "Set the current faith to zero")]
    public static void ClearFaith(){
        CultUtils.ModifyFaith(0f, "Cleared faith :)");
    }

    [CheatDetails("Remove Hunger", "Clears starvation from any followers and maximazes satiation for all followers")]
    public static void RemoveHunger(){
        foreach (var follower in DataManager.Instance.Followers)
        {
            CultUtils.MaximizeSatiationAndRemoveStarvation(follower);
        }
        CultUtils.PlayNotification("Everyone is full! :)");
    }

    [CheatDetails("Max Faith", "Clear the cult's thoughts and gives them large positive ones")]
    public static void MaxFaith(){
        CultUtils.ClearAndAddPositiveFollowerThought();
    }

    [CheatDetails("Level Up All Followers", "Sets all follower levels to max (10)")]
    public static void LevelUpAllFollowers(){
        try {
            foreach (var follower in DataManager.Instance.Followers)
            {
                follower.XPLevel = 10;
                HarmonyLib.Traverse.Create(follower).Field("XP").SetValue(0f);
            }
            CultUtils.PlayNotification("All followers leveled to max!");
        } catch(System.Exception e){
            UnityEngine.Debug.LogWarning($"Failed to level followers: {e.Message}");
            CultUtils.PlayNotification("Failed to level followers!");
        }
    }

    [CheatDetails("Increase Follower Loyalty", "Levels up loyalty for all followers by 1")]
    public static void IncreaseFollowerLoyalty(){
        try {
            foreach (var follower in DataManager.Instance.Followers)
            {
                Follower f = CultUtils.GetFollowerFromInfo(follower);
                if(f != null && f.Brain != null){
                    HarmonyLib.Traverse.Create(f.Brain).Method("AddAdoration", new System.Type[]{typeof(int), typeof(float)}).GetValue(0, 100f);
                }
            }
            CultUtils.PlayNotification("Follower loyalty increased!");
        } catch(System.Exception e){
            UnityEngine.Debug.LogWarning($"Failed to increase loyalty: {e.Message}");
            CultUtils.PlayNotification("Failed to increase loyalty!");
        }
    }
}
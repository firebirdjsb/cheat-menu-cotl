using System;
using UnityEngine;

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

    [CheatDetails("Spawn Child Follower", "Spawns a child follower at the base")]
    public static void SpawnChildFollower(){
        try {
            Follower follower = FollowerManager.CreateNewFollower(FollowerLocation.Base, PlayerFarming.Instance.transform.position, false);
            follower.Brain.MakeChild();
            CultUtils.PlayNotification("Child follower spawned!");
        } catch(Exception e){
            Debug.LogWarning($"Failed to spawn child: {e.Message}");
            CultUtils.PlayNotification("Failed to spawn child!");
        }
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

    [CheatDetails("Kill Random Follower", "Kills a random follower")]
    public static void KillRandomFollower(){
        try {
            HarmonyLib.Traverse.Create(typeof(CheatConsole)).Method("KillRandomFollower").GetValue();
            CultUtils.PlayNotification("Random follower killed!");
        } catch(Exception e){
            Debug.LogWarning($"Failed to kill random follower: {e.Message}");
            CultUtils.PlayNotification("Failed to kill random follower!");
        }
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

    [CheatDetails("Remove Hunger", "Clears starvation from any followers and maximizes satiation for all followers")]
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
        } catch(Exception e){
            Debug.LogWarning($"Failed to level followers: {e.Message}");
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
                    HarmonyLib.Traverse.Create(f.Brain).Method("AddAdoration", new Type[]{typeof(int), typeof(float)}).GetValue(0, 100f);
                }
            }
            CultUtils.PlayNotification("Follower loyalty increased!");
        } catch(Exception e){
            Debug.LogWarning($"Failed to increase loyalty: {e.Message}");
            CultUtils.PlayNotification("Failed to increase loyalty!");
        }
    }

    [CheatDetails("Make All Followers Immortal", "Adds the Immortal trait to all followers")]
    public static void MakeAllFollowersImmortal(){
        try {
            int count = 0;
            foreach(var follower in DataManager.Instance.Followers){
                if(!follower.Traits.Contains(FollowerTrait.TraitType.Immortal)){
                    follower.Traits.Add(FollowerTrait.TraitType.Immortal);
                    count++;
                }
            }
            CultUtils.PlayNotification($"{count} follower(s) made immortal!");
        } catch(Exception e){
            Debug.LogWarning($"Failed to make followers immortal: {e.Message}");
            CultUtils.PlayNotification("Failed to make followers immortal!");
        }
    }

    [CheatDetails("Max All Follower Stats", "Max out faith, satiation and clear starvation for all followers")]
    public static void MaxAllFollowerStats(){
        foreach(var follower in DataManager.Instance.Followers){
            CultUtils.SetFollowerFaith(follower, 100f);
            CultUtils.MaximizeSatiationAndRemoveStarvation(follower);
            CultUtils.CureIllness(follower);
        }
        CultUtils.PlayNotification("All follower stats maxed!");
    }

    [CheatDetails("Remove Exhaustion", "Clears exhaustion from all followers so they can work again")]
    public static void RemoveExhaustion(){
        try {
            int count = 0;
            foreach(var follower in DataManager.Instance.Followers){
                if(follower.Exhaustion > 0f){
                    follower.Exhaustion = 0f;
                    count++;
                }
            }
            CultUtils.PlayNotification(count > 0 ? $"{count} follower(s) rested!" : "No exhausted followers!");
        } catch(Exception e){
            Debug.LogWarning($"Failed to remove exhaustion: {e.Message}");
            CultUtils.PlayNotification("Failed to remove exhaustion!");
        }
    }

    [CheatDetails("Give Follower Tokens", "Gives 10 follower tokens")]
    public static void GiveFollowerTokens(){
        DataManager.Instance.FollowerTokens += 10;
        CultUtils.PlayNotification("10 follower tokens added!");
    }

    [CheatDetails("Reset All Follower Outfits", "EMERGENCY: Resets all follower outfits to default Acolyte Robes (fixes loading issues caused by clothing cheat)")]
    public static void ResetAllFollowerOutfits(){
        try {
            int count = 0;
            // Reset ALL followers (alive, dead, elderly) to default outfit
            // Default state is Clothing=None + Outfit=Follower which renders the
            // default Acolyte Robes (level-based via GetRobesName in the game assembly)
            foreach(var follower in DataManager.Instance.Followers){
                bool needsReset = follower.Outfit == FollowerOutfitType.Custom
                    || follower.Clothing != FollowerClothingType.None
                    || (follower.Clothing != FollowerClothingType.None
                        && follower.Clothing != FollowerClothingType.Naked
                        && TailorManager.GetClothingData(follower.Clothing) == null);
                if(needsReset){
                    follower.Outfit = FollowerOutfitType.Follower;
                    follower.Clothing = FollowerClothingType.None;
                    count++;
                }
            }
            foreach(var follower in DataManager.Instance.Followers_Dead){
                bool needsReset = follower.Outfit == FollowerOutfitType.Custom
                    || follower.Clothing != FollowerClothingType.None
                    || (follower.Clothing != FollowerClothingType.None
                        && follower.Clothing != FollowerClothingType.Naked
                        && TailorManager.GetClothingData(follower.Clothing) == null);
                if(needsReset){
                    follower.Outfit = FollowerOutfitType.Follower;
                    follower.Clothing = FollowerClothingType.None;
                    count++;
                }
            }
            foreach(var followerID in DataManager.Instance.Followers_Elderly_IDs){
                FollowerInfo info = DataManager.Instance.Followers.Find(f => f.ID == followerID);
                if(info != null && info.Outfit != FollowerOutfitType.Old){
                    info.Outfit = FollowerOutfitType.Old;
                    info.Clothing = FollowerClothingType.None;
                }
            }
            CultUtils.PlayNotification($"Reset {count} follower outfit(s) to default Acolyte Robes!");
            UnityEngine.Debug.Log($"[CheatMenu] Reset {count} follower outfits to default (None/Follower) - game should now load properly");
        } catch(Exception e){
            Debug.LogWarning($"Failed to reset follower outfits: {e.Message}");
            CultUtils.PlayNotification("Failed to reset outfits!");
        }
    }
}
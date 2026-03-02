using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace CheatMenu;

[CheatCategory(CheatCategoryEnum.FOLLOWER)]
public class FollowerDefinitions : IDefinition{

    private static bool s_lambGoatChanceEnabled = false;

    [CheatDetails("Spawn Follower (Worker)", "Spawns a follower waiting for indoctrination at the circle", subGroup: "Spawn")]
    public static void SpawnWorkerFollower(){
        CultUtils.SpawnFollower(FollowerRole.Worker);
    }

    [CheatDetails("Spawn Follower (Worshipper)", "Spawns a follower waiting for indoctrination at the circle", subGroup: "Spawn")]
    public static void SpawnWorkerWorshipper(){
        CultUtils.SpawnFollower(FollowerRole.Worshipper);
    }

    [CheatDetails("Spawn 'Arrived' Follower", "Spawns a follower ready for indoctrination at the circle", subGroup: "Spawn")]
    public static void SpawnArrivedFollower(){
        try {
            if(PlayerFarming.Instance == null){
                CultUtils.PlayNotification("Must be in game to spawn followers!");
                return;
            }

            Vector3 spawnPos = GetFollowerSpawnPosition();

            // Use the overload that physically spawns the recruit at the given position
            FollowerRecruit recruit = FollowerManager.CreateNewRecruit(FollowerLocation.Base, spawnPos);
            if(recruit != null){
                CultUtils.PlayNotification("New follower arrived at the circle!");
            } else {
                CultUtils.PlayNotification("Recruit created but not yet visible (check circle)");
            }
        } catch(Exception e){
            UnityEngine.Debug.LogWarning($"[CheatMenu] Failed to spawn arrived follower: {e.Message}");
            CultUtils.PlayNotification("Failed to spawn arrived follower!");
        }
    }

    [CheatDetails("Spawn Child Follower", "Spawns a child follower waiting for indoctrination at the circle", subGroup: "Spawn")]
    public static void SpawnChildFollower(){
        try {
            if(PlayerFarming.Instance == null){
                CultUtils.PlayNotification("Must be in game to spawn followers!");
                return;
            }
            FollowerInfo info = FollowerInfo.NewCharacter(FollowerLocation.Base);
            info.Age = 0;
            Vector3 spawnPos = GetFollowerSpawnPosition();
            FollowerRecruit recruit = FollowerManager.CreateNewRecruit(info, spawnPos);
            CultUtils.PlayNotification(recruit != null ? "Child follower arrived for indoctrination!" : "Child recruit created (check circle)!");
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

    [CheatDetails("Kill All Followers", "Kills all followers at the Base", subGroup: "Life")]
    public static void KillAllFollowers(){
        var followers = DataManager.Instance.Followers;
        foreach (var follower in followers)
        {
            CultUtils.KillFollower(CultUtils.GetFollower(follower), false);
        }
        CultUtils.PlayNotification("All followers killed!");
    }

    [CheatDetails("Kill Random Follower", "Kills a random follower", subGroup: "Life")]
    public static void KillRandomFollower(){
        try {
            HarmonyLib.Traverse.Create(typeof(CheatConsole)).Method("KillRandomFollower").GetValue();
            CultUtils.PlayNotification("Random follower killed!");
        } catch(Exception e){
            Debug.LogWarning($"Failed to kill random follower: {e.Message}");
            CultUtils.PlayNotification("Failed to kill random follower!");
        }
    }

    [CheatDetails("Revive All Followers", "Revive all currently dead followers", subGroup: "Life")]
    public static void ReviveAllFollowers(){
        var followers = CheatUtils.CloneList(DataManager.Instance.Followers_Dead);
        foreach (var follower in followers)
        {
            CultUtils.ReviveFollower(follower);
        }
        CultUtils.PlayNotification("All followers revived!");
    }

    [CheatDetails("Remove Sickness", "Clears sickness from all followers, cleanups any vomit, poop or dead bodies and clears outhouses", subGroup: "Status")]
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

    [CheatDetails("Convert Dissenting Followers", "Converts dissenting followers back to regular followers", subGroup: "Status")]
    public static void ConvertAllDissenting(){
        foreach (var follower in DataManager.Instance.Followers)
        {
            CultUtils.ConvertDissenting(follower);
        }
        CultUtils.PlayNotification("Converted all followers :)");
    }

    [CheatDetails("Clear Faith", "Set the current faith to zero", subGroup: "Status")]
    public static void ClearFaith(){
        CultUtils.ModifyFaith(0f, "Cleared faith :)");
    }

    [CheatDetails("Remove Hunger", "Clears starvation from any followers and maximizes satiation for all followers", subGroup: "Status")]
    public static void RemoveHunger(){
        foreach (var follower in DataManager.Instance.Followers)
        {
            CultUtils.MaximizeSatiationAndRemoveStarvation(follower);
        }
        CultUtils.PlayNotification("Everyone is full! :)");
    }

    [CheatDetails("Max Faith", "Clear the cult's thoughts and gives them large positive ones", subGroup: "Status")]
    public static void MaxFaith(){
        CultUtils.ClearAndAddPositiveFollowerThought();
    }

    [CheatDetails("Level Up All Followers", "Sets all follower levels to max (10)", subGroup: "Stats")]
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

    [CheatDetails("Increase Follower Loyalty", "Levels up loyalty for all followers by 1", subGroup: "Stats")]
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

    [CheatDetails("Make All Followers Immortal", "Adds the Immortal trait to all followers", subGroup: "Life")]
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

    [CheatDetails("Max All Follower Stats", "Max out faith, satiation and clear starvation for all followers", subGroup: "Stats")]
    public static void MaxAllFollowerStats(){
        foreach(var follower in DataManager.Instance.Followers){
            CultUtils.SetFollowerFaith(follower, 100f);
            CultUtils.MaximizeSatiationAndRemoveStarvation(follower);
            CultUtils.CureIllness(follower);
        }
        CultUtils.PlayNotification("All follower stats maxed!");
    }

    [CheatDetails("Remove Exhaustion", "Clears exhaustion from all followers so they can work again", subGroup: "Status")]
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

    [CheatDetails("Give Follower Tokens", "Gives 10 follower tokens", subGroup: "Stats")]
    public static void GiveFollowerTokens(){
        DataManager.Instance.FollowerTokens += 10;
        CultUtils.PlayNotification("10 follower tokens added!");
    }

    [CheatDetails("Reset All Follower Outfits", "EMERGENCY: Resets all follower outfits to default Acolyte Robes (fixes loading issues caused by clothing cheat)", subGroup: "Stats")]
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

    private static Vector3 GetFollowerSpawnPosition(){
        try {
            if(BiomeBaseManager.Instance != null && BiomeBaseManager.Instance.RecruitSpawnLocation != null){
                return BiomeBaseManager.Instance.RecruitSpawnLocation.transform.position;
            }
        } catch { }
        return PlayerFarming.Instance.transform.position;
    }

    private static void SpawnFollowerWithSkin(string skinName){
        try {
            if(PlayerFarming.Instance == null){
                CultUtils.PlayNotification("Must be in game to spawn followers!");
                return;
            }
            DataManager.SetFollowerSkinUnlocked(skinName);
            List<string> blacklist = DataManager.Instance.FollowerSkinsBlacklist;
            bool wasBlacklisted = blacklist.Contains(skinName);
            if(wasBlacklisted) blacklist.RemoveAll(s => s == skinName);
            try {
                FollowerInfo info = FollowerInfo.NewCharacter(FollowerLocation.Base, skinName);
                Vector3 spawnPos = GetFollowerSpawnPosition();
                FollowerRecruit recruit = FollowerManager.CreateNewRecruit(info, spawnPos);
                CultUtils.PlayNotification(recruit != null ? $"{skinName} follower arrived for indoctrination!" : $"{skinName} recruit created (check circle)!");
            } finally {
                if(wasBlacklisted && !blacklist.Contains(skinName)) blacklist.Add(skinName);
            }
        } catch(Exception e){
            Debug.LogWarning($"[CheatMenu] Failed to spawn {skinName} follower: {e.Message}");
            CultUtils.PlayNotification($"Failed to spawn {skinName} follower!");
        }
    }

    [CheatDetails("Spawn Lamb Follower", "Spawns a follower with the Lamb skin", subGroup: "Special Skins")]
    public static void SpawnLambFollower(){
        SpawnFollowerWithSkin("Lamb");
    }

    [CheatDetails("Spawn Abomination", "Spawns a follower with the Abomination skin", subGroup: "Special Skins")]
    public static void SpawnAbomination(){
        SpawnFollowerWithSkin("Abomination");
    }

    [CheatDetails("Spawn Deer Skull", "Spawns a follower with the Deer Skull skin (special event)", subGroup: "Special Skins")]
    public static void SpawnDeerSkull(){
        SpawnFollowerWithSkin("DeerSkull");
    }

    [CheatDetails("Spawn Bat Demon", "Spawns a follower with the Bat Demon skin (special event)", subGroup: "Special Skins")]
    public static void SpawnBatDemon(){
        SpawnFollowerWithSkin("BatDemon");
    }

    [CheatDetails("Spawn Snowman Follower", "Spawns a follower with a random Snowman skin (Woolhaven DLC required)", subGroup: "Special Skins")]
    [RequiresDLC(DlcRequirement.MajorDLC)]
    public static void SpawnSnowman(){
        if(!CultUtils.HasMajorDLC()){ CultUtils.PlayNotification("Requires Woolhaven DLC!"); return; }
        string skin = $"Snowman/Good_{UnityEngine.Random.Range(1, 4)}";
        SpawnFollowerWithSkin(skin);
    }

    [CheatDetails("Spawn Webber", "Spawns a follower with the Webber skin (Woolhaven DLC required)", subGroup: "Special Skins")]
    [RequiresDLC(DlcRequirement.MajorDLC)]
    public static void SpawnWebber(){
        if(!CultUtils.HasMajorDLC()){ CultUtils.PlayNotification("Requires Woolhaven DLC!"); return; }
        SpawnFollowerWithSkin("Webber");
    }

    [CheatDetails("Spawn Crow", "Spawns a follower with the Crow skin (special event)", subGroup: "Special Skins")]
    public static void SpawnCrow(){
        SpawnFollowerWithSkin("Crow");
    }

    [CheatDetails("Spawn Star Bunny", "Spawns a follower with the Star Bunny skin", subGroup: "Special Skins")]
    public static void SpawnStarBunny(){
        SpawnFollowerWithSkin("StarBunny");
    }

    [CheatDetails("Spawn Nightwolf", "Spawns a follower with the Nightwolf skin (Night Fox event)", subGroup: "Special Skins")]
    public static void SpawnNightwolf(){
        SpawnFollowerWithSkin("Nightwolf");
    }

    [CheatDetails("Spawn MassiveMonster", "Spawns a follower with the MassiveMonster dev skin", subGroup: "Special Skins")]
    public static void SpawnMassiveMonster(){
        SpawnFollowerWithSkin("MassiveMonster");
    }

    [CheatDetails("Spawn Poop Follower", "Spawns a follower with the Poop skin", subGroup: "Special Skins")]
    public static void SpawnPoop(){
        SpawnFollowerWithSkin("Poop");
    }

    [CheatDetails("Spawn Midas", "Spawns a follower with the King Midas skin", subGroup: "Special Skins")]
    public static void SpawnMidas(){
        SpawnFollowerWithSkin("Midas");
    }

    [CheatDetails("Spawn Sozo", "Spawns a follower with the Sozo skin", subGroup: "Special Skins")]
    public static void SpawnSozo(){
        SpawnFollowerWithSkin("Sozo");
    }

    [CheatDetails("Spawn Chosen Child", "Spawns a follower with the Chosen Child skin", subGroup: "Special Skins")]
    public static void SpawnChosenChild(){
        SpawnFollowerWithSkin("ChosenChild");
    }

    [CheatDetails("Spawn Yngya", "Spawns a follower with the Yngya skin (Woolhaven DLC required)", subGroup: "Special Skins")]
    [RequiresDLC(DlcRequirement.MajorDLC)]
    public static void SpawnYngya(){
        if(!CultUtils.HasMajorDLC()){ CultUtils.PlayNotification("Requires Woolhaven DLC!"); return; }
        SpawnFollowerWithSkin("Yngya");
    }

    [CheatDetails("Spawn Executioner", "Spawns a follower with the Executioner skin", subGroup: "Special Skins")]
    public static void SpawnExecutioner(){
        SpawnFollowerWithSkin("Executioner");
    }

    [CheatDetails("Spawn Narayana", "Spawns a follower with the Narayana skin (first free follower)", subGroup: "Special Skins")]
    public static void SpawnNarayana(){
        SpawnFollowerWithSkin("Narayana");
    }

    [CheatDetails("Spawn Volvy", "Spawns a follower with the Volvy skin", subGroup: "Special Skins")]
    public static void SpawnVolvy(){
        SpawnFollowerWithSkin("Volvy");
    }

    [CheatDetails("Spawn Leshy (CultLeader 1)", "Spawns a follower with the Leshy bishop skin", subGroup: "Special Skins")]
    public static void SpawnLeshy(){
        SpawnFollowerWithSkin("CultLeader 1");
    }

    [CheatDetails("Spawn Heket (CultLeader 2)", "Spawns a follower with the Heket bishop skin", subGroup: "Special Skins")]
    public static void SpawnHeket(){
        SpawnFollowerWithSkin("CultLeader 2");
    }

    [CheatDetails("Spawn Kallamar (CultLeader 3)", "Spawns a follower with the Kallamar bishop skin", subGroup: "Special Skins")]
    public static void SpawnKallamar(){
        SpawnFollowerWithSkin("CultLeader 3");
    }

    [CheatDetails("Spawn Shamura (CultLeader 4)", "Spawns a follower with the Shamura bishop skin", subGroup: "Special Skins")]
    public static void SpawnShamura(){
        SpawnFollowerWithSkin("CultLeader 4");
    }

    [CheatDetails("Spawn Baal", "Spawns a follower with the Boss Baal skin", subGroup: "Special Skins")]
    public static void SpawnBaal(){
        SpawnFollowerWithSkin("Boss Baal");
    }

    [CheatDetails("Spawn Aym", "Spawns a follower with the Boss Aym skin", subGroup: "Special Skins")]
    public static void SpawnAym(){
        SpawnFollowerWithSkin("Boss Aym");
    }

    public static void Postfix_GetRandomAvailableSkin(ref int __result){
        if(!s_lambGoatChanceEnabled) return;
        try {
            if(UnityEngine.Random.Range(0f, 1f) < 0.08f){
                string[] specialSkins = { "Lamb", "DeerSkull", "BatDemon", "StarBunny", "Crow", "Abomination", "Nightwolf" };
                string skin = specialSkins[UnityEngine.Random.Range(0, specialSkins.Length)];
                DataManager.SetFollowerSkinUnlocked(skin);
                int idx = WorshipperData.Instance.GetSkinIndexFromName(skin);
                if(idx > 0) __result = idx;
            }
        } catch { }
    }

    [CheatDetails("Special Skin Spawn Chance", "Special Chance (OFF)", "Special Chance (ON)",
        "Gives an 8% chance for any new follower to spawn with a special skin", true, subGroup: "Special Skins")]
    public static void ToggleLambGoatChance(bool flag){
        s_lambGoatChanceEnabled = flag;
        try {
            if(flag){
                MethodInfo patch = typeof(FollowerDefinitions).GetMethod(nameof(Postfix_GetRandomAvailableSkin), BindingFlags.Static | BindingFlags.Public);
                ReflectionHelper.PatchMethodPostfix(
                    typeof(WorshipperData),
                    "GetRandomAvailableSkin",
                    patch,
                    BindingFlags.Instance | BindingFlags.Public,
                    silent: true
                );
                DataManager.SetFollowerSkinUnlocked("Lamb");
            } else {
                ReflectionHelper.UnpatchTracked(typeof(WorshipperData), "GetRandomAvailableSkin");
            }
        } catch(Exception e){
            Debug.LogWarning($"[CheatMenu] ToggleLambGoatChance patch failed: {e.Message}");
        }
        CultUtils.PlayNotification(flag ? "Special skin spawn chance enabled!" : "Special skin spawn chance disabled!");
    }
}
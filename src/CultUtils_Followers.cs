using Lamb.UI;
using System;
using System.Collections;
using src.Extensions;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using HarmonyLib;
using Spine.Unity;

namespace CheatMenu;

// ============================================================================
// PARTIAL FILE: CultUtils_Followers.cs
// Contains: Follower spawning, stats, traits, age, illness, revive, kill, 
//           thoughts, bodies, and related follower management methods
// ============================================================================

internal static partial class CultUtils {
    // -- Follower State Management --------------------------------------------

    public static void SetFollowerFaith(FollowerInfo followerInfo, float value){
        followerInfo.Faith = UnityEngine.Mathf.Clamp(value, 0, 100);
    }

    public static void SetFollowerSatiation(FollowerInfo followerInfo, float value){
        followerInfo.Satiation = UnityEngine.Mathf.Clamp(value, 0, 100);
    }
       
    public static void SetFollowerStarvation(FollowerInfo followerInfo, float value){
        if(value > 0){
            followerInfo.Starvation = UnityEngine.Mathf.Clamp(value, 0, 75);
        } else {
            followerInfo.Starvation = 0f;
        }
    }

    public static void ConvertDissenting(FollowerInfo followerInfo){
        try {
            if(followerInfo == null) return;
            
            if(followerInfo.HasThought(Thought.Dissenter)){
                Follower thisFollower = GetFollowerFromInfo(followerInfo);
                if(thisFollower != null){
                    thisFollower.RemoveCursedState(Thought.Dissenter);
                }
                SetFollowerFaith(followerInfo, 100);
            }
        } catch(Exception e){
            UnityEngine.Debug.LogWarning($"[CheatMenu] ConvertDissenting error: {e.Message}");
        }
    }

    public static void MaximizeSatiationAndRemoveStarvation(FollowerInfo followerInfo){
        try {
            if(followerInfo == null) return;
            
            SetFollowerSatiation(followerInfo, 100);
            SetFollowerStarvation(followerInfo, 0);
            
            if(followerInfo.HasThought(Thought.BecomeStarving)){
                Follower thisFollower = GetFollowerFromInfo(followerInfo);
                if(thisFollower != null){
                    thisFollower.RemoveCursedState(Thought.BecomeStarving);
                }
            }
        } catch(Exception e){
            UnityEngine.Debug.LogWarning($"[CheatMenu] MaximizeSatiationAndRemoveStarvation error: {e.Message}");
        }
    }

    public static void TurnFollowerYoung(FollowerInfo follower){
        try {
            if(follower == null){
                UnityEngine.Debug.LogWarning("[CheatMenu] TurnFollowerYoung: follower is null");
                return;
            }

            var thisFollower = CultUtils.GetFollowerFromInfo(follower);
            if(thisFollower == null){
                UnityEngine.Debug.LogWarning($"[CheatMenu] TurnFollowerYoung: Could not find follower {follower.ID}");
                return;
            }

            thisFollower.RemoveCursedState(Thought.OldAge);
            thisFollower.Brain.ClearThought(Thought.OldAge);
            follower.Age = 0;
            follower.OldAge = false;
            thisFollower.Brain.CheckChangeState();
            DataManager.Instance.Followers_Elderly_IDs.Remove(follower.ID);
            
            // Only change outfit if follower is loaded and has outfit component
            if(thisFollower.Outfit != null && thisFollower.Outfit.CurrentOutfit == FollowerOutfitType.Old){
                try {
                    thisFollower.SetOutfit(FollowerOutfitType.Follower, false, Thought.None);
                } catch(Exception e){
                    UnityEngine.Debug.LogWarning($"[CheatMenu] Failed to set young outfit for follower {follower.ID}: {e.Message}");
                }
            }
        } catch(Exception e){
            UnityEngine.Debug.LogWarning($"[CheatMenu] TurnFollowerYoung error: {e.Message}");
        }
    }

    public static void TurnFollowerOld(FollowerInfo follower){
        try {
            if(follower == null){
                UnityEngine.Debug.LogWarning("[CheatMenu] TurnFollowerOld: follower is null");
                return;
            }

            Follower thisFollower = CultUtils.GetFollowerFromInfo(follower);
            if(thisFollower == null){
                UnityEngine.Debug.LogWarning($"[CheatMenu] TurnFollowerOld: Could not find follower {follower.ID}");
                return;
            }

            CultFaithManager.RemoveThought(Thought.OldAge);
            
            try {
                thisFollower.Brain.ApplyCurseState(Thought.OldAge);
            } catch(Exception e){
                UnityEngine.Debug.LogWarning($"[CheatMenu] Failed to apply old age curse for follower {follower.ID}: {e.Message}");
            }
        } catch(Exception e){
            UnityEngine.Debug.LogWarning($"[CheatMenu] TurnFollowerOld error: {e.Message}");
        }
    }

    public static FollowerInfo GetFollowerInfo(Follower follower)
    {
        return follower.Brain._directInfoAccess;
    }

    public static Follower GetFollowerFromInfo(FollowerInfo follower)
    {
        try {
            if(follower == null){
                return null;
            }
            return FollowerManager.FindFollowerByID(follower.ID);
        } catch(Exception e){
            UnityEngine.Debug.LogWarning($"[CheatMenu] GetFollowerFromInfo error for ID {(follower != null ? follower.ID.ToString() : "null")}: {e.Message}");
            return null;
        }
    }

    public static void SetFollowerIllness(FollowerInfo follower, float value)
    {
        follower.Illness = UnityEngine.Mathf.Clamp(value, 0f, 100f);
    }

    public static void ClearAllThoughts()
    {
        var thoughtsList = Traverse.Create(typeof(CultFaithManager)).Field("Thoughts").GetValue<List<ThoughtData>>();
        if(thoughtsList != null){
            thoughtsList.Clear();
        }
        CultFaithManager.GetFaith(0f, 0f, true, NotificationBase.Flair.Positive, "Cleared follower thoughts!", -1);
    }

    public static void ClearAndAddPositiveFollowerThought()
    {
        var thoughtsList = Traverse.Create(typeof(CultFaithManager)).Field("Thoughts").GetValue<List<ThoughtData>>();
        if(thoughtsList != null){
            thoughtsList.Clear();
        }
        foreach(var follower in DataManager.Instance.Followers){
            CultFaithManager.AddThought(Thought.TestPositive, follower.ID, 999);
        }
        ThoughtData data = FollowerThoughts.GetData(Thought.TestPositive);
        CultFaithManager.GetFaith(0f, data.Modifier, true, NotificationBase.Flair.Positive, "Cleared follower thoughts and added positive test thougtht!", -1);
    }

    public static void SetFollowerHunger(FollowerInfo follower, float value)
    {
        follower.Satiation = UnityEngine.Mathf.Clamp(value, 0f, 100f);
    }

    public static Follower GetFollower(FollowerInfo followerInfo)
    {
        return FollowerManager.FindFollowerByID(followerInfo.ID);
    }

    public static void KillFollower(Follower follower,
                                    bool withNotification = false)
    {
        NotificationCentre.NotificationType notifType = withNotification ? NotificationCentre.NotificationType.Died : NotificationCentre.NotificationType.None;
        follower.Die(notifType, false, 1, "dead", null);
    }

    //Similar to the revive that is performed by ritual but makes sure they aren't ill / hungry
    public static void ReviveFollower(FollowerInfo follower)
    {
        if (DataManager.Instance.Followers_Dead_IDs.Contains(follower.ID))
        {
            DataManager.Instance.Followers_Dead.Remove(follower);
            DataManager.Instance.Followers_Dead_IDs.Remove(follower.ID);

            Follower revivedFollower = FollowerManager.CreateNewFollower(follower, PlayerFarming.Instance.transform.position, false);
            if (follower.Age > follower.LifeExpectancy)
            {
                follower.LifeExpectancy = follower.Age + UnityEngine.Random.Range(20, 30);
            }
            else
            {
                follower.LifeExpectancy += UnityEngine.Random.Range(20, 30);
            }
            revivedFollower.Brain.ResetStats();
        }
    }

    // -- Follower Spawning -----------------------------------------------------

    public static void SpawnFollower(FollowerRole role)
    {
        try {
            if(PlayerFarming.Instance == null){
                PlayNotification("Must be in game to spawn followers!");
                return;
            }

            // FIX: Clear any existing recruits first to prevent duplicate spawn bug
            try {
                int existingRecruits = DataManager.Instance.Followers_Recruit.Count;
                if(existingRecruits > 0){
                    UnityEngine.Debug.Log($"[CheatMenu] Clearing {existingRecruits} existing recruits before spawning new one");
                    DataManager.Instance.Followers_Recruit.Clear();
                    foreach(var existingRecruit in UnityEngine.Object.FindObjectsOfType<FollowerRecruit>()){
                        UnityEngine.Object.Destroy(existingRecruit.gameObject);
                    }
                }
            } catch(Exception ex){
                UnityEngine.Debug.LogWarning($"[CheatMenu] Error clearing existing recruits: {ex.Message}");
            }

            IsSpawningFollowerFromCheat = true;

            Vector3 circlePos = PlayerFarming.Instance.transform.position;
            try {
                if(BiomeBaseManager.Instance != null && BiomeBaseManager.Instance.RecruitSpawnLocation != null){
                    circlePos = BiomeBaseManager.Instance.RecruitSpawnLocation.transform.position;
                }
            } catch { }
            FollowerRecruit recruit = FollowerManager.CreateNewRecruit(FollowerLocation.Base, circlePos);
            PlayNotification(recruit != null ? "Follower arrived for indoctrination!" : "Recruit created (check circle)!");

            PlayerFarming.Instance.StartCoroutine(ResetSpawnFlagAfterDelay(3f));
        } catch(Exception e){
            UnityEngine.Debug.LogWarning($"[CheatMenu] SpawnFollower error: {e.Message}");
            PlayNotification("Failed to spawn follower!");
            IsSpawningFollowerFromCheat = false;
        }
    }

    private static System.Collections.IEnumerator ResetSpawnFlagAfterDelay(float delay){
        yield return new UnityEngine.WaitForSeconds(delay);
        IsSpawningFollowerFromCheat = false;
        UnityEngine.Debug.Log("[CheatMenu] Reset IsSpawningFollowerFromCheat flag");
    }

    /// <summary>
    /// Spawns a golden follower egg in front of the player (the Woolhaven child).
    /// </summary>
    public static void SpawnFollowerEgg(){
        try {
            if(PlayerFarming.Instance == null){
                PlayNotification("Must be in game to spawn eggs!");
                return;
            }

            DataManager.Instance.ForceGoldenEgg = true;
            var followers = DataManager.Instance.Followers;
            StructuresData eggData = StructuresData.GetInfoByType(StructureBrain.TYPES.EGG_FOLLOWER, 0);
            
            if(followers.Count < 2){
                StructuresData.EggData newEgg = new StructuresData.EggData{
                    EggSeed = UnityEngine.Random.Range(0, 100000),
                    Parent_1_ID = 0,
                    Parent_2_ID = 0,
                    Parent_1_SkinName = "Lamb",
                    Parent_2_SkinName = "Lamb",
                    Parent1Name = "Lamb",
                    Parent2Name = "Lamb",
                    Traits = new System.Collections.Generic.List<FollowerTrait.TraitType>(),
                    Golden = true,
                    Rotting = false,
                    Special = FollowerSpecialType.None
                };

                eggData.EggInfo = newEgg;

                Vector3 eggSpawnPos = PlayerFarming.Instance.transform.position + PlayerFarming.Instance.transform.forward * 2f;
                StructureManager.BuildStructure(FollowerLocation.Base, eggData, eggSpawnPos, Vector2Int.one, false, delegate(UnityEngine.GameObject obj){
                    var eggInteraction = obj.GetComponent<Interaction_EggFollower>();
                    if(eggInteraction != null){
                        eggInteraction.UpdateEgg(false, false, false, true, FollowerSpecialType.None);
                    }
                    var pickUp = obj.GetComponent<PickUp>();
                    if(pickUp != null){
                        pickUp.Bounce = false;
                    }
                }, null, true);

                PlayNotification("Golden follower egg spawned!");
                return;
            }

            var parent1 = followers[UnityEngine.Random.Range(0, followers.Count)];
            var parent2 = followers[UnityEngine.Random.Range(0, followers.Count)];

            StructuresData.EggData childEggData = new StructuresData.EggData{
                EggSeed = UnityEngine.Random.Range(0, 100000),
                Parent_1_ID = parent1.ID,
                Parent_2_ID = parent2.ID,
                Parent_1_SkinName = parent1.SkinName,
                Parent_2_SkinName = parent2.SkinName,
                Parent1Name = parent1.Name,
                Parent2Name = parent2.Name,
                Traits = new System.Collections.Generic.List<FollowerTrait.TraitType>(),
                Golden = true,
                Rotting = false,
                Special = FollowerSpecialType.None
            };

            eggData.EggInfo = childEggData;

            Vector3 childEggSpawnPos = PlayerFarming.Instance.transform.position + PlayerFarming.Instance.transform.forward * 2f;
            StructureManager.BuildStructure(FollowerLocation.Base, eggData, childEggSpawnPos, Vector2Int.one, false, delegate(UnityEngine.GameObject obj){
                var eggInteraction = obj.GetComponent<Interaction_EggFollower>();
                if(eggInteraction != null){
                    eggInteraction.UpdateEgg(false, false, false, true, FollowerSpecialType.None);
                }
                var pickUp = obj.GetComponent<PickUp>();
                if(pickUp != null){
                    pickUp.Bounce = false;
                }
            }, null, true);

            PlayNotification($"Golden egg spawned from {parent1.Name} and {parent2.Name}!");
        } catch(Exception e){
            UnityEngine.Debug.LogWarning($"[CheatMenu] SpawnFollowerEgg error: {e.Message}");
            PlayNotification("Failed to spawn follower egg!");
        }
    }

    // -- Follower Traits & Status ---------------------------------------------

    public static void RemoveSpyStatus(Follower follower, FollowerInfo info){
        try {
            if(info != null){
                try {
                    if(info.Traits != null && info.Traits.Contains(FollowerTrait.TraitType.Spy)){
                        info.Traits.Remove(FollowerTrait.TraitType.Spy);
                    }
                } catch { }
            }

            try {
                if(follower != null && follower.Brain != null && follower.Brain.Info != null){
                    var binfo = follower.Brain.Info;
                    if(binfo.Traits != null && binfo.Traits.Contains(FollowerTrait.TraitType.Spy)){
                        binfo.Traits.Remove(FollowerTrait.TraitType.Spy);
                    }
                }
            } catch { }

            try {
                var dm = DataManager.Instance;
                if(dm != null){
                    var dmType = dm.GetType();
                    var fields = dmType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    foreach(var f in fields){
                        try {
                            var fType = f.FieldType;
                            if(typeof(System.Collections.IList).IsAssignableFrom(fType)){
                                var list = f.GetValue(dm) as System.Collections.IList;
                                if(list == null) continue;
                                for(int i = list.Count - 1; i >= 0; i--){
                                    try {
                                        var item = list[i];
                                        if(item == null) continue;
                                        if(info != null && item is FollowerInfo fi && fi.ID == info.ID){
                                            list.Remove(item);
                                            continue;
                                        }
                                        if(info != null && item is int idVal && idVal == info.ID){
                                            list.RemoveAt(i);
                                            continue;
                                        }
                                        var prop = item.GetType().GetProperty("ID");
                                        if(prop != null){
                                            try {
                                                var val = prop.GetValue(item);
                                                if(val is int iv && info != null && iv == info.ID){
                                                    list.RemoveAt(i);
                                                    continue;
                                                }
                                            } catch { }
                                        }
                                    } catch { }
                                }
                            }
                        } catch { }
                    }
                }
            } catch { }
        } catch { }
    }

    public static void CureIllness(FollowerInfo follower){
        try {
            if(follower == null) return;
            
            follower.Illness = 0f;
            
            Follower actualFollower = GetFollowerFromInfo(follower);
            if(actualFollower != null){
                actualFollower.RemoveCursedState(Thought.Ill);
            }
        } catch(Exception e){
            UnityEngine.Debug.LogWarning($"[CheatMenu] CureIllness error: {e.Message}");
        }
    }

    public static void RemoveSpyFromRecentFollowers(int recentCount = 3){
        try {
            var list = DataManager.Instance?.Followers;
            if(list == null) return;
            int removed = 0;
            for(int i = list.Count - 1; i >= 0 && removed < recentCount; i--){
                try {
                    var finfo = list[i];
                    if(finfo == null) continue;
                    try { if(finfo.Traits != null && finfo.Traits.Contains(FollowerTrait.TraitType.Spy)) finfo.Traits.Remove(FollowerTrait.TraitType.Spy); } catch { }

                    try {
                        var live = FollowerManager.FindFollowerByID(finfo.ID);
                        if(live != null){
                            try { if(live.Brain != null && live.Brain.Info != null && live.Brain.Info.Traits != null && live.Brain.Info.Traits.Contains(FollowerTrait.TraitType.Spy)) live.Brain.Info.Traits.Remove(FollowerTrait.TraitType.Spy); } catch { }
                        }
                    } catch { }

                    removed++;
                } catch { }
            }
            if(removed > 0){
                try { PlayNotification($"Removed Spy trait from {removed} recent follower(s)"); } catch { }
            }
        } catch { }
    }

    // -- Follower Thoughts -----------------------------------------------------

    public static ThoughtData HasThought(FollowerInfo follower, Thought thoughtType)
    {
        foreach(var thought in follower.Thoughts)
        {
            if(thought.ThoughtType == thoughtType)
            {
                return thought;
            }
        }
        return null;
    }

    // -- Follower Bodies ------------------------------------------------------

    public static void ClearBodies(){
        int followerBodies = 0;
        var deadWorshippers = new List<DeadWorshipper>(DeadWorshipper.DeadWorshippers);
        foreach(DeadWorshipper deadWorshipper in deadWorshippers){
            if(deadWorshipper == null || deadWorshipper.followerInfo == null) continue;
            try {
                AddInventoryItem(InventoryItem.ITEM_TYPE.FOLLOWER_MEAT, 5);
                AddInventoryItem(InventoryItem.ITEM_TYPE.BONE, 2);
                if (deadWorshipper.followerInfo.Necklace != InventoryItem.ITEM_TYPE.NONE)
                {
                    AddInventoryItem(deadWorshipper.followerInfo.Necklace, 1);
                }
                deadWorshipper.followerInfo.Necklace = InventoryItem.ITEM_TYPE.NONE;
                StructureManager.RemoveStructure(deadWorshipper.Structure.Brain);
                followerBodies++;
            } catch(Exception e){
                UnityEngine.Debug.LogWarning($"[CheatMenu] Failed to clear dead follower: {e.Message}");
            }
        }

        int animalBodies = 0;
        try {
            var deadAnimals = new List<Interaction_Ranchable>(Interaction_Ranchable.DeadRanchables);
            foreach(var deadAnimal in deadAnimals){
                if(deadAnimal == null || deadAnimal.Animal == null) continue;
                if(deadAnimal.CurrentState == Interaction_Ranchable.State.Dead){
                    try {
                        List<InventoryItem> meatLoot = Interaction_Ranchable.GetMeatLoot(deadAnimal.Animal);
                        foreach(var item in meatLoot){
                            AddInventoryItem((InventoryItem.ITEM_TYPE)item.type, item.quantity);
                        }
                        AddInventoryItem(InventoryItem.ITEM_TYPE.BONE, Structures_Ranch.GetAnimalGrowthState(deadAnimal.Animal));
                        if(deadAnimal.ranch != null){
                            deadAnimal.ranch.Brain.RemoveAnimal(deadAnimal.Animal);
                        }
                        DataManager.Instance.BreakingOutAnimals.Remove(deadAnimal.Animal);
                        DataManager.Instance.DeadAnimalsTemporaryList.Add(deadAnimal.Animal);
                        UnityEngine.Object.Destroy(deadAnimal.gameObject);
                        animalBodies++;
                    } catch(Exception e){
                        UnityEngine.Debug.LogWarning($"[CheatMenu] Failed to clear dead animal: {e.Message}");
                    }
                }
            }
        } catch(Exception e){
            UnityEngine.Debug.LogWarning($"[CheatMenu] Failed to clear dead animals: {e.Message}");
        }

        if(followerBodies == 0 && animalBodies == 0){
            PlayNotification("No dead bodies found!");
        } else {
            PlayNotification($"Bodies cleared! ({followerBodies} follower, {animalBodies} animal)");
        }
    }
}

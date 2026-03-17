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

using DoctrinePairs = Dictionary<SermonCategory, List<Tuple<DoctrineUpgradeSystem.DoctrineType, DoctrineUpgradeSystem.DoctrineType>>>;

// ============================================================================
// PARTIAL FILE: CultUtils_Cult.cs
// Contains: Doctrine, faith, rituals, objectives, cult management
// ============================================================================

internal static partial class CultUtils {
    // -- Doctrine & Cult Management -------------------------------------------

    public static void GiveDoctrineStone(){
        DataManager.Instance.FirstDoctrineStone = true;
        DataManager.Instance.ForceDoctrineStones = true;
        DataManager.Instance.CompletedDoctrineStones += 1;

        PlayerDoctrineStone.OnIncreaseCount?.Invoke();
        CultUtils.PlayNotification("Commandment stone given!");
    }

    public static void CompleteObjective(ObjectivesData objective){
        objective.Complete();
        
        Action onCompleteDelegate = delegate {
            FieldInfo objectiveEvent = typeof(ObjectiveManager).GetField("OnObjectiveCompleted", BindingFlags.Static | BindingFlags.NonPublic);
            if(objectiveEvent != null && objectiveEvent.GetValue(null) != null){
                var eventHandler = objectiveEvent.GetValue(null);
                if(eventHandler != null && eventHandler is ObjectiveManager.ObjectiveUpdated handler){
                    handler(objective);
                }
            }
            return;
        };

        MethodInfo invokeOrQueue = typeof(ObjectiveManager).GetMethod("InvokeOrQueue", BindingFlags.Static | BindingFlags.NonPublic);
        invokeOrQueue?.Invoke(null, new object[]{onCompleteDelegate});
        
        DataManager.Instance.Objectives.Remove(objective);
        DataManager.Instance.CompletedObjectives.Add(objective);
    }


    public static void ClearAllDoctrines(){
        ClearUnlockedRituals();
        DataManager.Instance.CultTraits.Clear();
        DoctrineUpgradeSystem.UnlockedUpgrades.Clear();

        foreach(var category in GetAllSermonCategories()){
            DoctrineUpgradeSystem.SetLevelBySermon(category, 0);
        }
        UpgradeSystem.UnlockAbility(UpgradeSystem.PrimaryRitual1);

        DoctrineUpgradeSystem.UnlockAbility(DoctrineUpgradeSystem.DoctrineType.Special_Bonfire);
        DoctrineUpgradeSystem.UnlockAbility(DoctrineUpgradeSystem.DoctrineType.Special_ReadMind);
        DoctrineUpgradeSystem.UnlockAbility(DoctrineUpgradeSystem.DoctrineType.Special_Sacrifice);
        PlayNotification("Cleared all doctrines!");
    }

    // Specific Winter DLC doctrines that require Woolhaven DLC
    private static readonly DoctrineUpgradeSystem.DoctrineType[] WinterDLCDoctrines = new[]{
        DoctrineUpgradeSystem.DoctrineType.Winter_FurnaceFollower,
        DoctrineUpgradeSystem.DoctrineType.Winter_FurnaceAnimal,
        DoctrineUpgradeSystem.DoctrineType.Winter_ConvertToRot,
        DoctrineUpgradeSystem.DoctrineType.Winter_RemoveRot,
        DoctrineUpgradeSystem.DoctrineType.Winter_ColdEnthusiast_Trait,
        DoctrineUpgradeSystem.DoctrineType.Winter_WorkThroughBlizzard_Trait,
        DoctrineUpgradeSystem.DoctrineType.Winter_RanchMeat,
        DoctrineUpgradeSystem.DoctrineType.Winter_RanchHarvest
    };

    private static bool IsWinterDLCDoctrine(DoctrineUpgradeSystem.DoctrineType type){
        foreach(var dlcType in WinterDLCDoctrines){
            if(type == dlcType) return true;
        }
        return false;
    }

    public static void UnlockAllDoctrines(){
        try {
            bool hasMajorDLC = HasMajorDLC();
            var doctrinePairs = GetAllDoctrinePairs();
            int count = 0;
            foreach(var category in doctrinePairs){
                foreach(var pair in category.Value){
                    bool skipItem1 = !hasMajorDLC && IsWinterDLCDoctrine(pair.Item1);
                    bool skipItem2 = !hasMajorDLC && IsWinterDLCDoctrine(pair.Item2);
                    if(!skipItem1 && pair.Item1 != DoctrineUpgradeSystem.DoctrineType.None && !DoctrineUpgradeSystem.GetUnlocked(pair.Item1)){
                        DoctrineUpgradeSystem.UnlockAbility(pair.Item1);
                        count++;
                    }
                    if(!skipItem2 && pair.Item2 != DoctrineUpgradeSystem.DoctrineType.None && !DoctrineUpgradeSystem.GetUnlocked(pair.Item2)){
                        DoctrineUpgradeSystem.UnlockAbility(pair.Item2);
                        count++;
                    }
                }
            }
            DoctrineUpgradeSystem.UnlockAbility(DoctrineUpgradeSystem.DoctrineType.Special_Bonfire);
            DoctrineUpgradeSystem.UnlockAbility(DoctrineUpgradeSystem.DoctrineType.Special_ReadMind);
            DoctrineUpgradeSystem.UnlockAbility(DoctrineUpgradeSystem.DoctrineType.Special_Sacrifice);
            DoctrineUpgradeSystem.UnlockAbility(DoctrineUpgradeSystem.DoctrineType.Special_Brainwashed);
            DoctrineUpgradeSystem.UnlockAbility(DoctrineUpgradeSystem.DoctrineType.Special_Consume);
            DoctrineUpgradeSystem.UnlockAbility(DoctrineUpgradeSystem.DoctrineType.Special_BecomeDisciple);
            DoctrineUpgradeSystem.UnlockAbility(DoctrineUpgradeSystem.DoctrineType.Special_EmbraceRot);
            DoctrineUpgradeSystem.UnlockAbility(DoctrineUpgradeSystem.DoctrineType.Special_RejectRot);
            DoctrineUpgradeSystem.UnlockAbility(DoctrineUpgradeSystem.DoctrineType.Special_HealingTouch);
            DoctrineUpgradeSystem.UnlockAbility(DoctrineUpgradeSystem.DoctrineType.Winter_Snowman_Ritual);
            DoctrineUpgradeSystem.UnlockAbility(DoctrineUpgradeSystem.DoctrineType.Winter_Follower_Wedding_Ritual);
            DoctrineUpgradeSystem.UnlockAbility(DoctrineUpgradeSystem.DoctrineType.Winter_RitualWarmth);
            DoctrineUpgradeSystem.UnlockAbility(DoctrineUpgradeSystem.DoctrineType.Winter_RitualMidwinter);
            DoctrineUpgradeSystem.UnlockAbility(DoctrineUpgradeSystem.DoctrineType.Winter_Furnace_Full);
            DoctrineUpgradeSystem.UnlockAbility(DoctrineUpgradeSystem.DoctrineType.Winter_Divorce_Ritual);
            DoctrineUpgradeSystem.UnlockAbility(DoctrineUpgradeSystem.DoctrineType.Winter_Rotstone_Spread);
            PlayNotification($"All doctrines unlocked! ({count + 16} abilities)");
        } catch{
            PlayNotification("Failed to unlock all doctrines!");
        }
    }

    //Avoids removing special and player related upgrades
    public static void RemoveDoctrineUpgrades(){
        List<DoctrineUpgradeSystem.DoctrineType> copiedUnlockedUpgrades = new();
        foreach(var unlock in DoctrineUpgradeSystem.UnlockedUpgrades){
            copiedUnlockedUpgrades.Add(unlock);
        }

        foreach(var unlock in DoctrineUpgradeSystem.UnlockedUpgrades){
            var unlockCategory = DoctrineUpgradeSystem.GetCategory(unlock);
            if(unlockCategory != SermonCategory.Special && unlockCategory != SermonCategory.PlayerUpgrade){
                copiedUnlockedUpgrades.Remove(unlock);
            }
        }
        DoctrineUpgradeSystem.UnlockedUpgrades = copiedUnlockedUpgrades;
    }

    public static List<SermonCategory> GetAllSermonCategories(){
        List<SermonCategory> sermonCategories = new();
        foreach(var value in Enum.GetValues(typeof(SermonCategory))){
            SermonCategory sermonCategory = (SermonCategory)value;
            string innerCategoryName = DoctrineUpgradeSystem.GetSermonCategoryLocalizedName(sermonCategory);
            if(innerCategoryName.StartsWith("DoctrineUpgradeSystem")){
                continue;
            } else {
                sermonCategories.Add(sermonCategory);
            }
        }
        return sermonCategories;
    }

    public static int[] GetDoctrineCategoryState(SermonCategory category, List<DoctrineUpgradeSystem.DoctrineType> upgrades = null){
        List<DoctrineUpgradeSystem.DoctrineType> innerUpgrades = upgrades;
        if(innerUpgrades == null){
            innerUpgrades = DoctrineUpgradeSystem.UnlockedUpgrades;
        }
     
        var doctrinePairs = GetAllDoctrinePairs();
        int[] pairStates = new int[4];

        for(int tupleIdx = 0; tupleIdx < pairStates.Length; tupleIdx++){
            var tupleSet = doctrinePairs[category][tupleIdx];                    
            if(innerUpgrades.Contains(tupleSet.Item1)){
                pairStates[tupleIdx] = 1;
            } else if(innerUpgrades.Contains(tupleSet.Item2)){
                pairStates[tupleIdx] = 2;
            } else {
                pairStates[tupleIdx] = 0;
            }
        }
        return pairStates;
    }

    public static void ReapplyAllDoctrinesWithChanges(SermonCategory overridenCategory, int[] stateMap){
        DataManager.Instance.CultTraits.Clear();       
        List<DoctrineUpgradeSystem.DoctrineType> copiedUnlockedUpgrades = new();
        foreach(var unlock in DoctrineUpgradeSystem.UnlockedUpgrades){
            copiedUnlockedUpgrades.Add(unlock);
        }
        RemoveDoctrineUpgrades();

        var categories = GetAllSermonCategories();
        var doctrinePairs = GetAllDoctrinePairs();
        foreach(var category in categories){
            int[] innerStateMap = category == overridenCategory ? stateMap : GetDoctrineCategoryState(category, copiedUnlockedUpgrades);
            for(int pairIdx = 0; pairIdx < innerStateMap.Length; pairIdx++){
                int state = innerStateMap[pairIdx];
                if(state == 1){
                    DoctrineUpgradeSystem.UnlockAbility(doctrinePairs[category][pairIdx].Item1);
                } else if(state == 2){
                    DoctrineUpgradeSystem.UnlockAbility(doctrinePairs[category][pairIdx].Item2);
                }

                if(state != 0){
                    DoctrineUpgradeSystem.SetLevelBySermon(category, pairIdx+1);
                }
            }
        }
    }

    public static void ClearAllCultTraits(){
        foreach(var trait in DataManager.Instance.CultTraits){
            UnityEngine.Debug.Log($"{FollowerTrait.GetLocalizedTitle(trait)}");
        }

        DataManager.Instance.CultTraits.Clear();
    }

    public static Dictionary<UpgradeSystem.Type, UpgradeSystem.Type> GetDictionaryRitualPairs(){
        Dictionary<UpgradeSystem.Type, UpgradeSystem.Type> ritualPairs = new();
        for(int ritualIdx = 0; ritualIdx < UpgradeSystem.SecondaryRitualPairs.Length-1; ritualIdx += 2){
            var item1 = UpgradeSystem.SecondaryRitualPairs[ritualIdx];
            var item2 = UpgradeSystem.SecondaryRitualPairs[ritualIdx+1];

            ritualPairs[item1] = item2;
            ritualPairs[item2] = item1;
        }
        return ritualPairs;
    }

    public static void ClearUnlockedRituals(){
        foreach(var ritual in UpgradeSystem.SecondaryRitualPairs){
            UpgradeSystem.UnlockedUpgrades.Remove(ritual);
        }
        
        foreach(var ritual in UpgradeSystem.SecondaryRituals){
            UpgradeSystem.UnlockedUpgrades.Remove(ritual);
        }

        UpgradeSystem.UnlockedUpgrades.Remove(UpgradeSystem.PrimaryRitual1);
    }

    public static DoctrinePairs GetAllDoctrinePairs(){
        DoctrinePairs pairs = new();
        var enumValues = Enum.GetValues(typeof(SermonCategory));
        foreach(var enumVal in enumValues){
            SermonCategory category = (SermonCategory)enumVal;
            List<Tuple<DoctrineUpgradeSystem.DoctrineType, DoctrineUpgradeSystem.DoctrineType>> innerPair = new();
            if(DoctrineUpgradeSystem.GetSermonReward(category, 1, true) != DoctrineUpgradeSystem.DoctrineType.None){
                for(int level = 1; level <= 4; level++){
                    innerPair.Add(Tuple.Create(
                        DoctrineUpgradeSystem.GetSermonReward((SermonCategory)enumVal, level, true),
                        DoctrineUpgradeSystem.GetSermonReward((SermonCategory)enumVal, level, false)
                    ));                
                }
            }
            pairs[category] = innerPair;
        }
        return pairs;
    }

    public static List<Tuple<UpgradeSystem.Type, UpgradeSystem.Type>> GetRitualPairs(){
        List<Tuple<UpgradeSystem.Type, UpgradeSystem.Type>> pairs = new();
        for(int i = 0; i < UpgradeSystem.SecondaryRitualPairs.Length; i+=2){
            pairs.Add(new(UpgradeSystem.SecondaryRitualPairs[i], UpgradeSystem.SecondaryRitualPairs[i+1]));
        }
        return pairs;
    }

    public static void RenameCult(Action<string> onNameConfirmed = null)
    {
        try {
            Traverse.Create(typeof(CheatConsole)).Method("RenameCult").GetValue();
        } catch(Exception e){
            UnityEngine.Debug.LogWarning($"Failed to rename cult: {e.Message}");
            PlayNotification("Failed to open rename dialog!");
        }
    }

    // -- Faith Management ------------------------------------------------------

    public static float CalculateCurrentFaith()
    {
        float totalFaith = 0f;
        var thoughtsList = Traverse.Create(typeof(CultFaithManager)).Field("Thoughts").GetValue<List<ThoughtData>>();
        if(thoughtsList == null) return totalFaith;
        
        foreach (ThoughtData thoughtData in thoughtsList)
        {
            int index = 0;
            float thoughtFaith = -1;
            while (index <= thoughtData.Quantity)
            {
                if(index == 0)
                {
                    thoughtFaith += thoughtData.Modifier;
                } else
                {
                    thoughtFaith += thoughtData.StackModifier;
                }
                index += 1;
            }
            totalFaith += thoughtFaith;
        }
        return totalFaith;
    }

    public static float GetCurrentFaith()
    {
        return CultFaithManager.CurrentFaith;
    }

    public static void ModifyFaith(float value, string notifMessage, bool shouldNotify = true)
    {
        NotificationBase.Flair flair = NotificationBase.Flair.Positive;
        float currentFaithValue = CultUtils.GetCurrentFaith();
        if (currentFaithValue > value)
        {
            flair = NotificationBase.Flair.Negative;
        }
        float currentFollowerFaith = CalculateCurrentFaith();
        float staticFaith = currentFaithValue < value ? value - currentFollowerFaith : currentFollowerFaith - value;

        NotificationData data = shouldNotify ? new NotificationData(notifMessage, 0f, -1, flair, new string[] { }) : null;

        CultFaithManager.StaticFaith = staticFaith;
        CultFaithManager.Instance.BarController.SetBarSize(value / 85f, true, true, data);
    }
}

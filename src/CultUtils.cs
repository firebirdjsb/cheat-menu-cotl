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

using DoctrinePairs =  Dictionary<SermonCategory, List<Tuple<DoctrineUpgradeSystem.DoctrineType, DoctrineUpgradeSystem.DoctrineType>>>;

internal class CultUtils {
    public static bool IsInGame(){
        return SaveAndLoad.Loaded;
    }

    public static void GiveDocterineStone(){
        // Used to make sure the user can declare doctrines before even doing a run
        DataManager.Instance.FirstDoctrineStone = true;
        DataManager.Instance.ForceDoctrineStones = true;
        DataManager.Instance.CompletedDoctrineStones += 1;

        //Needed to update the UI
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

    public static void CompleteAllQuests(){
        if(DataManager.Instance.Objectives.Count > 0)
        {
            foreach(var objective in DataManager.Instance.Objectives.ToArray())
            {
                CompleteObjective(objective);
            }
        }
    }

    public static void ClearAllDocterines(){
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
        PlayNotification("Cleared all docterines!");
    }

    //Avoids removing special and player related upgrades
    public static void RemoveDocterineUpgrades(){
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

    // These are the 'public' sermon categories that are visible to the user
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
        RemoveDocterineUpgrades();

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

    public static void PlayNotification(string message){
        if(NotificationCentre.Instance){
            NotificationCentre.Instance.PlayGenericNotification(message);
        }
    }

    public static void ClearBaseTrees(){
        try {
            foreach(var tree in StructureManager.GetAllStructuresOfType(FollowerLocation.Base, StructureBrain.TYPES.TREE)){
                tree.Remove();
            }
            PlayNotification("Trees cleared!");
        } catch(Exception e){
            UnityEngine.Debug.LogWarning($"Failed to clear trees: {e.Message}");
            PlayNotification("Failed to clear trees!");
        }
    }

    public static void ClearBaseRubble(){
        try {
            int count = 0;
            // Clear all rubble types: RUBBLE, RUBBLE_BIG, ROCK, BLOOD_STONE
            var rubbleTypes = new[] { 
                StructureBrain.TYPES.RUBBLE,
                StructureBrain.TYPES.RUBBLE_BIG,
                StructureBrain.TYPES.ROCK,
                StructureBrain.TYPES.BLOOD_STONE
            };

            foreach(var rubbleType in rubbleTypes){
                foreach(var rubble in StructureManager.GetAllStructuresOfType(FollowerLocation.Base, rubbleType)){
                    rubble.Remove();
                    count++;
                }
            }
            PlayNotification($"Rubble cleared! ({count} items)");
        } catch(Exception e){
            UnityEngine.Debug.LogWarning($"Failed to clear rubble: {e.Message}");
            PlayNotification("Failed to clear rubble!");
        }
    }

    private static bool IsLandscapeType(string typeName){
        return typeName.Contains("GRASS") || typeName.Contains("WEED") || typeName.Contains("BUSH") 
            || typeName.Contains("SHRUB") || typeName.Contains("FERN") || typeName.Contains("PLANT") 
            || typeName.Contains("FOLIAGE") || typeName.Contains("STUMP") || typeName.Contains("SAPLING")
            || typeName.Contains("DECORATION_ENVIRONMENT");
    }

    public static void ClearBaseGrass(){
        try {
            int count = 0;
            foreach(var locValue in Enum.GetValues(typeof(FollowerLocation))){
                FollowerLocation loc = (FollowerLocation)locValue;
                foreach(var brainType in Enum.GetValues(typeof(StructureBrain.TYPES))){
                    string typeName = brainType.ToString();
                    if(IsLandscapeType(typeName)){
                        try {
                            var structures = StructureManager.GetAllStructuresOfType(loc, (StructureBrain.TYPES)brainType);
                            foreach(var structure in structures){
                                structure.Remove();
                                count++;
                            }
                        } catch { }
                    }
                }
            }
            // Fallback: iterate all structure brain types by name for any remaining
            foreach(var brainType in Enum.GetValues(typeof(StructureBrain.TYPES))){
                string typeName = brainType.ToString();
                if(IsLandscapeType(typeName)){
                    try {
                        var remaining = StructureManager.GetAllStructuresOfType((StructureBrain.TYPES)brainType);
                        foreach(var structure in remaining){
                            structure.Remove();
                            count++;
                        }
                    } catch { }
                }
            }
            PlayNotification($"Landscape cleared! ({count} items)");
        } catch(Exception e){
            UnityEngine.Debug.LogWarning($"Failed to clear landscape: {e.Message}");
            PlayNotification("Failed to clear landscape!");
        }
    }

    public static void ClearVomit(){
        foreach(var vomit in StructureManager.GetAllStructuresOfType(FollowerLocation.Base, StructureBrain.TYPES.VOMIT)){
            vomit.Remove();
        }
        PlayNotification("Vomit cleared!");
    }

    public static async void ClearPoop(){
        int poopCount = 0;
        // Clear regular poop
        foreach(var poop in StructureManager.GetAllStructuresOfType(FollowerLocation.Base, StructureBrain.TYPES.POOP)){
            poop.Remove();
            poopCount++;
        }
        // Clear giant poop piles and any other poop-related structure types
        foreach(var brainType in Enum.GetValues(typeof(StructureBrain.TYPES))){
            string typeName = brainType.ToString();
            if(typeName.Contains("POOP") && (StructureBrain.TYPES)brainType != StructureBrain.TYPES.POOP){
                try {
                    var poopStructures = StructureManager.GetAllStructuresOfType(FollowerLocation.Base, (StructureBrain.TYPES)brainType);
                    foreach(var ps in poopStructures){
                        ps.Remove();
                        poopCount++;
                    }
                } catch { }
            }
        }
        // Clear nursery/daycare poop via Interaction_Daycare (the game calls them "Daycares")
        try {
            foreach(var daycare in Interaction_Daycare.Daycares){
                if(daycare == null || daycare.Structure == null) continue;
                var inventory = daycare.Structure.Inventory;
                if(inventory != null && inventory.Count > 0){
                    foreach(var item in inventory){
                        if(item.type == (int)InventoryItem.ITEM_TYPE.POOP && item.quantity > 0){
                            AddInventoryItem(InventoryItem.ITEM_TYPE.POOP, item.quantity);
                            poopCount++;
                        }
                    }
                    inventory.RemoveAll(i => i.type == (int)InventoryItem.ITEM_TYPE.POOP);
                    // Update the visual poop states on the daycare
                    daycare.UpdatePoopStates();
                }
            }
        } catch(Exception e){
            UnityEngine.Debug.LogWarning($"[CheatMenu] Failed to clear nursery poop: {e.Message}");
        }
        ClearJanitorStations();
        await AsyncHelper.WaitSeconds(1);
        foreach(var pickup in PickUp.PickUps){
            if(pickup.type == InventoryItem.ITEM_TYPE.POOP){
                pickup.PickMeUp();
            }
        }
        PlayNotification($"Poop cleared! ({poopCount} sources)");
    }

    public static void ClearJanitorStations(){
        try {
            int stationCount = 0;
            int totalSouls = 0;

            // Find all janitor station brains and collect their SoulCount
            var janitorStations = StructureManager.GetAllStructuresOfType<Structures_JanitorStation>();
            foreach(var janitorStation in janitorStations){
                if(janitorStation != null && janitorStation.SoulCount > 0){
                    totalSouls += janitorStation.SoulCount;
                    stationCount++;
                }
            }

            // Collect the souls and grant ChoreXP via the game's proper method
            if(totalSouls > 0 && PlayerFarming.Instance != null){
                // Reset SoulCount on all stations
                foreach(var janitorStation in janitorStations){
                    if(janitorStation != null){
                        janitorStation.SoulCount = 0;
                    }
                }

                // Force the scene JanitorStation objects to refresh their gauge naturally
                // by resetting previousSoulCount so Update() detects the change and calls
                // gauge.SetPosition() itself, preserving the gauge's GameObject state
                foreach(var sceneStation in JanitorStation.JanitorStations){
                    if(sceneStation != null){
                        Traverse.Create(sceneStation).Field("previousSoulCount").SetValue(-1);
                    }
                }

                // Grant ChoreXP through the game's proper system
                PlayerFarming.Instance.playerChoreXPBarController.AddChoreXP(PlayerFarming.Instance, (float)totalSouls);
            }

            if(stationCount > 0 || totalSouls > 0){
                PlayNotification($"Janitor stations collected! ({totalSouls} XP from {stationCount} stations)");
            } else {
                PlayNotification("No janitor stations with XP found!");
            }
        } catch(Exception e){
            UnityEngine.Debug.LogWarning($"[CheatMenu] Failed to clear janitor stations: {e.Message}");
        }
    }

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

    public static void ClearOuthouses(){
        List<StructureBrain> outhouse1 = StructureManager.GetAllStructuresOfType(FollowerLocation.Base, StructureBrain.TYPES.OUTHOUSE);
        List<StructureBrain> outhouse2 = StructureManager.GetAllStructuresOfType(FollowerLocation.Base, StructureBrain.TYPES.OUTHOUSE_2);
        StructureBrain[] outhouses = CheatUtils.Concat(outhouse1.ToArray(), outhouse2.ToArray());
        int totalPoop = 0;
        foreach(var outhouse in outhouses){
            if (outhouse is Structures_Outhouse outhouseStructure)
            {
                int poopCount = outhouseStructure.GetPoopCount();
                if(poopCount > 0){
                    AddInventoryItem(InventoryItem.ITEM_TYPE.POOP, poopCount);
                    totalPoop += poopCount;
                }
                outhouseStructure.Data.Inventory.Clear();
            }
        }

        // Update the visual gauges on all outhouse interactions in the scene
        try {
            foreach(var outhouseInteraction in Interaction_Outhouse.Outhouses){
                if(outhouseInteraction != null){
                    outhouseInteraction.UpdateGauge();
                }
            }
        } catch(Exception e){
            UnityEngine.Debug.LogWarning($"[CheatMenu] Failed to update outhouse gauges: {e.Message}");
        }

        PlayNotification(totalPoop > 0 ? $"Outhouses cleared! ({totalPoop} poop)" : "Outhouses already clean!");
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

    public static void AddInventoryItem(InventoryItem.ITEM_TYPE type, int amount){
        try {
            Inventory.AddItem((int)type, amount, false);
        } catch(Exception e){
            UnityEngine.Debug.LogWarning($"[CheatMenu] AddInventoryItem fallback for {type}: {e.Message}");
            try {
                Inventory.AddItem(type, amount, false);
            } catch(Exception e2){
                UnityEngine.Debug.LogWarning($"[CheatMenu] AddInventoryItem failed for {type}: {e2.Message}");
            }
        }
    }

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
                    // Continue anyway - follower is still turned young, just outfit might not update
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

    public static void SpawnFollower(FollowerRole role)
    {
        try {
            Follower follower = FollowerManager.CreateNewFollower(PlayerFarming.Location, PlayerFarming.Instance.transform.position, false);
            
            if(follower == null || follower.Brain == null || follower.Brain.Info == null){
                UnityEngine.Debug.LogWarning("[CheatMenu] Failed to spawn follower - null reference");
                PlayNotification("Failed to spawn follower!");
                return;
            }

            follower.Brain.Info.FollowerRole = role;
            
            // Ensure follower has valid outfit data before setting
            // Only set outfit if the follower instance is fully loaded
            if(follower.Outfit != null && follower.gameObject != null && follower.gameObject.activeInHierarchy){
                follower.Brain.Info.Outfit = FollowerOutfitType.Follower;
                
                // Use safe outfit setting with null check
                try {
                    follower.SetOutfit(FollowerOutfitType.Follower, false, Thought.None);
                } catch(Exception e){
                    UnityEngine.Debug.LogWarning($"[CheatMenu] Failed to set follower outfit: {e.Message}");
                    // Continue anyway, follower will use default outfit
                }
            }

            if (role == FollowerRole.Worker)
            {
                follower.Brain.Info.WorkerPriority = WorkerPriority.Rubble;
                follower.Brain.Stats.WorkerBeenGivenOrders = true;
                
                // Only call CheckChangeState if the brain is fully initialized
                try {
                    if(follower.Brain != null && follower.Brain.Info != null){
                        follower.Brain.CheckChangeState();
                    }
                } catch(Exception e){
                    UnityEngine.Debug.LogWarning($"[CheatMenu] Failed to change worker state: {e.Message}");
                    // Continue anyway
                }
            }

            FollowerInfo newFollowerInfo = GetFollowerInfo(follower);
            if(newFollowerInfo != null){
                SetFollowerIllness(newFollowerInfo, 0f);
                SetFollowerHunger(newFollowerInfo, 100f);
            }
        } catch(Exception e){
            UnityEngine.Debug.LogWarning($"[CheatMenu] SpawnFollower error: {e.Message}");
            PlayNotification("Failed to spawn follower!");
        }
    }

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

    public static void ForceGrowAllAnimals(){
        try {
            int count = 0;
            var animals = AnimalData.GetAnimals();
            foreach(var animal in animals){
                if(animal.Age < 2){
                    animal.Age = 2;
                }
                animal.GrowthStage = 0;
                animal.WorkedReady = true;
                animal.WorkedToday = false;
                animal.Satiation = Interaction_Ranchable.MAX_HUNGER;
                Interaction_Ranchable ranchable = Interaction_Ranch.GetAnimal(animal);
                if(ranchable != null){
                    ranchable.UpdateSkin();
                }
                count++;
            }
            PlayNotification(count > 0 ? $"Force grew {count} animal(s)!" : "No animals to grow!");
        } catch(Exception e){
            UnityEngine.Debug.LogWarning($"[CheatMenu] ForceGrowAllAnimals error: {e.Message}");
            PlayNotification("Failed to force grow animals!");
        }
    }

    public static Interaction_WolfBase FriendlyWolf = null;
    public static bool WolfDungeonCombat = true;
    private static string s_wolfCurrentAnim = "";
    private static bool s_wolfIsRunning = false;
    private static Vector3 s_wolfVelocity = Vector3.zero;
    private static float s_wolfAttackCooldown = 0f;
    private static bool s_wolfPetting = false;

    private static bool s_wolfShouldExist = false;
    private static bool s_wolfRespawning = false;
    private static float s_wolfAnimHoldTimer = 0f;
    private const float WOLF_ANIM_HOLD_MIN = 0.15f;
    private static string s_wolfAttackAnimName = null;
    private static float s_wolfCombatTransitionTimer = 0f;
    private const float WOLF_COMBAT_TRANSITION_MIN = 0.7f;

    private const float WOLF_FOLLOW_SPEED = 4.5f;
    private const float WOLF_TELEPORT_DIST = 8f;
    private const float WOLF_START_FOLLOW_DIST = 3.5f;
    private const float WOLF_STOP_FOLLOW_DIST = 1.8f;
    private const float WOLF_SMOOTH_TIME = 0.35f;
    private const float WOLF_ATTACK_RANGE = 1.5f;
    private const float WOLF_DETECT_RANGE = 6f;
    private const float WOLF_ATTACK_COOLDOWN = 1.8f;
    private const float WOLF_ATTACK_DAMAGE = 2f;
    private static Health s_wolfTargetEnemy = null;

    public static void SpawnFriendlyWolf(){
        SpawnFriendlyWolfInternal(true);
    }

    private static void SpawnFriendlyWolfInternal(bool userInitiated){
        try {
            if(PlayerFarming.Instance == null){
                if(userInitiated) PlayNotification("Must be in game!");
                return;
            }

            // Only allow 1 friendly wolf at a time
            if(FriendlyWolf != null){
                if(userInitiated) PlayNotification("You already have a friendly wolf!");
                return;
            }

            Vector3 spawnPos = PlayerFarming.Instance.transform.position;

            Interaction_WolfBase.WolfTarget = 1;
            Interaction_WolfBase.WolfCount = 0;
            Interaction_WolfBase.WolfFled = 0;
            Interaction_WolfBase.WolfDied = 0;

            s_wolfRespawning = true;
            Interaction_WolfBase.SpawnWolf(spawnPos, null, false, (Interaction_WolfBase wolf) => {
                s_wolfRespawning = false;
                if(wolf != null){
                    wolf.CurrentState = Interaction_WolfBase.State.Animating;
                    wolf.SecondaryInteractable = false;
                    FriendlyWolf = wolf;
                    s_wolfCurrentAnim = "";
                    s_wolfIsRunning = false;
                    s_wolfVelocity = Vector3.zero;
                    s_wolfAttackCooldown = 0f;
                    s_wolfPetting = false;
                    s_wolfAnimHoldTimer = 0f;
                    s_wolfShouldExist = true;
                    s_wolfAttackAnimName = null;
                    s_wolfCombatTransitionTimer = 0f;

                    // Discover the correct attack animation name from the wolf's skeleton
                    DiscoverWolfAttackAnimation(wolf);

                    Interaction_WolfBase.ResetWolvesEnounterData();
                    UnityEngine.Debug.Log("[CheatMenu] Friendly wolf spawned and following player!");
                }
            });

            if(userInitiated) PlayNotification("Friendly wolf spawned!");
        } catch(Exception e){
            s_wolfRespawning = false;
            UnityEngine.Debug.LogWarning($"[CheatMenu] SpawnFriendlyWolf error: {e.Message}");
            if(userInitiated) PlayNotification("Failed to spawn friendly wolf!");
        }
    }

    public static void DismissFriendlyWolf(){
        try {
            s_wolfShouldExist = false;
            s_wolfRespawning = false;
            if(FriendlyWolf != null){
                UnityEngine.Object.Destroy(FriendlyWolf.gameObject);
                FriendlyWolf = null;
                s_wolfPetting = false;
                PlayNotification("Friendly wolf dismissed!");
            } else {
                var wolves = new List<Interaction_WolfBase>(Interaction_WolfBase.Wolfs);
                int count = 0;
                foreach(var wolf in wolves){
                    if(wolf != null){
                        UnityEngine.Object.Destroy(wolf.gameObject);
                        count++;
                    }
                }
                Interaction_WolfBase.Wolfs.Clear();
                Interaction_WolfBase.ResetWolvesEnounterData();
                PlayNotification(count > 0 ? $"Dismissed {count} wolf/wolves!" : "No wolves to dismiss!");
            }
        } catch(Exception e){
            UnityEngine.Debug.LogWarning($"[CheatMenu] DismissFriendlyWolf error: {e.Message}");
            PlayNotification("Failed to dismiss wolf!");
        }
    }

    public static void PetFriendlyWolf(){
        try {
            if(FriendlyWolf == null){
                PlayNotification("No friendly wolf to pet!");
                return;
            }
            if(PlayerFarming.Instance == null){
                PlayNotification("Must be in game!");
                return;
            }
            if(s_wolfPetting){
                return;
            }
            GameManager.GetInstance().StartCoroutine(PetWolfCoroutine());
        } catch(Exception e){
            UnityEngine.Debug.LogWarning($"[CheatMenu] PetFriendlyWolf error: {e.Message}");
            PlayNotification("Failed to pet wolf!");
        }
    }

    private static IEnumerator PetWolfCoroutine(){
        s_wolfPetting = true;
        var player = PlayerFarming.Instance;
        var wolf = FriendlyWolf;

        if(player == null || wolf == null){
            s_wolfPetting = false;
            yield break;
        }

        // Calculate a position in front of the wolf for the player to walk to
        float wolfToPlayerAngle = Utils.GetAngle(wolf.transform.position, player.transform.position);
        float petDist = 1.2f;
        Vector3 petTarget = wolf.transform.position + new Vector3(
            Mathf.Cos(wolfToPlayerAngle * Mathf.Deg2Rad) * petDist,
            Mathf.Sin(wolfToPlayerAngle * Mathf.Deg2Rad) * petDist, 0f);

        // Make the player run toward the pet position
        float moveSpeed = 6f;
        float timeout = 3f;
        float elapsed = 0f;
        float arrivalThreshold = 0.3f;

        while(elapsed < timeout){
            if(player == null || wolf == null || !(bool)(UnityEngine.Object)wolf){
                s_wolfPetting = false;
                yield break;
            }

            Vector3 currentPos = player.transform.position;
            float remaining = Vector3.Distance(currentPos, petTarget);
            if(remaining <= arrivalThreshold) break;

            // Face and move toward the target
            float moveAngle = Utils.GetAngle(currentPos, petTarget);
            player.state.facingAngle = moveAngle;
            player.state.CURRENT_STATE = StateMachine.State.Moving;

            Vector3 dir = (petTarget - currentPos).normalized;
            player.transform.position = currentPos + dir * Mathf.Min(moveSpeed * Time.deltaTime, remaining);

            elapsed += Time.deltaTime;
            yield return null;
        }

        if(player == null || wolf == null || !(bool)(UnityEngine.Object)wolf){
            s_wolfPetting = false;
            yield break;
        }

        // Face the wolf
        float angle = Utils.GetAngle(player.transform.position, wolf.transform.position);
        player.state.facingAngle = angle;

        // Play pet animation on the player
        player.state.CURRENT_STATE = StateMachine.State.CustomAnimation;
        player.simpleSpineAnimator.Animate("pet-dog", 0, false);

        // Make the wolf idle and face the player
        var stateMachine = Traverse.Create(wolf).Field("stateMachine").GetValue<StateMachine>();
        if(stateMachine != null){
            float wolfAngle = Utils.GetAngle(wolf.transform.position, player.transform.position);
            stateMachine.facingAngle = wolfAngle;
            stateMachine.LookAngle = wolfAngle;
        }
        SetWolfAnimation(wolf.Spine, "idle", true);

        try {
            AudioManager.Instance.PlayOneShot("event:/dlc/animal/shared/calm", wolf.transform.position);
        } catch { }

        yield return new WaitForSeconds(1f);

        // Emit hearts
        try {
            BiomeConstants.Instance.EmitHeartPickUpVFX(wolf.transform.position, 0f, "red", "burst_big", false);
            AudioManager.Instance.PlayOneShot("event:/followers/love_hearts", wolf.transform.position);
        } catch { }

        CameraManager.instance.ShakeCameraForDuration(0.3f, 0.4f, 0.3f, true);

        yield return new WaitForSeconds(0.8f);

        player.state.CURRENT_STATE = StateMachine.State.Idle;
        s_wolfPetting = false;
        PlayNotification("Good wolf!");
    }

    /// <summary>
    /// Called from the Update loop to check if the friendly wolf needs respawning
    /// after a scene change or dungeon transition.
    /// </summary>
    [Update]
    public static void UpdateFriendlyWolf(){
        if(!s_wolfShouldExist) return;
        if(s_wolfRespawning) return;
        if(PlayerFarming.Instance == null) return;

        // Check if the wolf reference is stale (destroyed by scene change)
        // Use Unity's implicit bool operator to detect destroyed-but-not-null objects
        if(FriendlyWolf == null || !(bool)(UnityEngine.Object)FriendlyWolf){
            FriendlyWolf = null;
            s_wolfPetting = false;
            s_wolfTargetEnemy = null;
            s_wolfCurrentAnim = "";
            s_wolfIsRunning = false;
            s_wolfVelocity = Vector3.zero;
            s_wolfAttackCooldown = 0f;
            s_wolfAnimHoldTimer = 0f;
            s_wolfAttackAnimName = null;
            s_wolfCombatTransitionTimer = 0f;
            UnityEngine.Debug.Log("[CheatMenu] Friendly wolf lost (scene change) - respawning...");
            SpawnFriendlyWolfInternal(false);
        }
    }

    /// <summary>
    /// Called from GlobalPatches Harmony prefix on Interaction_WolfBase.Update.
    /// Uses smooth movement with hysteresis to prevent animation flickering.
    /// In dungeons, attacks nearby enemies.
    /// </summary>
    public static bool HandleFriendlyWolfUpdate(Interaction_WolfBase wolf){
        if(wolf != FriendlyWolf || FriendlyWolf == null) return true;
        if(PlayerFarming.Instance == null) return false;
        if(s_wolfPetting) return false;

        try {
            Vector3 playerPos = PlayerFarming.Instance.transform.position;
            Vector3 wolfPos = wolf.transform.position;
            float distance = Vector3.Distance(wolfPos, playerPos);

            var spineAnim = wolf.Spine;
            var stateMachine = Traverse.Create(wolf).Field("stateMachine").GetValue<StateMachine>();

            var unitObject = wolf.UnitObject;
            if(unitObject != null){
                unitObject.ClearPaths();
                unitObject.UsePathing = false;
            }

            // Dungeon combat: detect and chase enemies
            bool inDungeon = GameManager.IsDungeon(PlayerFarming.Location);
            bool wolfInCombat = false;

            if(inDungeon && WolfDungeonCombat){
                s_wolfAttackCooldown -= Time.deltaTime;
                s_wolfCombatTransitionTimer -= Time.deltaTime;

                // Clear dead/invalid target using Unity's implicit bool to catch destroyed objects
                if(s_wolfTargetEnemy != null){
                    bool targetInvalid = false;
                    try {
                        targetInvalid = !(bool)(UnityEngine.Object)s_wolfTargetEnemy
                            || s_wolfTargetEnemy.HP <= 0f
                            || !s_wolfTargetEnemy.enabled;
                    } catch {
                        targetInvalid = true;
                    }
                    if(targetInvalid){
                        s_wolfTargetEnemy = null;
                        // Force a brief run animation before attacking the next enemy
                        s_wolfCombatTransitionTimer = WOLF_COMBAT_TRANSITION_MIN;
                    }
                }

                // Find a new target if we don't have one
                if(s_wolfTargetEnemy == null){
                    s_wolfTargetEnemy = FindClosestEnemy(wolfPos, WOLF_DETECT_RANGE);
                }

                if(s_wolfTargetEnemy != null){
                    wolfInCombat = true;
                    Vector3 enemyPos = s_wolfTargetEnemy.transform.position;
                    float enemyDist = Vector3.Distance(wolfPos, enemyPos);

                    // Face the enemy
                    if(stateMachine != null){
                        float faceAngle = Utils.GetAngle(wolfPos, enemyPos);
                        stateMachine.facingAngle = faceAngle;
                        stateMachine.LookAngle = faceAngle;
                    }

                    // During combat transition, always show run animation before attacking next enemy
                    if(s_wolfCombatTransitionTimer > 0f){
                        wolf.transform.position = Vector3.SmoothDamp(
                            wolfPos, enemyPos, ref s_wolfVelocity, WOLF_SMOOTH_TIME * 0.7f, WOLF_FOLLOW_SPEED * 1.3f, Time.deltaTime
                        );
                        SetWolfAnimation(spineAnim, "run", false);
                    } else {
                        string atkAnim = s_wolfAttackAnimName ?? "idle";

                        // Use hysteresis for attack range to prevent rapid in/out of range flickering
                        bool wasAttacking = s_wolfCurrentAnim == atkAnim || s_wolfCurrentAnim == "idle";
                        float effectiveRange = wasAttacking ? WOLF_ATTACK_RANGE * 1.5f : WOLF_ATTACK_RANGE;

                        if(enemyDist <= effectiveRange){
                            // In melee range - attack
                            if(s_wolfAttackCooldown <= 0f){
                                s_wolfTargetEnemy.DealDamage(WOLF_ATTACK_DAMAGE, wolf.gameObject, wolfPos, false, Health.AttackTypes.Melee, false, (Health.AttackFlags)0);
                                s_wolfAttackCooldown = WOLF_ATTACK_COOLDOWN;

                                try {
                                    AudioManager.Instance.PlayOneShot("event:/dlc/env/dog/dog_basic_attack_bite", wolf.transform.position);
                                } catch { }

                                SetWolfAnimation(spineAnim, atkAnim, true);
                            } else {
                                // Keep attack animation playing during cooldown, only idle near end
                                if(s_wolfAttackCooldown < WOLF_ATTACK_COOLDOWN * 0.3f){
                                    SetWolfAnimation(spineAnim, "idle", false);
                                }
                            }
                            // Gently drift toward the enemy if slightly out of true attack range
                            if(enemyDist > WOLF_ATTACK_RANGE){
                                wolf.transform.position = Vector3.SmoothDamp(
                                    wolfPos, enemyPos, ref s_wolfVelocity, WOLF_SMOOTH_TIME, WOLF_FOLLOW_SPEED * 0.5f, Time.deltaTime
                                );
                            } else {
                                s_wolfVelocity = Vector3.zero;
                            }
                        } else {
                            // Chase the enemy
                            wolf.transform.position = Vector3.SmoothDamp(
                                wolfPos, enemyPos, ref s_wolfVelocity, WOLF_SMOOTH_TIME * 0.7f, WOLF_FOLLOW_SPEED * 1.3f, Time.deltaTime
                            );
                            SetWolfAnimation(spineAnim, "run", false);
                        }
                    }
                }
            } else {
                s_wolfTargetEnemy = null;
            }

            // Movement: follow player (only when not in combat)
            if(!wolfInCombat){
                if(distance > WOLF_TELEPORT_DIST){
                    // Teleport behind the player based on their facing direction
                    Vector3 behindPos = playerPos;
                    try {
                        float facingAngle = PlayerFarming.Instance.state.facingAngle;
                        float behindAngle = (facingAngle + 180f) * Mathf.Deg2Rad;
                        behindPos += new Vector3(Mathf.Cos(behindAngle) * 2f, Mathf.Sin(behindAngle) * 2f, 0f);
                    } catch {
                        behindPos += new Vector3(-2f, 0f, 0f);
                    }
                    wolf.transform.position = behindPos;
                    s_wolfVelocity = Vector3.zero;
                    s_wolfIsRunning = false;
                    SetWolfAnimation(spineAnim, "idle", true);
                }
                else {
                    // Hysteresis: start running at START_FOLLOW_DIST, stop at STOP_FOLLOW_DIST
                    if(!s_wolfIsRunning && distance > WOLF_START_FOLLOW_DIST){
                        s_wolfIsRunning = true;
                    }
                    else if(s_wolfIsRunning && distance < WOLF_STOP_FOLLOW_DIST){
                        s_wolfIsRunning = false;
                        s_wolfVelocity = Vector3.zero;
                    }

                    if(s_wolfIsRunning){
                        Vector3 targetPos = playerPos;
                        wolf.transform.position = Vector3.SmoothDamp(
                            wolfPos, targetPos, ref s_wolfVelocity, WOLF_SMOOTH_TIME, WOLF_FOLLOW_SPEED, Time.deltaTime
                        );

                        if(stateMachine != null){
                            float faceAngle = Utils.GetAngle(wolfPos, playerPos);
                            stateMachine.facingAngle = faceAngle;
                            stateMachine.LookAngle = faceAngle;
                        }

                        SetWolfAnimation(spineAnim, "run", false);
                    } else {
                        if(stateMachine != null){
                            float faceAngle = Utils.GetAngle(wolfPos, playerPos);
                            stateMachine.facingAngle = faceAngle;
                            stateMachine.LookAngle = faceAngle;
                        }

                        SetWolfAnimation(spineAnim, "idle", false);
                    }
                }
            }
        } catch(Exception e){
            UnityEngine.Debug.LogWarning($"[CheatMenu] FriendlyWolf update error: {e.Message}");
        }

        return false;
    }

    private static Health FindClosestEnemy(Vector3 position, float maxRange){
        Health closest = null;
        float closestDist = maxRange;
        try {
            foreach(var enemy in Health.team2){
                if(enemy == null || !(bool)(UnityEngine.Object)enemy) continue;
                try {
                    if(!enemy.enabled || enemy.HP <= 0f) continue;
                    float dist = Vector3.Distance(position, enemy.transform.position);
                    if(dist < closestDist){
                        closestDist = dist;
                        closest = enemy;
                    }
                } catch { continue; }
            }
        } catch { }
        return closest;
    }

    private static void SetWolfAnimation(SkeletonAnimation spineAnim, string animName, bool force){
        if(spineAnim == null) return;
        try {
            s_wolfAnimHoldTimer -= Time.deltaTime;
            string atkAnim = s_wolfAttackAnimName ?? "idle";
            bool isAttackAnim = animName == atkAnim;
            if(force || (s_wolfCurrentAnim != animName && s_wolfAnimHoldTimer <= 0f)){
                spineAnim.AnimationState.SetAnimation(0, animName, !isAttackAnim);
                s_wolfCurrentAnim = animName;
                // Hold attack animation longer to prevent stuttering during combat transitions
                if(isAttackAnim){
                    s_wolfAnimHoldTimer = 0.35f;
                } else {
                    s_wolfAnimHoldTimer = WOLF_ANIM_HOLD_MIN;
                }
            }
        } catch(Exception e){
            UnityEngine.Debug.LogWarning($"[CheatMenu] SetWolfAnimation error ({animName}): {e.Message}");
        }
    }

    /// <summary>
    /// Discovers the correct attack animation name from the wolf's Spine skeleton data.
    /// From the assembly dump (Interaction_WolfBase.cs), the wolf has these animations:
    /// run, charge_attack, idle, howl, knockback, knockback-reset,
    /// jump_anticipation, jump, jump_land, trapped.
    /// There is no dedicated "attack" or "bite" animation - the base game wolf
    /// attacks by moving close and calling Damage() with only a sound effect.
    /// We use "charge_attack" as the attack visual since it's the most aggressive.
    /// </summary>
    private static void DiscoverWolfAttackAnimation(Interaction_WolfBase wolf){
        try {
            var spineAnim = wolf.Spine;
            if(spineAnim == null || spineAnim.skeleton == null || spineAnim.skeleton.Data == null){
                s_wolfAttackAnimName = "charge_attack";
                UnityEngine.Debug.LogWarning("[CheatMenu] Wolf skeleton not ready, defaulting to charge_attack");
                return;
            }

            var skeletonData = spineAnim.skeleton.Data;
            var animations = skeletonData.Animations;
            List<string> animNames = new List<string>();
            foreach(var anim in animations){
                animNames.Add(anim.Name);
            }

            UnityEngine.Debug.Log($"[CheatMenu] Wolf available animations: {string.Join(", ", animNames)}");

            // From the assembly dump, the wolf's aggressive animation is "charge_attack"
            // Priority: charge_attack > howl > idle
            if(animNames.Contains("charge_attack")){
                s_wolfAttackAnimName = "charge_attack";
            } else if(animNames.Contains("howl")){
                s_wolfAttackAnimName = "howl";
            } else {
                s_wolfAttackAnimName = "idle";
            }

            UnityEngine.Debug.Log($"[CheatMenu] Wolf attack animation set to: {s_wolfAttackAnimName}");
        } catch(Exception e){
            s_wolfAttackAnimName = "charge_attack";
            UnityEngine.Debug.LogWarning($"[CheatMenu] DiscoverWolfAttackAnimation error: {e.Message}");
        }
    }

    public static void AscendAllAnimals(){
        try {
            int count = 0;
            var animals = new List<StructuresData.Ranchable_Animal>(AnimalData.GetAnimals());
            foreach(var animal in animals){
                if(animal == null) continue;

                Interaction_Ranchable ranchable = Interaction_Ranch.GetAnimal(animal);

                // If the ranchable is in the scene, use the game's built-in AscendIE coroutine
                if(ranchable != null && ranchable.gameObject != null && ranchable.gameObject.activeInHierarchy){
                    try {
                        ranchable.StartCoroutine(AscendAnimalCoroutine(ranchable, animal));
                    } catch(Exception e){
                        UnityEngine.Debug.LogWarning($"[CheatMenu] Failed to start ascend coroutine: {e.Message}");
                        // Fallback: remove immediately
                        RemoveAnimalImmediate(ranchable, animal);
                    }
                } else {
                    // Animal not in scene, just collect resources and remove data
                    CollectAnimalResources(animal, Vector3.zero, false);
                    RemoveAnimalData(ranchable, animal);
                }

                count++;
            }
            PlayNotification(count > 0 ? $"Ascended {count} animal(s)!" : "No animals to ascend!");
        } catch(Exception e){
            UnityEngine.Debug.LogWarning($"[CheatMenu] AscendAllAnimals error: {e.Message}");
            PlayNotification("Failed to ascend animals!");
        }
    }

    private static System.Collections.IEnumerator AscendAnimalCoroutine(Interaction_Ranchable ranchable, StructuresData.Ranchable_Animal animal){
        // Mark as being ascended so AI doesn't interfere
        ranchable.BeingAscended = true;
        ranchable.ReservedByPlayer = true;
        animal.State = Interaction_Ranchable.State.Animating;

        // Play the ascend animation on the animal's spine
        string animalName = GetAnimationAnimalName(animal.Type);
        string ascendAnim = "ascend-" + animalName;
        var spine = Traverse.Create(ranchable).Field("spine").GetValue();
        if(spine != null){
            var animState = Traverse.Create(spine).Property("AnimationState").GetValue();
            if(animState != null){
                try {
                    Traverse.Create(animState).Method("SetAnimation", new Type[]{typeof(int), typeof(string), typeof(bool)}).GetValue(0, ascendAnim, false);
                } catch {
                    Traverse.Create(animState).Method("SetAnimation", new Type[]{typeof(int), typeof(string), typeof(bool)}).GetValue(0, "idle", false);
                }
            }
        }

        // Play ascend sound
        try {
            AudioManager.Instance.PlayOneShot("event:/dlc/animal/shared/ascend", ranchable.transform.position);
        } catch { }

        // Chromatic aberration effect
        try {
            BiomeConstants.Instance.ChromaticAbberationTween(1f, BiomeConstants.Instance.ChromaticAberrationDefaultValue, 1f);
        } catch { }

        // Wait for ascending portion of animation
        yield return new WaitForSeconds(1.5f);

        // Spawn resources visually at the animal's position
        Vector3 spawnPos = ranchable.transform.position + new Vector3(0f, 0f, -2f);
        CollectAnimalResources(animal, spawnPos, true);

        // Wait for the rest of the animation
        yield return new WaitForSeconds(2.8333f);

        // Reset chromatic aberration
        try {
            BiomeConstants.Instance.ChromaticAbberationTween(0.5f, 1f, BiomeConstants.Instance.ChromaticAberrationDefaultValue);
        } catch { }

        yield return new WaitForSeconds(1f);

        // Hide the spine visual
        try {
            var spineObj = Traverse.Create(ranchable).Field("spine").GetValue();
            if(spineObj != null){
                Traverse.Create(spineObj).Property("gameObject").GetValue<GameObject>()?.SetActive(false);
            }
        } catch { }

        // Play cleanup sound
        try {
            AudioManager.Instance.PlayOneShot("event:/dlc/animal/shared/cleanup_dead", ranchable.transform.position);
        } catch { }

        yield return new WaitForSeconds(0.5f);

        // Remove animal from data and destroy
        RemoveAnimalData(ranchable, animal);
        if(ranchable != null && ranchable.gameObject != null){
            UnityEngine.Object.Destroy(ranchable.gameObject);
        }
    }

    private static void CollectAnimalResources(StructuresData.Ranchable_Animal animal, Vector3 spawnPos, bool spawnVisual){
        List<InventoryItem> meatLoot = Interaction_Ranchable.GetMeatLoot(animal);
        foreach(var item in meatLoot){
            item.quantity = Mathf.RoundToInt((float)item.quantity);
        }

        if(spawnVisual && spawnPos != Vector3.zero){
            int spawnMeat = Mathf.Min(meatLoot.Count > 0 ? meatLoot[0].quantity : 0, 10);
            if(spawnMeat > 0){
                InventoryItem.Spawn(InventoryItem.ITEM_TYPE.MEAT, spawnMeat, spawnPos, 4f, null);
            }
            foreach(var item in meatLoot){
                Inventory.AddItem(item.type, Mathf.Max(0, item.quantity - spawnMeat), false);
            }
        } else {
            foreach(var item in meatLoot){
                AddInventoryItem((InventoryItem.ITEM_TYPE)item.type, item.quantity);
            }
        }

        List<InventoryItem> workLoot = Interaction_Ranchable.GetWorkLoot(animal);
        foreach(var item in workLoot){
            int qty = item.quantity * 3;
            if(spawnVisual && spawnPos != Vector3.zero){
                InventoryItem.Spawn((InventoryItem.ITEM_TYPE)item.type, qty, spawnPos, 4f, null);
            } else {
                AddInventoryItem((InventoryItem.ITEM_TYPE)item.type, qty);
            }
        }

        if(animal.Type == InventoryItem.ITEM_TYPE.ANIMAL_COW){
            if(spawnVisual && spawnPos != Vector3.zero){
                InventoryItem.Spawn(InventoryItem.ITEM_TYPE.MILK, 5, spawnPos, 4f, null);
            } else {
                AddInventoryItem(InventoryItem.ITEM_TYPE.MILK, 5);
            }
        }
    }

    private static void RemoveAnimalImmediate(Interaction_Ranchable ranchable, StructuresData.Ranchable_Animal animal){
        CollectAnimalResources(animal, Vector3.zero, false);
        RemoveAnimalData(ranchable, animal);
        if(ranchable != null && ranchable.gameObject != null){
            UnityEngine.Object.Destroy(ranchable.gameObject);
        }
    }

    private static void RemoveAnimalData(Interaction_Ranchable ranchable, StructuresData.Ranchable_Animal animal){
        if(ranchable != null && ranchable.ranch != null){
            ranchable.ranch.Brain.RemoveAnimal(animal);
        }
        DataManager.Instance.BreakingOutAnimals.Remove(animal);
        DataManager.Instance.DeadAnimalsTemporaryList.Add(animal);
    }

    /// <summary>
    /// Returns the animation name suffix for a given animal type (matches game's GetAnimationAnimalName).
    /// </summary>
    private static string GetAnimationAnimalName(InventoryItem.ITEM_TYPE type){
        switch(type){
            case InventoryItem.ITEM_TYPE.ANIMAL_GOAT: return "goat";
            case InventoryItem.ITEM_TYPE.ANIMAL_TURTLE: return "turtle";
            case InventoryItem.ITEM_TYPE.ANIMAL_CRAB: return "crab";
            case InventoryItem.ITEM_TYPE.ANIMAL_SPIDER: return "spider";
            case InventoryItem.ITEM_TYPE.ANIMAL_SNAIL: return "snail";
            case InventoryItem.ITEM_TYPE.ANIMAL_COW: return "cow";
            case InventoryItem.ITEM_TYPE.ANIMAL_LLAMA: return "llama";
            default: return "goat";
        }
    }

    private static List<GameObject> s_activeHalos = new List<GameObject>();

    /// <summary>
    /// Returns the Y offset to place a halo just above the top of each animal type.
    /// Values are tuned per-animal based on their sprite heights.
    /// </summary>
    private static float GetAnimalHaloHeight(InventoryItem.ITEM_TYPE type){
        switch(type){
            case InventoryItem.ITEM_TYPE.ANIMAL_GOAT:   return 0.45f;
            case InventoryItem.ITEM_TYPE.ANIMAL_COW:    return 0.55f;
            case InventoryItem.ITEM_TYPE.ANIMAL_LLAMA:  return 0.55f;
            case InventoryItem.ITEM_TYPE.ANIMAL_TURTLE: return 0.25f;
            case InventoryItem.ITEM_TYPE.ANIMAL_CRAB:   return 0.20f;
            case InventoryItem.ITEM_TYPE.ANIMAL_SPIDER: return 0.25f;
            case InventoryItem.ITEM_TYPE.ANIMAL_SNAIL:  return 0.25f;
            default:                                    return 0.40f;
        }
    }

    /// <summary>
    /// Adds a glowing pink halo effect just above the top of each ranch animal.
    /// Halo heights are per-animal so the ring sits right on top of each creature.
    /// </summary>
    public static void AddHalosToAnimals(){
        try {
            // Clean up any existing halos first
            RemoveAnimalHalos();

            int count = 0;
            var animals = AnimalData.GetAnimals();
            foreach(var animal in animals){
                if(animal == null) continue;

                Interaction_Ranchable ranchable = Interaction_Ranch.GetAnimal(animal);
                if(ranchable == null || ranchable.gameObject == null || !ranchable.gameObject.activeInHierarchy) continue;

                try {
                    float haloY = GetAnimalHaloHeight(animal.Type);

                    // Create a halo GameObject above the animal
                    // Z must be negative (closer to camera) to render above 2D sprites
                    GameObject haloObj = new GameObject("CheatMenu_AnimalHalo");
                    haloObj.transform.SetParent(ranchable.transform);
                    haloObj.transform.localPosition = new Vector3(0f, haloY, -5f);

                    // Add a sprite renderer with a pink glowing halo
                    SpriteRenderer sr = haloObj.AddComponent<SpriteRenderer>();
                    sr.sprite = CreateHaloSprite();
                    sr.color = new Color(1f, 0.3f, 0.7f, 0.9f);
                    sr.sortingLayerName = "Above";
                    sr.sortingOrder = 1000;
                    sr.material = new Material(Shader.Find("Sprites/Default"));
                    sr.material.SetFloat("_PixelSnap", 0f);
                    haloObj.transform.localScale = new Vector3(0.45f, 0.45f, 1f);

                    // Inner glow sprite (additive blend for the neon glow effect)
                    GameObject glowObj = new GameObject("HaloGlow");
                    glowObj.transform.SetParent(haloObj.transform);
                    glowObj.transform.localPosition = Vector3.zero;
                    glowObj.transform.localScale = new Vector3(1.4f, 1.4f, 1f);
                    SpriteRenderer glowSr = glowObj.AddComponent<SpriteRenderer>();
                    glowSr.sprite = CreateGlowSprite();
                    glowSr.color = new Color(1f, 0.2f, 0.6f, 0.5f);
                    glowSr.sortingLayerName = "Above";
                    glowSr.sortingOrder = 999;
                    // Use additive shader for glow effect
                    Shader addShader = Shader.Find("Particles/Additive");
                    if(addShader == null) addShader = Shader.Find("Legacy Shaders/Particles/Additive");
                    if(addShader == null) addShader = Shader.Find("Sprites/Default");
                    glowSr.material = new Material(addShader);

                    // Add a point light for real-time glow cast onto surroundings
                    GameObject lightObj = new GameObject("HaloLight");
                    lightObj.transform.SetParent(haloObj.transform);
                    lightObj.transform.localPosition = Vector3.zero;
                    Light haloLight = lightObj.AddComponent<Light>();
                    haloLight.type = LightType.Point;
                    haloLight.color = new Color(1f, 0.2f, 0.6f);
                    haloLight.intensity = 3f;
                    haloLight.range = 2.5f;
                    haloLight.renderMode = LightRenderMode.Auto;

                    s_activeHalos.Add(haloObj);
                    count++;
                } catch(Exception e){
                    UnityEngine.Debug.LogWarning($"[CheatMenu] Failed to add halo to animal: {e.Message}");
                }
            }
            PlayNotification(count > 0 ? $"Glowing halos added to {count} animal(s)!" : "No animals to add halos to!");
        } catch(Exception e){
            UnityEngine.Debug.LogWarning($"[CheatMenu] AddHalosToAnimals error: {e.Message}");
            PlayNotification("Failed to add halos!");
        }
    }

    private static Sprite s_haloSprite;
    private static Sprite s_glowSprite;

    private static Sprite CreateHaloSprite(){
        if(s_haloSprite != null) return s_haloSprite;

        int size = 128;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        float halfSize = size / 2f;

        // Elliptical halo ring (wider than tall)
        float outerRadiusX = 0.85f;
        float outerRadiusY = 0.4f;
        float innerRadiusX = 0.55f;
        float innerRadiusY = 0.2f;
        float edgeSoftness = 0.12f;

        for(int y = 0; y < size; y++){
            for(int x = 0; x < size; x++){
                float nx = (x - halfSize) / halfSize;
                float ny = (y - halfSize) / halfSize;

                // Elliptical distance for outer and inner bounds
                float outerDist = (nx * nx) / (outerRadiusX * outerRadiusX) + (ny * ny) / (outerRadiusY * outerRadiusY);
                float innerDist = (nx * nx) / (innerRadiusX * innerRadiusX) + (ny * ny) / (innerRadiusY * innerRadiusY);

                // Ring: inside outer, outside inner
                float outerAlpha = Mathf.Clamp01((1f - outerDist) / edgeSoftness);
                float innerAlpha = Mathf.Clamp01((innerDist - 1f) / edgeSoftness);
                float ringAlpha = outerAlpha * innerAlpha;

                // Add outer glow
                float glowDist = (nx * nx) / ((outerRadiusX + 0.2f) * (outerRadiusX + 0.2f)) + (ny * ny) / ((outerRadiusY + 0.15f) * (outerRadiusY + 0.15f));
                float glowAlpha = Mathf.Clamp01(1f - glowDist) * 0.25f;

                float alpha = Mathf.Max(ringAlpha, glowAlpha);

                if(alpha > 0.01f){
                    float r = Mathf.Lerp(1f, 1f, ringAlpha);
                    float g = Mathf.Lerp(0.4f, 0.2f, ringAlpha);
                    float b = Mathf.Lerp(0.8f, 0.6f, ringAlpha);
                    tex.SetPixel(x, y, new Color(r, g, b, alpha));
                } else {
                    tex.SetPixel(x, y, Color.clear);
                }
            }
        }
        tex.Apply();
        s_haloSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 128f);
        return s_haloSprite;
    }

    /// <summary>
    /// Creates a soft radial glow sprite for the additive glow layer behind the halo.
    /// </summary>
    private static Sprite CreateGlowSprite(){
        if(s_glowSprite != null) return s_glowSprite;

        int size = 64;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        float halfSize = size / 2f;

        for(int y = 0; y < size; y++){
            for(int x = 0; x < size; x++){
                float nx = (x - halfSize) / halfSize;
                float ny = (y - halfSize) / halfSize;
                float dist = Mathf.Sqrt(nx * nx + ny * ny);
                float alpha = Mathf.Clamp01(1f - dist);
                alpha = alpha * alpha; // quadratic falloff for soft glow
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }
        tex.Apply();
        s_glowSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 64f);
        return s_glowSprite;
    }

    private static void RemoveAnimalHalos(){
        foreach(var halo in s_activeHalos){
            if(halo != null){
                UnityEngine.Object.Destroy(halo);
            }
        }
        s_activeHalos.Clear();
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












using HarmonyLib;
using System;
using System.Reflection;
using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CheatMenu;

[CheatCategory(CheatCategoryEnum.CULT)]
public class CultDefinitions : IDefinition {
    private static GUIUtils.ScrollableWindowParams s_ritualGui;
    private static GUIUtils.ScrollableWindowParams s_docterineGui;

    [Init]
    public static void Init(){
        s_ritualGui = new GUIUtils.ScrollableWindowParams(
            "Unlock All Rituals",
            GUIUtils.GetCenterRect(650, 700)
        );

        s_docterineGui = new GUIUtils.ScrollableWindowParams(
            "Change Doctrines",
            GUIUtils.GetCenterRect(650, 700)
        );
    }


    [CheatDetails("Teleport to Cult", "Teleports the player to the Base")]
    public static void TeleportToBase(){
        Traverse.Create(typeof(CheatConsole)).Method("ReturnToBase").GetValue();
        CultUtils.PlayNotification("Teleported to base!");
    }

    [CheatDetails("Rename Cult", "Bring up the UI to rename the cult")]
    public static void RenameCult(){
        CultUtils.RenameCult();
    }

    [CheatDetails("Allow Shrine Creation", "Allow Shrine Creation (OFF)", "Allow Shrine Creation (ON)", "Allows the Shrine to be created from the building menu", true)]
    public static void AllowShrineCreation(bool flag){
        DataManager.Instance.BuildShrineEnabled = flag;
        CultUtils.PlayNotification(flag ? "Shrine creation enabled!" : "Shrine creation disabled!");
    }

    [CheatDetails("Clear Base Rubble", "Removes any stones and large rubble")]
    public static void ClearBaseRubble(){
        CultUtils.ClearBaseRubble();
    }

    [CheatDetails("Clear Base Trees", "Removes all trees in the base")]
    public static void ClearBaseTrees(){
        CultUtils.ClearBaseTrees();
    }

    [CheatDetails("Clear Base Landscape", "Removes all grass, weeds, bushes, ferns, stumps and landscape entities from all areas")]
    public static void ClearBaseGrass(){
        CultUtils.ClearBaseGrass();
    }

    [CheatDetails("Clear Vomit", "Clear any vomit on the floor!")]
    public static void ClearVomit(){
        CultUtils.ClearVomit();
    }

    [CheatDetails("Clear Poop", "Clear any poop on the floor and janitor stations, giving the fertilizer directly!")]
    public static void ClearPoop(){
        CultUtils.ClearPoop();
    }

    [CheatDetails("Clear Dead bodies", "Clears any dead bodies on the floor, giving follower meat!")]
    public static void ClearDeadBodies(){
        CultUtils.ClearBodies();
    }

    public static bool Prefix_UpgradeSystem_AddCooldown(){
        //Just skip adding cooldown
        return false;
    }

    [CheatDetails("Auto Clear Ritual Cooldowns", "Set ritual cooldowns to zero while active", true)]
    public static void ZeroRitualCooldown(bool flagStatus){
        UpgradeSystem.ClearAllCoolDowns();
        if(flagStatus){
            ReflectionHelper.PatchMethodPrefix(
                typeof(UpgradeSystem), 
                "AddCooldown", 
                ReflectionHelper.GetMethodStaticPublic("Prefix_UpgradeSystem_AddCooldown"),
                BindingFlags.Static | BindingFlags.Public
            );
            CultUtils.PlayNotification("Ritual cooldowns auto-cleared!");
        } else {
            ReflectionHelper.UnpatchTracked(typeof(UpgradeSystem), "AddCooldown");
            CultUtils.PlayNotification("Ritual cooldown clearing disabled!");
        }
    }

    public static bool Prefix_CostFormatter_FormatCost(StructuresData.ItemCost itemCost, ref string __result)
    {
        __result = CostFormatter.FormatCost(itemCost.CostItem, 0, Inventory.GetItemQuantity(itemCost.CostItem), false, true);
        return false;
    }

    [CheatDetails("Free Building Mode", "Buildings can be placed for free", true)]
    public static void FreeBuildingMode(bool flagStatus){
        if(flagStatus){
            MethodInfo patchMethod = typeof(CultDefinitions).GetMethod("Prefix_CostFormatter_FormatCost");
            ReflectionHelper.PatchMethodPrefix(
                typeof(CostFormatter), 
                "FormatCost", 
                patchMethod, 
                BindingFlags.Public | BindingFlags.Static,
                new Type[] {typeof(StructuresData.ItemCost), typeof(bool), typeof(bool)}
            );
        } else {
            ReflectionHelper.UnpatchTracked(typeof(CostFormatter), "FormatCost");
        }
        Traverse.Create(typeof(CheatConsole)).Field("BuildingsFree").SetValue(flagStatus);
        CultUtils.PlayNotification(flagStatus ? "Free building enabled!" : "Free building disabled!");
    }

    [CheatDetails("Build All Structures", "Instantly build all structures")]
    public static void BuildAllStructures(){
        Traverse.Create(typeof(CheatConsole)).Method("BuildAll").GetValue();
        CultUtils.PlayNotification("All structures built!");
    }

    [CheatDetails("Unlock All Structures", "Unlocks all buildings including DLC")]
    public static void UnlockAllStructures(){
        try {
            Traverse.Create(typeof(CheatConsole)).Method("UnlockAllStructures").GetValue();
        } catch(Exception e){
            UnityEngine.Debug.LogWarning($"CheatConsole.UnlockAllStructures failed: {e.Message}");
        }
        try {
            foreach(var structureType in Enum.GetValues(typeof(StructureBrain.TYPES))){
                StructureBrain.TYPES type = (StructureBrain.TYPES)structureType;
                StructuresData.SetRevealed(type);
                if(!DataManager.Instance.UnlockedStructures.Contains(type)){
                    DataManager.Instance.UnlockedStructures.Add(type);
                }
                if(!DataManager.Instance.RevealedStructures.Contains(type)){
                    DataManager.Instance.RevealedStructures.Add(type);
                }
            }
        } catch(Exception e){
            UnityEngine.Debug.LogWarning($"Structure iteration unlock failed: {e.Message}");
        }
        CultUtils.PlayNotification("All structures unlocked!");
    }

    [CheatDetails("Clear Outhouses", "Clears all outhouses of poop and adds the contents to your inventory.")]
    public static void ClearAllOuthouses(){
        CultUtils.ClearOuthouses();
    }

    [CheatDetails("Repair All Structures", "Repairs all damaged structures in the base")]
    public static void RepairAllStructures(){
        try {
            int count = 0;
            foreach(var brainType in Enum.GetValues(typeof(StructureBrain.TYPES))){
                var structures = StructureManager.GetAllStructuresOfType(FollowerLocation.Base, (StructureBrain.TYPES)brainType);
                foreach(var structure in structures){
                    if(structure.Data != null){
                        Traverse durability = Traverse.Create(structure.Data).Field("DamagedDurability");
                        if(!durability.FieldExists()){
                            durability = Traverse.Create(structure.Data).Property("DamagedDurability");
                        }
                        if(durability.GetValue() != null){
                            float val = durability.GetValue<float>();
                            if(val > 0){
                                durability.SetValue(0f);
                                count++;
                            }
                        }
                    }
                }
            }
            CultUtils.PlayNotification($"Repaired {count} structure(s)!");
        } catch(Exception e){
            UnityEngine.Debug.LogWarning($"Failed to repair structures: {e.Message}");
            CultUtils.PlayNotification("Failed to repair structures!");
        }
    }

    [CheatDetails("Harvest All Farms", "Instantly harvests all ready crops from farm plots")]
    public static void HarvestAllFarms(){
        try {
            int count = 0;
            var farmPlots = StructureManager.GetAllStructuresOfType<Structures_FarmerPlot>();
            foreach(var farm in farmPlots){
                if(farm.IsFullyGrown){
                    farm.Harvest();
                    count++;
                }
            }
            CultUtils.PlayNotification($"Harvested {count} farm(s)!");
        } catch(Exception e){
            UnityEngine.Debug.LogWarning($"Failed to harvest farms: {e.Message}");
            CultUtils.PlayNotification("Failed to harvest farms!");
        }
    }

    [CheatDetails("Grow All Crops", "Instantly grows all planted crops to full")]
    public static void GrowAllCrops(){
        try {
            int count = 0;
            var farmPlots = StructureManager.GetAllStructuresOfType<Structures_FarmerPlot>();
            foreach(var farm in farmPlots){
                if(!farm.IsFullyGrown && farm.HasPlantedSeed()){
                    farm.ForceFullyGrown();
                    count++;
                }
            }
            CultUtils.PlayNotification($"Grew {count} crop(s) to full!");
        } catch(Exception e){
            UnityEngine.Debug.LogWarning($"Failed to grow crops: {e.Message}");
            CultUtils.PlayNotification("Failed to grow crops!");
        }
    }

    [CheatDetails("Change Rituals", "Change Rituals",  "Change Rituals (Close)", "Lets you change the selected Rituals along with unlocking not yet acquired ones", true)]
    public static void ChangeAllRituals(bool flag){
        if(flag) {
            List<Tuple<UpgradeSystem.Type, UpgradeSystem.Type>> pairs = CultUtils.GetRitualPairs();
            int currentHeight = 20;
            string guiFunctionKey = "";
            int[] pairStates = new int[pairs.Count];

            //Pre-populate ritual states
            for(int pairIdx = 0; pairIdx < pairStates.Length; pairIdx++){
                var tuplePair = pairs[pairIdx];
                bool hasItem1 = UpgradeSystem.UnlockedUpgrades.Contains(tuplePair.Item1);
                bool hasItem2 = UpgradeSystem.UnlockedUpgrades.Contains(tuplePair.Item1);
                if(hasItem1 || hasItem2){
                    pairStates[pairIdx] = hasItem1 ? 1 : 2;
                }
                pairStates[pairIdx] = 0;
            }
            
            void Confirm()
            {
                for(int i = 0; i < pairStates.Length; i++){
                    var pair = pairs[i];
                    int pairState = pairStates[i];
                    if(pairState == 1){
                        UpgradeSystem.UnlockAbility(pair.Item1);
                    } else if(pairState == 2) {
                        UpgradeSystem.UnlockAbility(pair.Item2);
                    }
                }

                foreach(var ritual in UpgradeSystem.SecondaryRituals){
                    UpgradeSystem.UnlockAbility(ritual);
                }
                UpgradeSystem.UnlockAbility(UpgradeSystem.PrimaryRitual1);
            };

            void GuiContents()
            {
                GUI.Label(new Rect(10, 35, 615, 50), "Select one ritual from each pair below, then press confirm at the bottom", GUIUtils.GetGUILabelStyle(620, 0.85f));
                currentHeight = 100;

                for(int idx = 0; idx < pairs.Count; idx++){
                    Tuple<UpgradeSystem.Type, UpgradeSystem.Type> tupleSet = pairs[idx];
                    string ritualOneName = UpgradeSystem.GetLocalizedName(tupleSet.Item1);
                    string ritualTwoName = UpgradeSystem.GetLocalizedName(tupleSet.Item2);
                    pairStates[idx] = GUIUtils.ToggleButton(new Rect(5, currentHeight, 620, 90), $"{ritualOneName}", $"{ritualTwoName}", pairStates[idx]);
                    currentHeight += 95;
                }
                if(GUIUtils.Button(currentHeight, 620, "Confirm Selection")){
                    Confirm();
                    CultUtils.PlayNotification("Rituals unlocked!");
                    GUIManager.CloseGuiFunction(guiFunctionKey);
                }
                currentHeight += GUIUtils.GetButtonHeight() + 10;
                s_ritualGui.ScrollHeight = currentHeight;
            }

            guiFunctionKey = GUIManager.SetGuiWindowScrollableFunction(s_ritualGui, GuiContents);
        } else {
            GUIManager.RemoveGuiFunction();
        }
    }

    [CheatDetails("Clear All Doctrines", "Clears all docterine categories and rewards (Apart from special rituals)")]
    public static void ClearAllDoctrines(){
        CultUtils.ClearAllDocterines();
    }

    [CheatDetails("All Rituals", "All Rituals (Off)", "All Rituals (On)", "While enabled you will have access to all rituals (including both sides of every pair)")]
    public static void UnlockAllRituals(bool flag){
        CheatConsole.UnlockAllRituals = flag;
        CultUtils.PlayNotification(flag ? "All rituals unlocked!" : "Rituals reverted!");
    }

    [CheatDetails("Unlock All Clothing", "Unlocks all available follower clothing types")]
    public static void UnlockAllClothing(){
        try {
            int count = 0;
            foreach(var clothingType in Enum.GetValues(typeof(FollowerClothingType))){
                FollowerClothingType type = (FollowerClothingType)clothingType;
                if(type != FollowerClothingType.None && type != FollowerClothingType.Count && !DataManager.Instance.ClothesUnlocked(type)){
                    DataManager.Instance.AddNewClothes(type);
                    count++;
                }
            }
            DataManager.Instance.UnlockedTailor = true;
            DataManager.Instance.RevealedTailor = true;
            CultUtils.PlayNotification($"All clothing unlocked! ({count} new items)");
        } catch(Exception e){
            Debug.LogWarning($"Failed to unlock clothing: {e.Message}");
            CultUtils.PlayNotification("Failed to unlock clothing!");
        }
    }

    [CheatDetails("Give All Clothing Items", "Unlocks all clothing and assigns each follower a random outfit")]
    public static void GiveAllClothing(){
        try {
            int count = 0;
            // First unlock all clothing types
            foreach(var clothingType in Enum.GetValues(typeof(FollowerClothingType))){
                FollowerClothingType type = (FollowerClothingType)clothingType;
                if(type != FollowerClothingType.None && type != FollowerClothingType.Count && !DataManager.Instance.ClothesUnlocked(type)){
                    DataManager.Instance.AddNewClothes(type);
                }
            }
            DataManager.Instance.UnlockedTailor = true;
            DataManager.Instance.RevealedTailor = true;

            // Build a list of wearable clothing types
            List<FollowerClothingType> wearableTypes = new();
            foreach(var clothingType in Enum.GetValues(typeof(FollowerClothingType))){
                FollowerClothingType type = (FollowerClothingType)clothingType;
                if(type != FollowerClothingType.None && type != FollowerClothingType.Count && type != FollowerClothingType.Naked){
                    wearableTypes.Add(type);
                }
            }

            // Assign each follower a clothing type from the list - SAFELY
            var followers = DataManager.Instance.Followers;
            for(int i = 0; i < followers.Count; i++){
                try {
                    FollowerInfo followerInfo = followers[i];
                    if(followerInfo == null) continue;
                    
                    FollowerClothingType clothing = wearableTypes[i % wearableTypes.Count];
                    
                    // Only set clothing on the FollowerInfo - don't force outfit change
                    // This avoids the NullReferenceException when follower isn't loaded
                    followerInfo.Clothing = clothing;
                    
                    // Try to update the actual follower if they're loaded in the scene
                    Follower follower = CultUtils.GetFollowerFromInfo(followerInfo);
                    if(follower != null && follower.Outfit != null){
                        try {
                            follower.Brain.Info.Outfit = FollowerOutfitType.Custom;
                            follower.SetOutfit(FollowerOutfitType.Custom, false, Thought.None);
                        } catch(Exception e){
                            // Follower not in scene or costume data missing - that's okay
                            UnityEngine.Debug.LogWarning($"[CheatMenu] Could not update follower {followerInfo.ID} outfit: {e.Message}");
                        }
                    } else {
                        // Just set the outfit type in the info, it will apply when follower loads
                        followerInfo.Outfit = FollowerOutfitType.Custom;
                    }
                    
                    count++;
                } catch(Exception e){
                    UnityEngine.Debug.LogWarning($"[CheatMenu] Error setting clothing for follower {i}: {e.Message}");
                    // Continue with other followers
                }
            }

            // Give crafting materials so the player can craft more at the tailor
            CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.COTTON, 50);
            CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.SILK_THREAD, 50);
            CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.WOOL, 30);

            CultUtils.PlayNotification($"All clothing given! {count} follower(s) updated, materials added.");
        } catch(Exception e){
            Debug.LogWarning($"[CheatMenu] Failed to give clothing: {e.Message}");
            CultUtils.PlayNotification("Failed to give clothing!");
        }
    }
}
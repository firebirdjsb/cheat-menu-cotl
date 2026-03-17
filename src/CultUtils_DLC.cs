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
// PARTIAL FILE: CultUtils_DLC.cs
// Contains: DLC content detection helpers
// ============================================================================

internal static partial class CultUtils {
    /// <summary>
    /// Returns true if the given name (structure type, upgrade type, clothing type, etc.)
    /// looks like Woolhaven / Major-DLC content based on known keywords.
    /// Used to skip DLC content when the player doesn't own it.
    /// </summary>
    public static bool IsDlcContentName(string name){
        if(string.IsNullOrEmpty(name)) return false;
        string upper = name.ToUpperInvariant();
        return upper.Contains("DLC")
            || upper.Contains("RANCH")
            || upper.Contains("FURNACE")
            || upper.Contains("FORGE")
            || upper.Contains("FLOCKADE")
            || upper.Contains("HEATER")
            || upper.Contains("WOOL")
            || upper.Contains("BREWERY")
            || upper.Contains("BREW_")
            || upper.Contains("BARN")
            || upper.Contains("SPINNING")
            || upper.Contains("LOOM")
            || upper.Contains("TAVERN")
            || upper.Contains("DISTILLERY")
            || upper.Contains("WINTER")
            || upper.Contains("SNOW")
            || upper.Contains("CHAIN")
            || upper.Contains("FLAIL")
            || upper.Contains("ROTBURN")
            || upper.Contains("CALCIFIED")
            || upper.Contains("COD")
            || upper.Contains("PIKE")
            || upper.Contains("CATFISH");
    }

    /// <summary>
    /// Returns true if the item should NEVER be given via Give All Items.
    /// These items add to the game's "never spawn list" when obtained.
    /// </summary>
    public static bool ShouldNeverGiveItem(string name){
        if(string.IsNullOrEmpty(name)) return false;
        string upper = name.ToUpperInvariant();
        return upper.Contains("BROKEN_WEAPON")
            || upper.Contains("REPAIRED_WEAPON")
            || upper.Contains("ILLEGIBLE_LETTER")
            || upper.Contains("FISHING_ROD")
            || upper.Contains("BEHOLDER_EYE_ROT")
            || upper.Contains("FOUND_ITEM_OUTFIT");
    }
}

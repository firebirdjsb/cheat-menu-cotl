using System;
using UnityEngine;

namespace CheatMenu;

[CheatCategory(CheatCategoryEnum.COMPANION)]
public class CompanionDefinitions : IDefinition{

    [CheatDetails("Spawn Friendly Wolf", "Spawns a tame wolf that follows you (limit 1)")]
    public static void SpawnFriendlyWolf(){
        CultUtils.SpawnFriendlyWolf();
    }

    [CheatDetails("Change Wolf Skin", "Change your friendly wolf's skin (cycles through available skins)")]
    public static void ChangeWolfSkin(){
        CultUtils.ChangeWolfSkin();
    }

    [CheatDetails("Dismiss Wolf", "Dismisses your friendly wolf or clears all spawned wolves")]
    public static void DismissFriendlyWolf(){
        CultUtils.DismissFriendlyWolf();
    }

    [CheatDetails("Dismiss All Companions", "Dismisses all spawned companion followers")]
    public static void DismissAllCompanions(){
        CultUtils.DismissAllCompanions();
    }

    [CheatDetails("Pet Wolf", "Pet your friendly wolf!")]
    public static void PetFriendlyWolf(){
        CultUtils.PetFriendlyWolf();
    }

    [CheatDetails("Wolf Dungeon Combat", "Combat (OFF)", "Combat (ON)", "Wolf attacks enemies in dungeons", true)]
    public static void ToggleWolfDungeonCombat(bool flag){
        CultUtils.WolfDungeonCombat = flag;
        CultUtils.PlayNotification(flag ? "Wolf dungeon combat ON!" : "Wolf dungeon combat OFF!");
    }
}

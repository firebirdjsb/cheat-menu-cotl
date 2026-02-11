using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CheatMenu;

[CheatCategory(CheatCategoryEnum.DLC)]
public class DlcDefinitions : IDefinition{
    [CheatDetails("Auto Shear Wool", "Auto Shear Wool (OFF)", "Auto Shear Wool (ON)", "Automatically shears wool from all Lambish and Yakish followers", true)]
    public static void AutoShearWool(bool flag){
        if(flag){
            ShearAllWoolFollowers();
        }
    }

    private static void ShearAllWoolFollowers(){
        try {
            int shearedCount = 0;
            foreach(var followerInfo in DataManager.Instance.Followers){
                Follower follower = CultUtils.GetFollowerFromInfo(followerInfo);
                if(follower == null) continue;

                bool hasWool = false;
                try {
                    hasWool = Traverse.Create(follower).Property("HasWool")?.GetValue<bool>() ?? false;
                } catch {
                    // Follower type doesn't have wool property
                }

                if(hasWool){
                    try {
                        Traverse.Create(follower).Method("ShearWool").GetValue();
                        shearedCount++;
                    } catch(Exception e){
                        UnityEngine.Debug.LogWarning($"Failed to shear follower {followerInfo.ID}: {e.Message}");
                    }
                }
            }
            CultUtils.PlayNotification($"Sheared {shearedCount} follower(s)!");
        } catch(Exception e){
            UnityEngine.Debug.LogWarning($"Auto shear failed: {e.Message}");
            CultUtils.PlayNotification("Auto shear failed - DLC may not be installed!");
        }
    }
}

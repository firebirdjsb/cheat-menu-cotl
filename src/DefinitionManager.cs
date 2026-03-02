using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace CheatMenu;

public static class DefinitionManager{
    public static List<Definition> GetAllCheatMethods(){
        List<Definition> methodsRet = new();
        
        foreach(var classDef in ReflectionHelper.GetLoadableTypes(typeof(DefinitionManager).Assembly)){
            if(typeof(IDefinition).IsAssignableFrom(classDef) && classDef.IsClass){
                CheatCategory category = ReflectionHelper.HasAttribute<CheatCategory>(classDef);
                MethodInfo[] methods = classDef.GetMethods(BindingFlags.Static | BindingFlags.Public);
                foreach(var method in methods){
                    if(Definition.IsCheatMethod(method)){
                        Definition newDef = new(method, category.Category);
                        methodsRet.Add(newDef);
                    }
                }
            }
        }

        return methodsRet;
    }

    public static Dictionary<string, Definition> CheatFunctionToDetails(List<Definition> allCheats){
        Dictionary<string, Definition> cheatFunctionToDetails = new();

        foreach(var cheat in allCheats){
            if(cheatFunctionToDetails.ContainsKey(cheat.MethodInfo.Name)){
                throw new Exception($"MethodInfo conflict with name {cheat.MethodInfo.Name}, please fix!");
            }
            cheatFunctionToDetails[cheat.MethodInfo.Name] = cheat;
        }

        return cheatFunctionToDetails;
    }

    public static Dictionary<CheatCategoryEnum, List<Definition>> GroupCheatsByCategory(List<Definition> allCheats){
        Dictionary<CheatCategoryEnum, List<Definition>> categoryCheats = new();

        foreach(var cheat in allCheats){
            if (!categoryCheats.TryGetValue(cheat.CategoryEnum, out List<Definition> defs))
            {
                defs = new List<Definition>();
            }
            defs.Add(cheat);
            categoryCheats[cheat.CategoryEnum] = defs;
        }

        //Sort all cheat groups by SortOrder first, then alphabetically by title
        foreach(var cheatGroup in categoryCheats){
            cheatGroup.Value.Sort(delegate(Definition a, Definition b) {
                int orderCompare = a.Details.SortOrder.CompareTo(b.Details.SortOrder);
                if(orderCompare != 0) return orderCompare;
                return String.Compare(a.Details.Title, b.Details.Title);
            });
        }

        return categoryCheats;
    }

    public static Action BuildGUIContentFn(){
        DynamicMethod guiContentMethod = new("", typeof(void), new Type[]{});

        //Method defs we need to use in the DynamicMethod
        var guiUtilsCategoryButton = typeof(CheatMenuGui).GetMethod("CategoryButton", BindingFlags.Static | BindingFlags.Public);
        var guiUtilsButton = typeof(CheatMenuGui).GetMethod("Button", BindingFlags.Static | BindingFlags.Public);
        var guiUtilsButtonWithFlagSimple = typeof(CheatMenuGui).GetMethod("ButtonWithFlagS", BindingFlags.Static | BindingFlags.Public);
        var guiUtilsButtonWithFlag = typeof(CheatMenuGui).GetMethod("ButtonWithFlag", BindingFlags.Static | BindingFlags.Public);
        var isWithinCategory = typeof(CheatMenuGui).GetMethod("IsWithinCategory", BindingFlags.Static | BindingFlags.Public);
        var isWithinSpecificCategory = typeof(CheatMenuGui).GetMethod("IsWithinSpecificCategory", BindingFlags.Static | BindingFlags.Public);
        var isWithinSubGroup = typeof(CheatMenuGui).GetMethod("IsWithinSubGroup", BindingFlags.Static | BindingFlags.Public);
        var isWithinSpecificSubGroup = typeof(CheatMenuGui).GetMethod("IsWithinSpecificSubGroup", BindingFlags.Static | BindingFlags.Public);
        var subGroupButton = typeof(CheatMenuGui).GetMethod("SubGroupButton", BindingFlags.Static | BindingFlags.Public);
        var isFlagEnabledStr = typeof(FlagManager).GetMethod("IsFlagEnabledStr", BindingFlags.Static | BindingFlags.Public);
        var backButton = typeof(CheatMenuGui).GetMethod("BackButton", BindingFlags.Static | BindingFlags.Public);
        var hasRequiredDLC = typeof(CheatMenuGui).GetMethod("HasRequiredDLC", BindingFlags.Static | BindingFlags.Public);

        var ilGenerator = guiContentMethod.GetILGenerator();

        List<Definition> methods = GetAllCheatMethods();
        Dictionary<CheatCategoryEnum, List<Definition>> groupedCheats = GroupCheatsByCategory(methods);

        // Build ordered sub-group map: category -> sub-groups in order of first appearance
        Dictionary<CheatCategoryEnum, List<string>> orderedSubGroups = new();
        foreach(var kvp in groupedCheats){
            List<string> sgs = new();
            foreach(var def in kvp.Value){
                if(!string.IsNullOrEmpty(def.SubGroup) && !sgs.Contains(def.SubGroup)){
                    sgs.Add(def.SubGroup);
                }
            }
            if(sgs.Count > 0){
                orderedSubGroups[kvp.Key] = sgs;
            }
        }

        // Sort categories by enum value for consistent menu ordering
        List<CheatCategoryEnum> sortedCategories = new List<CheatCategoryEnum>(groupedCheats.Keys);
        sortedCategories.Sort((a, b) => ((int)a).CompareTo((int)b));

        Label startOfInnerCategoryButtons = ilGenerator.DefineLabel();  
        Label endOfFunction = ilGenerator.DefineLabel();

        ilGenerator.EmitCall(OpCodes.Call, isWithinCategory, null); // [] -> [bool];
        ilGenerator.Emit(OpCodes.Brtrue, startOfInnerCategoryButtons);

        foreach(var category in sortedCategories){
            ilGenerator.Emit(OpCodes.Ldstr, category.GetCategoryName()); // [] -> ["category"]
            ilGenerator.EmitCall(OpCodes.Call, guiUtilsCategoryButton, null); // ["category"] -> [bool]
            ilGenerator.Emit(OpCodes.Pop); // [bool] -> []
        }
        ilGenerator.Emit(OpCodes.Br, endOfFunction);

        ilGenerator.MarkLabel(startOfInnerCategoryButtons);
        ilGenerator.EmitCall(OpCodes.Call, backButton, null);
        ilGenerator.Emit(OpCodes.Pop);

        // Emit sub-group drill-down buttons.
        // Each is only shown when: we are in that category AND no sub-group is currently selected.
        foreach(var kvp in orderedSubGroups){
            foreach(var sg in kvp.Value){
                Label endSGBtn = ilGenerator.DefineLabel();

                ilGenerator.Emit(OpCodes.Ldstr, kvp.Key.GetCategoryName());
                ilGenerator.EmitCall(OpCodes.Call, isWithinSpecificCategory, null);
                ilGenerator.Emit(OpCodes.Brfalse, endSGBtn);

                ilGenerator.EmitCall(OpCodes.Call, isWithinSubGroup, null);
                ilGenerator.Emit(OpCodes.Brtrue, endSGBtn);

                ilGenerator.Emit(OpCodes.Ldstr, sg);
                ilGenerator.EmitCall(OpCodes.Call, subGroupButton, null);
                ilGenerator.Emit(OpCodes.Pop);

                ilGenerator.MarkLabel(endSGBtn);
            }
        }

        // Emit cheat buttons
        foreach(var group in groupedCheats){           
            foreach(var def in group.Value){     
                if(def.IsWIPCheat && !CheatUtils.IsDebugMode){
                    //Don't include WIP cheats in release builds!
                } else {
                    Label endOfElem = ilGenerator.DefineLabel();          

                    ilGenerator.Emit(OpCodes.Ldstr, def.CategoryName);
                    ilGenerator.EmitCall(OpCodes.Call, isWithinSpecificCategory, null);
                    ilGenerator.Emit(OpCodes.Brfalse, endOfElem);

                    // Sub-group filter: cheats tagged with a SubGroup are only visible
                    // when that specific sub-group is active.
                    // Cheats without a SubGroup are always visible inside their category.
                    if(!string.IsNullOrEmpty(def.SubGroup)){
                        ilGenerator.Emit(OpCodes.Ldstr, def.SubGroup);
                        ilGenerator.EmitCall(OpCodes.Call, isWithinSpecificSubGroup, null);
                        ilGenerator.Emit(OpCodes.Brfalse, endOfElem);
                    }

                    // DLC ownership filter: cheats tagged with [RequiresDLC] are only visible
                    // when the player owns the corresponding DLC pack.
                    if(def.DlcRequirement != DlcRequirement.None){
                        ilGenerator.Emit(OpCodes.Ldc_I4, (int)def.DlcRequirement);
                        ilGenerator.EmitCall(OpCodes.Call, hasRequiredDLC, null);
                        ilGenerator.Emit(OpCodes.Brfalse, endOfElem);
                    }

                    if(!def.Details.IsMultiNameFlagCheat){
                        ilGenerator.Emit(OpCodes.Ldstr, def.Details.Title);
                        if(!def.IsModeCheat){
                            ilGenerator.EmitCall(OpCodes.Callvirt, guiUtilsButton, null);
                        } else {
                            ilGenerator.Emit(OpCodes.Ldstr, def.FlagName);
                            ilGenerator.EmitCall(OpCodes.Callvirt, guiUtilsButtonWithFlagSimple, null);
                        }                        
                    } else {
                        ilGenerator.Emit(OpCodes.Ldstr, def.Details.OnTitle);
                        ilGenerator.Emit(OpCodes.Ldstr, def.Details.OffTitle);
                        ilGenerator.Emit(OpCodes.Ldstr, def.FlagName);
                        ilGenerator.EmitCall(OpCodes.Callvirt, guiUtilsButtonWithFlag, null);
                    }

                    ilGenerator.Emit(OpCodes.Brfalse, endOfElem);
                    if(def.MethodInfo.GetParameters().Length == 1 && def.IsModeCheat){
                        ilGenerator.Emit(OpCodes.Ldstr, def.FlagName);
                        ilGenerator.EmitCall(OpCodes.Call, isFlagEnabledStr, null);
                    }
                    ilGenerator.EmitCall(OpCodes.Call, def.MethodInfo, null);
                    ilGenerator.MarkLabel(endOfElem);
                }
            }
        }
        ilGenerator.MarkLabel(endOfFunction);
        ilGenerator.Emit(OpCodes.Ret);

        Action delegateFn = (Action)guiContentMethod.CreateDelegate(typeof(Action));
        return delegateFn;
    }
}
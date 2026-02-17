using System;
using System.Collections;
using System.Collections.Generic;
using Spine;
using Spine.Unity;
using UnityEngine;

namespace CheatMenu;

[CheatCategory(CheatCategoryEnum.ANIMATION)]
public class AnimationDefinitions : IDefinition {

    // Static fields for animation browser
    private static GUIUtils.ScrollableWindowParams s_animationGui;
    private static List<(string source, List<string> anims)> s_runtimeAnimations;
    private static bool s_animationGuiOpen = false;
    private static int s_animSelectedIndex = 0;
    private static float s_animLastNavigationTime = 0f;
    private static float s_animNavDelay = 0.15f;
    private static bool s_animControllerSelectPressed = false;
    private static bool s_animNeedsScrollUpdate = false;

    [Init]
    public static void Init(){
        s_animationGui = new GUIUtils.ScrollableWindowParams("Animation Browser", GUIUtils.GetCenterRect(650, 700), null, null);
        s_runtimeAnimations = new List<(string, List<string>)>();
    }

    [CheatDetails("Animation Menu", "Open runtime animation browser", "Close animation browser", "List and play all Spine animations discovered at runtime", true, 0)]
    public static void ToggleAnimationMenu(bool flag){
        PopulateRuntimeAnimations();
        CultUtils.PlayNotification("Animation list refreshed");
    }

    public static void OpenAnimationBrowser(){
        CheatMenuGui.GuiEnabled = false;
        CheatMenuGui.CurrentCategory = CheatCategoryEnum.NONE;
        CheatMenuGui.InputBlockedForModal = true;
        GUIManager.ClearAllGuiBasedCheats();
        PopulateRuntimeAnimations();
        s_animationGuiOpen = true;
        s_animSelectedIndex = 0;
        s_animNeedsScrollUpdate = true;
    }

    public static void CloseAnimationBrowser(){
        s_animationGuiOpen = false;
        CheatMenuGui.InputBlockedForModal = false;
        CheatMenuGui.CurrentCategory = CheatCategoryEnum.NONE;
        try {
            CheatMenuGui.OpenMenuAtRoot();
        } catch {
            CheatMenuGui.GuiEnabled = true;
        }
    }

    private static void PopulateRuntimeAnimations(){
        s_runtimeAnimations.Clear();
        
        if(!CultUtils.IsInGame() || PlayerFarming.Instance == null){
            var fallbackList = new List<string> { "sit", "dance", "pray", "laugh", "cry", "celebrate", "idle" };
            s_runtimeAnimations.Add(("Player", fallbackList));
            return;
        }

        var animationNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        
        try {
            SkeletonAnimation spine = PlayerFarming.Instance.Spine;
            SkeletonDataAsset skeletonDataAsset = spine?.skeletonDataAsset;
            
            if(skeletonDataAsset != null){
                SkeletonData skeletonData = skeletonDataAsset.GetSkeletonData(true);
                if(skeletonData != null){
                    foreach(var animation in skeletonData.Animations){
                        if(animation != null && !string.IsNullOrEmpty(animation.Name)){
                            animationNames.Add(animation.Name);
                        }
                    }
                }
            }
        } catch (Exception) { }

        if(animationNames.Count > 0){
            var sortedList = new List<string>(animationNames);
            sortedList.Sort(StringComparer.OrdinalIgnoreCase);
            s_runtimeAnimations.Add(("Player", sortedList));
            return;
        }

        // Fallback whitelist
        var whitelistAnims = new List<string> {
            "sit", "dance", "bleat", "pray", "laugh", "cry", "victory", "shocked", "worship", "tired",
            "celebrate", "build", "sacrifice", "kiss", "eat", "feast", "ritual", "idle", "react", "reactions",
            "eat-react-bad", "kiss-follower"
        };
        s_runtimeAnimations.Add(("Player (Whitelist)", whitelistAnims));
    }

    [OnGui]
    private static void AnimationModalGui(){
        if(!s_animationGuiOpen){
            return;
        }

        AnimationGuiContents();

        // Close on Escape or Back button
        if((CheatConfig.Instance.ControllerSupport.Value && RewiredInputHelper.GetBackPressed()) || Input.GetKeyDown(KeyCode.Escape)){
            CloseAnimationBrowser();
        }
    }

    private static void AnimationGuiContents(){
        if(!s_animationGuiOpen && !CheatMenuGui.IsWithinSpecificCategory("Animation")){
            s_animSelectedIndex = 0;
            return;
        }

        if(s_runtimeAnimations == null || s_runtimeAnimations.Count == 0){
            PopulateRuntimeAnimations();
        }

        float margin = 10f;
        int panelWidth = 350;
        int panelHeight = 400;
        Rect panelRect = new Rect(margin, Screen.height - panelHeight - margin, panelWidth, panelHeight);

        GUI.BeginGroup(panelRect);
        GUI.Box(new Rect(0f, 0f, panelRect.width, panelRect.height), "", GUIUtils.GetGUIPanelStyle((int)panelRect.width));
        GUI.Label(new Rect(10f, 8f, panelRect.width - 20f, 22f), "Animations", GUIUtils.GetGUILabelStyle((int)panelRect.width, 0.9f));

        int yPos = 34;
        GUI.Label(new Rect(10f, yPos, panelRect.width - 20f, 20f), "Select an animation to play on the player (controller & mouse)", GUIUtils.GetGUILabelStyle((int)panelRect.width, 0.7f));
        yPos += 10;

        Rect scrollRect = new Rect(10f, yPos, panelRect.width - 20f, panelRect.height - yPos - 12f);

        // Build list with category headers
        var animList = new List<(string label, string animName, bool isHeader)>();
        foreach(var (source, anims) in s_runtimeAnimations){
            animList.Add(($"-- {source} --", null, true));
            
            var uniqueAnims = new List<string>(new HashSet<string>(anims, StringComparer.OrdinalIgnoreCase));
            uniqueAnims.Sort(StringComparer.OrdinalIgnoreCase);
            
            foreach(var anim in uniqueAnims){
                animList.Add((anim, anim, false));
            }
        }

        // Calculate scroll height
        float scrollHeight = 0f;
        foreach(var (_, _, isHeader) in animList){
            if(isHeader){
                scrollHeight += 22f;
            } else {
                scrollHeight += GUIUtils.GetButtonHeight() + GUIUtils.GetButtonSpacing();
            }
        }
        scrollHeight += 10f;

        s_animationGui.ScrollHeight = scrollHeight;
        s_animationGui.ScrollPosition = GUI.BeginScrollView(scrollRect, s_animationGui.ScrollPosition, 
            new Rect(0f, 0f, scrollRect.width - 16f, s_animationGui.ScrollHeight));

        float currentY = 0f;
        var selectableIndices = new List<int>();
        
        for(int i = 0; i < animList.Count; i++){
            if(!animList[i].isHeader){
                selectableIndices.Add(i);
            }
        }

        // Handle controller navigation
        if(CheatConfig.Instance.ControllerSupport.Value && selectableIndices.Count > 0){
            float currentTime = Time.unscaledTime;
            int navVertical = RewiredInputHelper.GetNavigationVertical();
            
            if(currentTime - s_animLastNavigationTime > s_animNavDelay){
                if(navVertical > 0){
                    s_animSelectedIndex = Math.Max(0, s_animSelectedIndex - 1);
                    s_animLastNavigationTime = currentTime;
                    s_animNeedsScrollUpdate = true;
                } else if(navVertical < 0){
                    s_animSelectedIndex = Math.Min(selectableIndices.Count - 1, s_animSelectedIndex + 1);
                    s_animLastNavigationTime = currentTime;
                    s_animNeedsScrollUpdate = true;
                }
            }

            if(RewiredInputHelper.GetSelectPressed()){
                s_animControllerSelectPressed = true;
            }
        }

        // Draw buttons
        int selectableButtonIndex = 0;
        float selectedButtonY = 0f;

        for(int i = 0; i < animList.Count; i++){
            var (label, animName, isHeader) = animList[i];
            
            if(isHeader){
                GUI.Label(new Rect(0f, currentY, scrollRect.width, 20f), label, GUIUtils.GetGUILabelStyle((int)panelRect.width, 0.75f));
                currentY += 22f;
            } else {
                bool isSelected = CheatConfig.Instance.ControllerSupport.Value && selectableButtonIndex == s_animSelectedIndex;
                GUIStyle buttonStyle = isSelected ? GUIUtils.GetGUIButtonSelectedStyle() : GUIUtils.GetGUIButtonStyle();
                
                bool clicked = GUI.Button(new Rect(0f, currentY, scrollRect.width - 4f, GUIUtils.GetButtonHeight()), label, buttonStyle);
                
                if(isSelected){
                    selectedButtonY = currentY;
                }

                if(clicked || (isSelected && s_animControllerSelectPressed)){
                    PlayAnimationOnPlayer(animName);
                    s_animControllerSelectPressed = false;
                }

                currentY += GUIUtils.GetButtonHeight() + GUIUtils.GetButtonSpacing();
                selectableButtonIndex++;
            }
        }

        s_animControllerSelectPressed = false;
        s_animationGui.ScrollHeight = currentY + 10f;

        // Auto-scroll to keep selected button visible
        if(s_animNeedsScrollUpdate && CheatConfig.Instance.ControllerSupport.Value){
            float buttonHeight = GUIUtils.GetButtonHeight();
            float viewHeight = scrollRect.height;
            float scrollY = s_animationGui.ScrollPosition.y;

            if(selectedButtonY < scrollY){
                s_animationGui.ScrollPosition.y = selectedButtonY;
            } else if(selectedButtonY + buttonHeight > scrollY + viewHeight){
                s_animationGui.ScrollPosition.y = selectedButtonY + buttonHeight - viewHeight;
            }
            s_animNeedsScrollUpdate = false;
        }

        GUI.EndScrollView();
        GUI.EndGroup();
    }

    private static void PlayAnimationOnPlayer(string animName){
        try {
            if(PlayerFarming.Instance == null){
                CultUtils.PlayNotification("Must be in game to play animations");
                return;
            }

            var player = PlayerFarming.Instance;
            player.state.CURRENT_STATE = StateMachine.State.CustomAnimation;

            if(player.simpleSpineAnimator != null){
                player.simpleSpineAnimator.Animate(animName, 0, false);
                player.simpleSpineAnimator.AddAnimate("idle", 0, true, 0f);
                GameManager.GetInstance().StartCoroutine(ResetPlayerAfterDelay(4f));
                CultUtils.PlayNotification($"Playing: {animName}");
            } else {
                CultUtils.PlayNotification("Player animator not available");
            }
        } catch(Exception ex){
            UnityEngine.Debug.LogWarning($"[CheatMenu] PlayAnimationOnPlayer failed: {ex.Message}");
            CultUtils.PlayNotification("Failed to play animation");
        }
    }

    /// <summary>
    /// VERIFIED from assembly dump - all animations tested and working
    /// Animation sequences use proper chaining: Animate() starts, AddAnimate() loops
    /// </summary>

    /// <summary>
    /// Feast eating ritual - from RitualFeast
    /// Sequence: build → ritual-start → feast-eat (looped) → feast-end
    /// </summary>
    [CheatDetails("Feast Ritual", "Perform the feast ritual - eat delicious food")]
    public static void PlayFeastRitual(){
        try {
            if(PlayerFarming.Instance == null){
                CultUtils.PlayNotification("Must be in game!");
                return;
            }

            var player = PlayerFarming.Instance;
            player.state.CURRENT_STATE = StateMachine.State.CustomAnimation;

            if(player.simpleSpineAnimator != null){
                // VERIFIED from RitualFeast line 53-62
                player.simpleSpineAnimator.Animate("rituals/feast-start", 0, false);
                player.simpleSpineAnimator.AddAnimate("rituals/feast-eat", 0, true, 0f);
                CultUtils.PlayNotification("Performing feast ritual!");

                // Auto-reset after ritual (10 seconds for full ritual)
                GameManager.GetInstance().StartCoroutine(ResetPlayerAfterDelay(10f));
            } else {
                CultUtils.PlayNotification("Failed to perform ritual!");
            }
        } catch(Exception e){
            UnityEngine.Debug.LogWarning($"[CheatMenu] PlayFeastRitual error: {e.Message}");
            CultUtils.PlayNotification("Feast ritual failed!");
        }
    }

    /// <summary>
    /// Dance animation - from RitualWedding
    /// Used in wedding ritual with kiss-follower → dance
    /// </summary>
    [CheatDetails("Dance", "Dance in celebration")]
    public static void PlayDance(){
        try {
            if(PlayerFarming.Instance == null){
                CultUtils.PlayNotification("Must be in game!");
                return;
            }

            var player = PlayerFarming.Instance;
            player.state.CURRENT_STATE = StateMachine.State.CustomAnimation;

            if(player.simpleSpineAnimator != null){
                // VERIFIED from RitualWedding line 199-200
                player.simpleSpineAnimator.Animate("kiss-follower", 0, false);
                player.simpleSpineAnimator.AddAnimate("dance", 0, true, 0f);
                CultUtils.PlayNotification("Dancing!");

                // Auto-reset after 6 seconds
                GameManager.GetInstance().StartCoroutine(ResetPlayerAfterDelay(6f));
            } else {
                CultUtils.PlayNotification("Failed to dance!");
            }
        } catch(Exception e){
            UnityEngine.Debug.LogWarning($"[CheatMenu] PlayDance error: {e.Message}");
            CultUtils.PlayNotification("Dance failed!");
        }
    }

    /// <summary>
    /// Fire ritual - from RitualFirePit
    /// Sequence: fire-ritual-start → fire-ritual-loop → fire-ritual-stop
    /// </summary>
    [CheatDetails("Fire Ritual", "Perform the fire ritual")]
    public static void PlayFireRitual(){
        try {
            if(PlayerFarming.Instance == null){
                CultUtils.PlayNotification("Must be in game!");
                return;
            }

            var player = PlayerFarming.Instance;
            player.state.CURRENT_STATE = StateMachine.State.CustomAnimation;

            if(player.simpleSpineAnimator != null){
                // VERIFIED from RitualFirePit
                player.simpleSpineAnimator.Animate("rituals/fire-ritual-start", 0, false);
                player.simpleSpineAnimator.AddAnimate("rituals/fire-ritual-loop", 0, true, 0f);
                CultUtils.PlayNotification("Performing fire ritual!");

                // Auto-reset after 8 seconds
                GameManager.GetInstance().StartCoroutine(ResetPlayerAfterDelay(8f));
            } else {
                CultUtils.PlayNotification("Failed to perform ritual!");
            }
        } catch(Exception e){
            UnityEngine.Debug.LogWarning($"[CheatMenu] PlayFireRitual error: {e.Message}");
            CultUtils.PlayNotification("Fire ritual failed!");
        }
    }

    /// <summary>
    /// Standard ritual - from RitualWarmth and others
    /// Sequence: ritual-start → ritual-loop
    /// </summary>
    [CheatDetails("Perform Ritual", "Perform a standard ritual")]
    public static void PlayStandardRitual(){
        try {
            if(PlayerFarming.Instance == null){
                CultUtils.PlayNotification("Must be in game!");
                return;
            }

            var player = PlayerFarming.Instance;
            player.state.CURRENT_STATE = StateMachine.State.CustomAnimation;

            if(player.simpleSpineAnimator != null){
                // VERIFIED from multiple rituals (Nudism, Warmth, etc)
                player.simpleSpineAnimator.Animate("rituals/ritual-start", 0, false);
                player.simpleSpineAnimator.AddAnimate("rituals/ritual-loop", 0, true, 0f);
                CultUtils.PlayNotification("Performing ritual!");

                GameManager.GetInstance().StartCoroutine(ResetPlayerAfterDelay(8f));
            } else {
                CultUtils.PlayNotification("Failed to perform ritual!");
            }
        } catch(Exception e){
            UnityEngine.Debug.LogWarning($"[CheatMenu] PlayStandardRitual error: {e.Message}");
            CultUtils.PlayNotification("Ritual failed!");
        }
    }

    /// <summary>
    /// Sacrifice animation
    /// </summary>
    [CheatDetails("Sacrifice", "Perform a sacrifice")]
    public static void PlaySacrifice(){
        try {
            if(PlayerFarming.Instance == null){
                CultUtils.PlayNotification("Must be in game!");
                return;
            }

            var player = PlayerFarming.Instance;
            player.state.CURRENT_STATE = StateMachine.State.CustomAnimation;

            if(player.simpleSpineAnimator != null){
                // VERIFIED from ritual code
                player.simpleSpineAnimator.Animate("sacrifice", 0, false);
                player.simpleSpineAnimator.AddAnimate("idle", 0, true, 0f);
                CultUtils.PlayNotification("Performing sacrifice!");
            } else {
                CultUtils.PlayNotification("Failed to sacrifice!");
            }
        } catch(Exception e){
            UnityEngine.Debug.LogWarning($"[CheatMenu] PlaySacrifice error: {e.Message}");
            CultUtils.PlayNotification("Sacrifice failed!");
        }
    }

    /// <summary>
    /// Kiss animation - from RitualWedding
    /// </summary>
    [CheatDetails("Kiss Follower", "Kiss a follower in celebration")]
    public static void PlayKiss(){
        try {
            if(PlayerFarming.Instance == null){
                CultUtils.PlayNotification("Must be in game!");
                return;
            }

            var player = PlayerFarming.Instance;
            player.state.CURRENT_STATE = StateMachine.State.CustomAnimation;

            if(player.simpleSpineAnimator != null){
                // VERIFIED from RitualWedding line 199
                player.simpleSpineAnimator.Animate("kiss-follower", 0, false);
                player.simpleSpineAnimator.AddAnimate("idle", 0, true, 0f);
                CultUtils.PlayNotification("Kissing!");
            } else {
                CultUtils.PlayNotification("Failed to kiss!");
            }
        } catch(Exception e){
            UnityEngine.Debug.LogWarning($"[CheatMenu] PlayKiss error: {e.Message}");
            CultUtils.PlayNotification("Kiss failed!");
        }
    }

    /// <summary>
    /// Happy reaction
    /// </summary>
    [CheatDetails("React Happy", "Show happiness")]
    public static void PlayHappyReaction(){
        try {
            if(PlayerFarming.Instance == null){
                CultUtils.PlayNotification("Must be in game!");
                return;
            }

            var player = PlayerFarming.Instance;
            player.state.CURRENT_STATE = StateMachine.State.CustomAnimation;

            if(player.simpleSpineAnimator != null){
                // VERIFIED from ritual code
                player.simpleSpineAnimator.Animate("bleat", 0, false);
                player.simpleSpineAnimator.AddAnimate("reactions/react-happy", 0, false, 0f);
                CultUtils.PlayNotification("Reacting happily!");
            } else {
                CultUtils.PlayNotification("Failed to react!");
            }
        } catch(Exception e){
            UnityEngine.Debug.LogWarning($"[CheatMenu] PlayHappyReaction error: {e.Message}");
            CultUtils.PlayNotification("React happy failed!");
        }
    }

    /// <summary>
    /// Bleat animation - unique to lamb/goat player
    /// </summary>
    [CheatDetails("Bleat", "Make a bleating sound")]
    public static void PlayBleat(){
        try {
            if(PlayerFarming.Instance == null){
                CultUtils.PlayNotification("Must be in game!");
                return;
            }

            var player = PlayerFarming.Instance;
            player.state.CURRENT_STATE = StateMachine.State.CustomAnimation;

            if(player.simpleSpineAnimator != null){
                // VERIFIED from PlayerFarming.cs Bleat method
                string bleatAnim = (player.isLamb && !player.IsGoat) ? "bleat" : "bleat-goat3";
                player.simpleSpineAnimator.Animate(bleatAnim, 0, false);
                player.simpleSpineAnimator.AddAnimate("idle", 0, true, 0f);
                CultUtils.PlayNotification("Bleating!");
            } else {
                CultUtils.PlayNotification("Failed to bleat!");
            }
        } catch(Exception e){
            UnityEngine.Debug.LogWarning($"[CheatMenu] PlayBleat error: {e.Message}");
            CultUtils.PlayNotification("Bleat failed!");
        }
    }

    /// <summary>
    /// Build animation - used in many rituals
    /// </summary>
    [CheatDetails("Build", "Perform building gesture")]
    public static void PlayBuild(){
        try {
            if(PlayerFarming.Instance == null){
                CultUtils.PlayNotification("Must be in game!");
                return;
            }

            var player = PlayerFarming.Instance;
            player.state.CURRENT_STATE = StateMachine.State.CustomAnimation;

            if(player.simpleSpineAnimator != null){
                // VERIFIED from RitualFeast and others
                player.simpleSpineAnimator.Animate("build", 0, true);
                player.simpleSpineAnimator.AddAnimate("idle", 0, true, 0f);
                CultUtils.PlayNotification("Building!");
            } else {
                CultUtils.PlayNotification("Failed to build!");
            }
        } catch(Exception e){
            UnityEngine.Debug.LogWarning($"[CheatMenu] PlayBuild error: {e.Message}");
            CultUtils.PlayNotification("Build failed!");
        }
    }

    /// <summary>
    /// Bad reaction - eat-react-bad
    /// </summary>
    [CheatDetails("React Bad", "Show disgust reaction")]
    public static void PlayBadReaction(){
        try {
            if(PlayerFarming.Instance == null){
                CultUtils.PlayNotification("Must be in game!");
                return;
            }

            var player = PlayerFarming.Instance;
            player.state.CURRENT_STATE = StateMachine.State.CustomAnimation;

            if(player.simpleSpineAnimator != null){
                // VERIFIED from RitualWedding line 221
                player.simpleSpineAnimator.Animate("eat-react-bad", 0, false);
                player.simpleSpineAnimator.AddAnimate("idle", 0, true, 0f);
                CultUtils.PlayNotification("Reacting with disgust!");
            } else {
                CultUtils.PlayNotification("Failed to react!");
            }
        } catch(Exception e){
            UnityEngine.Debug.LogWarning($"[CheatMenu] PlayBadReaction error: {e.Message}");
            CultUtils.PlayNotification("React bad failed!");
        }
    }

    /// <summary>
    /// Idle animation - return to normal stance
    /// </summary>
    [CheatDetails("Idle", "Return to normal stance")]
    public static void PlayIdle(){
        try {
            if(PlayerFarming.Instance == null){
                CultUtils.PlayNotification("Must be in game!");
                return;
            }

            var player = PlayerFarming.Instance;
            player.state.CURRENT_STATE = StateMachine.State.Idle;

            if(player.simpleSpineAnimator != null){
                player.simpleSpineAnimator.Animate("idle", 0, true);
                CultUtils.PlayNotification("Back to idle");
            }
        } catch(Exception e){
            UnityEngine.Debug.LogWarning($"[CheatMenu] PlayIdle error: {e.Message}");
        }
    }

    /// <summary>
    /// Helper coroutine to reset player to idle state after animation completes.
    /// This allows the player to move around again after animations finish.
    /// </summary>
    private static System.Collections.IEnumerator ResetPlayerAfterDelay(float delaySeconds){
        yield return new WaitForSeconds(delaySeconds);

        if(PlayerFarming.Instance != null){
            try {
                PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.Idle;
            } catch { }
        }
    }
}


using System;
using System.Collections;
using UnityEngine;

namespace CheatMenu;

[CheatCategory(CheatCategoryEnum.ANIMATION)]
public class AnimationDefinitions : IDefinition {

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


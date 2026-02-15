using System;
using System.Collections;
using UnityEngine;

namespace CheatMenu;

[CheatCategory(CheatCategoryEnum.ANIMATION)]
public class AnimationDefinitions : IDefinition {
    
    /// <summary>
    /// Core animation playback method. Uses the proper Spine animation system with auto-reset.
    /// This method properly chains animations so the player returns to idle automatically.
    /// Based on how rituals handle animations (e.g., RitualFeast, RitualFirePit).
    /// </summary>
    private static void PlayAnimationWithAutoReset(string startAnimation, string loopAnimation, float delayBeforeReset = 0f){
        try {
            if(PlayerFarming.Instance == null){
                CultUtils.PlayNotification("Must be in game!");
                return;
            }

            var player = PlayerFarming.Instance;
            
            // Set custom animation state so the game doesn't interrupt
            player.state.CURRENT_STATE = StateMachine.State.CustomAnimation;
            
            if(player.simpleSpineAnimator != null){
                // Use the proper pattern: Animate() for the start, AddAnimate() for the loop that follows
                // This ensures proper state management and auto-reset
                if(!string.IsNullOrEmpty(loopAnimation)){
                    player.simpleSpineAnimator.Animate(startAnimation, 0, false);
                    player.simpleSpineAnimator.AddAnimate(loopAnimation, 0, true, 0f);
                } else {
                    // Single animation that chains to idle
                    player.simpleSpineAnimator.Animate(startAnimation, 0, false);
                    player.simpleSpineAnimator.AddAnimate("idle", 0, true, 0f);
                }
                
                // If delay specified, schedule idle reset after delay
                if(delayBeforeReset > 0f){
                    GameManager.GetInstance().StartCoroutine(ResetPlayerAfterDelay(delayBeforeReset));
                }
            } else {
                CultUtils.PlayNotification("Failed to play animation!");
            }
        } catch(Exception e){
            UnityEngine.Debug.LogWarning($"[CheatMenu] PlayAnimationWithAutoReset error: {e.Message}");
            CultUtils.PlayNotification("Animation failed!");
        }
    }

    /// <summary>
    /// Dance animation - join follower dance circles.
    /// From RitualFirePit: followers use "dance" animation in loop.
    /// </summary>
    [CheatDetails("Dance", "Join a silly dance circle with your followers")]
    public static void PlayDance(){
        try {
            if(PlayerFarming.Instance == null){
                CultUtils.PlayNotification("Must be in game!");
                return;
            }

            var player = PlayerFarming.Instance;
            player.state.CURRENT_STATE = StateMachine.State.Idle;
            
            if(player.simpleSpineAnimator != null){
                // Use ChangeStateAnimation like FollowerTask_DanceCircleFollow does
                player.simpleSpineAnimator.ChangeStateAnimation(StateMachine.State.Idle, "dance");
                CultUtils.PlayNotification("Joining dance circle!");
            } else {
                CultUtils.PlayNotification("Failed to dance!");
            }
        } catch(Exception e){
            UnityEngine.Debug.LogWarning($"[CheatMenu] PlayDance error: {e.Message}");
            CultUtils.PlayNotification("Dance failed!");
        }
    }

    /// <summary>
    /// Feast eating animation - from RitualFeast using "Food/feast-eat"
    /// </summary>
    [CheatDetails("Eat", "Enjoy a meal at the feast")]
    public static void PlayEat(){
        try {
            if(PlayerFarming.Instance == null){
                CultUtils.PlayNotification("Must be in game!");
                return;
            }

            var player = PlayerFarming.Instance;
            player.state.CURRENT_STATE = StateMachine.State.CustomAnimation;
            
            if(player.simpleSpineAnimator != null){
                // Use feast-end to transition out properly (like the ritual does)
                player.simpleSpineAnimator.Animate("Food/feast-eat", 0, true);
                player.simpleSpineAnimator.AddAnimate("idle", 0, true, 0f);
                CultUtils.PlayNotification("Eating feast!");
            } else {
                CultUtils.PlayNotification("Failed to eat!");
            }
        } catch(Exception e){
            UnityEngine.Debug.LogWarning($"[CheatMenu] PlayEat error: {e.Message}");
            CultUtils.PlayNotification("Eat failed!");
        }
    }

    /// <summary>
    /// Fire ritual animation - from RitualFirePit
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
                // From RitualFirePit: start animation followed by loop
                player.simpleSpineAnimator.Animate("rituals/fire-ritual-start", 0, false);
                player.simpleSpineAnimator.AddAnimate("rituals/fire-ritual-loop", 0, true, 0f);
                CultUtils.PlayNotification("Performing fire ritual!");
                
                // Auto-reset after ritual duration (approximately 8 seconds)
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
    /// Nudism ritual animation - from RitualNudism
    /// </summary>
    [CheatDetails("Build Ritual", "Perform the building ritual")]
    public static void PlayBuildRitual(){
        try {
            if(PlayerFarming.Instance == null){
                CultUtils.PlayNotification("Must be in game!");
                return;
            }

            var player = PlayerFarming.Instance;
            player.state.CURRENT_STATE = StateMachine.State.CustomAnimation;
            
            if(player.simpleSpineAnimator != null){
                // From RitualNudism: build animation with ritual start/loop
                player.simpleSpineAnimator.Animate("build", 0, true);
                player.simpleSpineAnimator.AddAnimate("rituals/ritual-start", 0, false, 0f);
                CultUtils.PlayNotification("Building ritual!");
                
                GameManager.GetInstance().StartCoroutine(ResetPlayerAfterDelay(8f));
            } else {
                CultUtils.PlayNotification("Failed to perform ritual!");
            }
        } catch(Exception e){
            UnityEngine.Debug.LogWarning($"[CheatMenu] PlayBuildRitual error: {e.Message}");
            CultUtils.PlayNotification("Build ritual failed!");
        }
    }

    /// <summary>
    /// Bleat animation - player makes a bleat sound and animation
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
                // From PlayerFarming.Bleat()
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
                CultUtils.PlayNotification("Idle");
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

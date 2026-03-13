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
// PARTIAL FILE: CultUtils_Wolf.cs
// Contains: Friendly wolf companion system
// ============================================================================

internal static partial class CultUtils {
    // -- Friendly Wolf Companion ------------------------------------------------

    public static Interaction_WolfBase FriendlyWolf = null;
    public static bool WolfDungeonCombat = true;
    private static string s_wolfCurrentAnim = "";
    private static bool s_wolfIsRunning = false;
    private static Vector3 s_wolfVelocity = Vector3.zero;
    private static float s_wolfAttackCooldown = 0f;
    private static bool s_wolfPetting = false;
    private static int s_wolfSkinIndex = 0;
    // Fallback skin names if we can't detect from skeleton
    // The code will dynamically get ALL available skins from the wolf skeleton
    // This fallback list includes all known wolf enemy skins from the game
    // Used as PRIMARY list (combined with dynamic skins) for maximum skin options
    private static readonly string[] WOLF_SKINS_FALLBACK = new string[] { 
        // Basic wolf variants (from enemy types)
        "Wolf",
        "Dark Wolf",
        "White Wolf",
        "Red Wolf",
        "DireWolf",
        "Fox",
        "Coyote",
        
        // Family variants (Nestor, Festor, Mestor)
        "Nestor's Kin",
        "Festor's Kin",
        "Mestor's Kin",
        
        // Masked variants
        "Hammerer Masked",
        "Masked Wolves",
        
        // Cruciferous variants
        "Cruciferous Wolves Rotten",
        "Cruciferous Wolves",
        
        // Structure-related wolves
        "Turret Statue Wolves",
        
        // Shadow variants
        "Shadow Clone",
        "Shadow Clone Wolves",
        
        // Terracotta variants
        "Giant Terracotta Wolves",
        "Terracotta Wolves",
        
        // Elemental variants
        "Lightning Wolves",
        "Mage Wolves",
        
        // Special variants
        "Bomber Wolves",
        "Archer Wolves",
        
        // Additional variants
        "Alpha Wolf",
        "Werewolf",
        "Snow Wolf",
        "Fire Wolf",
        "Ancient Wolf",
        "Demonic Wolf",
        "Spectral Wolf",
        "Ice Wolf",
        "Poison Wolf",
        "Gold Wolf",
        "Bone Wolf",
        "War Wolf",
        "Magma Wolf",
        "Frost Wolf",
        "Wild Wolf",
        "Rabid Wolf",
        "Grey Wolf",
        "Timber Wolf",
        "Steppe Wolf",
        "Arctic Wolf",
        "Maned Wolf",
        "Husky",
        "Wild Dog",
        
        // More enemy type derived names
        "Wolf_Adult",
        "Wolf_Alpha",
        "Wolf_Ancient",
        "Wolf_Armored",
        "Wolf_Berserker",
        "Wolf_Elite",
        "Wolf_Giant",
        "Wolf_Mega",
        "Wolf_Mini",
        "Wolf_Shadow",
        "Wolf_Shaman",
        "Wolf_Summoner",
        "Wolf_Totem",
        
        // Skeleton names (actual asset names from game)
        "Characters/Wolf",
        "Characters/Wolf_Dark",
        "Characters/Wolf_White",
        "Characters/Wolf_Red",
        "Characters/DireWolf",
        "Characters/Fox",
        "Characters/Coyote",
        "Characters/Wolf_FestorsKin",
        "Characters/Wolf_NestorsKin",
        "Characters/Wolf_MestorsKin",
        "Characters/Wolf_Masked",
        "Characters/Wolf_Hammerer",
        "Characters/Wolf_Cruciferous",
        "Characters/Wolf_CruciferousRotten",
        "Characters/Wolf_TurretStatue",
        "Characters/Wolf_ShadowClone",
        "Characters/Wolf_Terracotta",
        "Characters/Wolf_TerracottaGiant",
        "Characters/Wolf_Lightning",
        "Characters/Wolf_Mage",
        "Characters/Wolf_Bomber",
        "Characters/Wolf_Archer"
    };

    private static bool s_wolfShouldExist = false;
    private static bool s_wolfRespawning = false;
    private static float s_wolfRespawnDelay = 0f;
    private static float s_wolfAnimHoldTimer = 0f;
    private const float WOLF_ANIM_HOLD_MIN = 0.15f;
    private static string s_wolfAttackAnimName = null;
    private static float s_wolfCombatTransitionTimer = 0f;
    private const float WOLF_COMBAT_TRANSITION_MIN = 0.7f;
    private const float WOLF_ROOM_TRANSITION_DELAY = 1.5f;

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
            s_wolfRespawnDelay = 0f;
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

        float wolfToPlayerAngle = Utils.GetAngle(wolf.transform.position, player.transform.position);
        float petDist = 1.2f;
        Vector3 petTarget = wolf.transform.position + new Vector3(
            Mathf.Cos(wolfToPlayerAngle * Mathf.Deg2Rad) * petDist,
            Mathf.Sin(wolfToPlayerAngle * Mathf.Deg2Rad) * petDist, 0f);

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

        float angle = Utils.GetAngle(player.transform.position, wolf.transform.position);
        player.state.facingAngle = angle;

        player.state.CURRENT_STATE = StateMachine.State.CustomAnimation;
        player.simpleSpineAnimator.Animate("pet-dog", 0, false);

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

    [Update]
    public static void UpdateFriendlyWolf(){
        if(!s_wolfShouldExist) return;
        if(s_wolfRespawning) return;
        if(PlayerFarming.Instance == null) return;

        if(s_wolfRespawnDelay > 0f){
            s_wolfRespawnDelay -= Time.deltaTime;
            if(s_wolfRespawnDelay <= 0f){
                s_wolfRespawnDelay = 0f;
                SpawnFriendlyWolfInternal(false);
            }
            return;
        }

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
            s_wolfRespawnDelay = WOLF_ROOM_TRANSITION_DELAY;
            UnityEngine.Debug.Log("[CheatMenu] Friendly wolf lost (room/scene change) - respawning after delay...");
        }
    }

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

            bool inDungeon = GameManager.IsDungeon(PlayerFarming.Location);
            bool wolfInCombat = false;

            if(inDungeon && WolfDungeonCombat){
                s_wolfAttackCooldown -= Time.deltaTime;
                s_wolfCombatTransitionTimer -= Time.deltaTime;

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
                        s_wolfCombatTransitionTimer = WOLF_COMBAT_TRANSITION_MIN;
                    }
                }

                if(s_wolfTargetEnemy == null){
                    s_wolfTargetEnemy = FindClosestEnemy(wolfPos, WOLF_DETECT_RANGE);
                }

                if(s_wolfTargetEnemy != null){
                    wolfInCombat = true;
                    Vector3 enemyPos = s_wolfTargetEnemy.transform.position;
                    float enemyDist = Vector3.Distance(wolfPos, enemyPos);

                    if(stateMachine != null){
                        float faceAngle = Utils.GetAngle(wolfPos, enemyPos);
                        stateMachine.facingAngle = faceAngle;
                        stateMachine.LookAngle = faceAngle;
                    }

                    if(s_wolfCombatTransitionTimer > 0f){
                        wolf.transform.position = Vector3.SmoothDamp(
                            wolfPos, enemyPos, ref s_wolfVelocity, WOLF_SMOOTH_TIME * 0.7f, WOLF_FOLLOW_SPEED * 1.3f, Time.deltaTime
                        );
                        SetWolfAnimation(spineAnim, "run", false);
                    } else {
                        string atkAnim = s_wolfAttackAnimName ?? "idle";

                        bool wasAttacking = s_wolfCurrentAnim == atkAnim || s_wolfCurrentAnim == "idle";
                        float effectiveRange = wasAttacking ? WOLF_ATTACK_RANGE * 1.5f : WOLF_ATTACK_RANGE;

                        if(enemyDist <= effectiveRange){
                            if(s_wolfAttackCooldown <= 0f){
                                s_wolfTargetEnemy.DealDamage(WOLF_ATTACK_DAMAGE, wolf.gameObject, wolfPos, false, Health.AttackTypes.Melee, false, (Health.AttackFlags)0);
                                s_wolfAttackCooldown = WOLF_ATTACK_COOLDOWN;

                                try {
                                    AudioManager.Instance.PlayOneShot("event:/dlc/env/dog/dog_basic_attack_bite", wolf.transform.position);
                                } catch { }

                                SetWolfAnimation(spineAnim, atkAnim, true);
                            } else {
                                if(s_wolfAttackCooldown < WOLF_ATTACK_COOLDOWN * 0.3f){
                                    SetWolfAnimation(spineAnim, "idle", false);
                                }
                            }
                            if(enemyDist > WOLF_ATTACK_RANGE){
                                wolf.transform.position = Vector3.SmoothDamp(
                                    wolfPos, enemyPos, ref s_wolfVelocity, WOLF_SMOOTH_TIME, WOLF_FOLLOW_SPEED * 0.5f, Time.deltaTime
                                );
                            } else {
                                s_wolfVelocity = Vector3.zero;
                            }
                        } else {
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

            if(!wolfInCombat){
                if(distance > WOLF_TELEPORT_DIST){
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

    // -- Additional Companion Spawn Methods ----------------------------------------

    private static List<Follower> s_companions = new List<Follower>();

    public static void SpawnWolfPack(){
        try {
            if(PlayerFarming.Instance == null){
                PlayNotification("Must be in game!");
                return;
            }

            Vector3 spawnPos = PlayerFarming.Instance.transform.position;
            int packSize = 3;

            for(int i = 0; i < packSize; i++){
                Vector3 offset = new Vector3(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-0.5f, 0.5f), 0);
                SpawnWolfAtPosition(spawnPos + offset);
            }

            PlayNotification($"Wolf pack ({packSize}) spawned!");
        } catch(Exception e){
            UnityEngine.Debug.LogWarning($"[CheatMenu] SpawnWolfPack error: {e.Message}");
            PlayNotification("Failed to spawn wolf pack!");
        }
    }

    private static void SpawnWolfAtPosition(Vector3 position){
        Interaction_WolfBase.WolfTarget = 1;
        Interaction_WolfBase.WolfCount = 0;
        Interaction_WolfBase.WolfFled = 0;
        Interaction_WolfBase.WolfDied = 0;

        Interaction_WolfBase.SpawnWolf(position, null, false, (Interaction_WolfBase wolf) => {
            if(wolf != null){
                wolf.CurrentState = Interaction_WolfBase.State.Animating;
                wolf.SecondaryInteractable = false;
            }
        });
    }

    public static void SpawnBabyWolf(){
        try {
            if(PlayerFarming.Instance == null){
                PlayNotification("Must be in game!");
                return;
            }

            // Spawn a smaller wolf (same method, just notification says baby)
            SpawnWolfAtPosition(PlayerFarming.Instance.transform.position);
            PlayNotification("Baby wolf spawned!");
        } catch(Exception e){
            UnityEngine.Debug.LogWarning($"[CheatMenu] SpawnBabyWolf error: {e.Message}");
            PlayNotification("Failed to spawn baby wolf!");
        }
    }
    
    public static void ChangeWolfSkin(){
        try {
            // First check if we have a friendly wolf
            Interaction_WolfBase wolf = FriendlyWolf;
            
            // If no friendly wolf, check if there's any wolf
            if(wolf == null){
                var wolves = Interaction_WolfBase.Wolfs;
                if(wolves != null && wolves.Count > 0){
                    wolf = wolves[0];
                }
            }
            
            if(wolf == null){
                PlayNotification("No wolf found! Spawn a wolf first.");
                return;
            }
            
            SkeletonAnimation spineAnim = wolf.Spine;
            if(spineAnim == null || spineAnim.skeleton == null || spineAnim.skeleton.Data == null){
                PlayNotification("Wolf not ready! Try again.");
                return;
            }
            
            var skeletonData = spineAnim.skeleton.Data;
            var skins = skeletonData.Skins;
            
            if(skins == null || skins.Count == 0){
                PlayNotification("No skins available for wolf!");
                return;
            }
            
            // Get ONLY the skin names that actually exist in the wolf skeleton
            // The friendly wolf only has 6 valid skins: default, Bomb, Boss, Normal, Normal_Up, Tunnel
            // Skins from enemy wolf types do NOT exist in the friendly wolf skeleton
            List<string> availableSkinNames = new List<string>();
            for(int i = 0; i < skins.Count; i++){
                var skin = skins.Items[i];
                if(skin != null && !string.IsNullOrEmpty(skin.Name)){
                    availableSkinNames.Add(skin.Name);
                }
            }
            
            if(availableSkinNames.Count == 0){
                PlayNotification("No skins available for wolf!");
                return;
            }
            
            UnityEngine.Debug.Log($"[CheatMenu] Available wolf skins ({availableSkinNames.Count}): {string.Join(", ", availableSkinNames)}");
            
            // Filter out broken/unwanted skins:
            // - "default" is invisible
            // - "Normal_Up" has broken sprites
            availableSkinNames.Remove("default");
            availableSkinNames.Remove("Normal_Up");
            
            if(availableSkinNames.Count == 0){
                PlayNotification("No valid skins available for wolf!");
                return;
            }
            
            UnityEngine.Debug.Log($"[CheatMenu] Filtered wolf skins ({availableSkinNames.Count}): {string.Join(", ", availableSkinNames)}");
            
            // Get current skin name to determine next skin
            string currentSkinName = spineAnim.skeleton.Skin?.Name ?? "";
            
            // Find current index in available skins list
            int currentIndex = -1;
            for(int i = 0; i < availableSkinNames.Count; i++){
                if(availableSkinNames[i] == currentSkinName){
                    currentIndex = i;
                    break;
                }
            }
            
            // If current skin not in our list, start from beginning (index 0)
            // FIX: Was incorrectly set to availableSkinNames.Count - 1
            if(currentIndex < 0) currentIndex = -1;
            
            // Move to next skin in our list
            int nextIndex = (currentIndex + 1) % availableSkinNames.Count;
            string nextSkinName = availableSkinNames[nextIndex];
            
            // Find the actual skin in skeleton data
            Spine.Skin nextSkin = null;
            for(int i = 0; i < skins.Count; i++){
                var skin = skins.Items[i];
                if(skin != null && skin.Name == nextSkinName){
                    nextSkin = skin;
                    break;
                }
            }
            
            if(nextSkin == null){
                nextSkin = skeletonData.FindSkin(nextSkinName);
            }
            
            if(nextSkin == null){
                // This skin doesn't exist in the skeleton - skip it and try next
                UnityEngine.Debug.LogWarning($"[CheatMenu] Skin '{nextSkinName}' not found in wolf skeleton, skipping...");
                
                // Find next valid skin by searching from current position
                for(int offset = 1; offset < availableSkinNames.Count; offset++){
                    int testIndex = (currentIndex + offset) % availableSkinNames.Count;
                    string testSkinName = availableSkinNames[testIndex];
                    
                    // Check if this skin exists
                    for(int i = 0; i < skins.Count; i++){
                        if(skins.Items[i] != null && skins.Items[i].Name == testSkinName){
                            nextSkinName = testSkinName;
                            nextSkin = skins.Items[i];
                            nextIndex = testIndex;
                            break;
                        }
                    }
                    if(nextSkin != null) break;
                }
                
                if(nextSkin == null){
                    PlayNotification("No valid skins found!");
                    return;
                }
            }
            
            // Apply the new skin
            spineAnim.skeleton.SetSkin(nextSkin);
            spineAnim.skeleton.SetSlotsToSetupPose();
            
            // Force skeleton to update immediately
            spineAnim.LateUpdate();
            
            // Re-apply current animation to refresh visuals
            if(!string.IsNullOrEmpty(s_wolfCurrentAnim)){
                spineAnim.AnimationState.SetAnimation(0, s_wolfCurrentAnim, s_wolfIsRunning);
            } else {
                // Reset to idle if no current animation
                spineAnim.AnimationState.SetAnimation(0, "idle", true);
            }
            
            // Update our tracking index
            s_wolfSkinIndex = nextIndex;
            
            PlayNotification($"Wolf skin changed to: {nextSkinName}");
            UnityEngine.Debug.Log($"[CheatMenu] Wolf skin changed to: {nextSkinName}");
            
        } catch(Exception e){
            UnityEngine.Debug.LogWarning($"[CheatMenu] ChangeWolfSkin error: {e.Message}");
            UnityEngine.Debug.LogWarning($"[CheatMenu] ChangeWolfSkin stack: {e.StackTrace}");
            PlayNotification("Failed to change wolf skin!");
        }
    }

    public static void SpawnCompanion(string skinName){
        try {
            if(PlayerFarming.Instance == null){
                PlayNotification("Must be in game!");
                return;
            }

            // Unlock the skin first
            DataManager.SetFollowerSkinUnlocked(skinName);

            // Create a new follower with the specified skin
            FollowerInfo followerInfo = FollowerInfo.NewCharacter(PlayerFarming.Location, skinName);

            Vector3 spawnPos = PlayerFarming.Instance.transform.position + Vector3.right * 2f;

            var spawnedFollower = FollowerManager.SpawnCopyFollower(followerInfo, spawnPos, PlayerFarming.Instance.transform.parent, PlayerFarming.Location);

            if(spawnedFollower != null && spawnedFollower.Follower != null){
                var follower = spawnedFollower.Follower;
                s_companions.Add(follower);

                UnityEngine.Debug.Log($"[CheatMenu] {skinName} companion spawned!");
                PlayNotification($"{skinName} companion spawned!");
            } else {
                PlayNotification($"Failed to spawn {skinName}!");
            }
        } catch(Exception e){
            UnityEngine.Debug.LogWarning($"[CheatMenu] SpawnCompanion ({skinName}) error: {e.Message}");
            PlayNotification($"Failed to spawn {skinName} companion!");
        }
    }

    public static void DismissAllCompanions(){
        try {
            int count = 0;
            foreach(var companion in s_companions){
                if(companion != null){
                    UnityEngine.Object.Destroy(companion.gameObject);
                    count++;
                }
            }
            s_companions.Clear();

            // Also dismiss wolves
            DismissFriendlyWolf();

            PlayNotification($"Dismissed {count} companions!");
        } catch(Exception e){
            UnityEngine.Debug.LogWarning($"[CheatMenu] DismissAllCompanions error: {e.Message}");
            PlayNotification("Failed to dismiss companions!");
        }
    }
}

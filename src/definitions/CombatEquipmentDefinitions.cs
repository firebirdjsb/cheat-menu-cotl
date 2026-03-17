using System;
using System.Collections.Generic;
using UnityEngine;
using HarmonyLib;

namespace CheatMenu;

/// <summary>
/// Definition class containing cheats for real-time combat equipment management.
/// Allows changing weapons, relics, curses, and tarot cards while in a dungeon.
/// </summary>
[CheatCategory(CheatCategoryEnum.COMBAT)]
public class CombatEquipmentDefinitions : IDefinition {

    // State for cycling
    private static int s_currentWeaponIndex = 0;
    private static int s_currentRelicIndex = 0;
    private static int s_currentCurseIndex = 0;
    
    // Slider values for GUI - accessible from outside
    public static int WeaponLevelSliderValue = 1;
    public static int CurseLevelSliderValue = 1;

    private static bool IsInDungeon() {
        try {
            return PlayerFarming.Instance != null && PlayerFarming.Location != FollowerLocation.Base;
        } catch {
            return false;
        }
    }

    // ==================== PLAYER SPEED ====================

    private static float s_originalRunSpeed = -1f;

    [CheatDetails("Player Speed x2", "Speed x2 (OFF)", "Speed x2 (ON)", "Doubles the player's movement speed without affecting the world", true)]
    public static void PlayerSpeedDouble(bool flag){
        try {
            if(PlayerFarming.Instance != null){
                var controller = PlayerFarming.Instance.playerController;
                if(flag){
                    if(s_originalRunSpeed < 0f){
                        s_originalRunSpeed = controller.DefaultRunSpeed;
                    }
                    controller.RunSpeed = s_originalRunSpeed * 2f;
                    controller.DefaultRunSpeed = s_originalRunSpeed * 2f;
                } else {
                    if(s_originalRunSpeed >= 0f){
                        controller.RunSpeed = s_originalRunSpeed;
                        controller.DefaultRunSpeed = s_originalRunSpeed;
                    }
                    s_originalRunSpeed = -1f;
                }
            }
            CultUtils.PlayNotification(flag ? "Player speed x2!" : "Player speed normal!");
        } catch(Exception e){
            Debug.LogWarning($"Failed to set player speed: {e.Message}");
            CultUtils.PlayNotification("Failed to toggle player speed!");
        }
    }

    // ==================== WEAPON SUBGROUP ====================

    [CheatDetails("Next Weapon", " Cycles to the next weapon", subGroup: "Weapon")]
    public static void NextWeapon() {
        if (!IsInDungeon()) {
            CultUtils.PlayNotification("Must be in a dungeon!");
            return;
        }
        try {
            var weapons = GetWeapons();
            s_currentWeaponIndex = (s_currentWeaponIndex + 1) % weapons.Count;
            SetWeapon(weapons[s_currentWeaponIndex]);
        } catch (Exception e) {
            Debug.LogWarning($"[CheatMenu] NextWeapon: {e.Message}");
        }
    }

    [CheatDetails("Prev Weapon", " Cycles to the previous weapon", subGroup: "Weapon")]
    public static void PrevWeapon() {
        if (!IsInDungeon()) {
            CultUtils.PlayNotification("Must be in a dungeon!");
            return;
        }
        try {
            var weapons = GetWeapons();
            s_currentWeaponIndex = (s_currentWeaponIndex - 1 + weapons.Count) % weapons.Count;
            SetWeapon(weapons[s_currentWeaponIndex]);
        } catch (Exception e) {
            Debug.LogWarning($"[CheatMenu] PrevWeapon: {e.Message}");
        }
    }

    [CheatDetails("Apply Weapon Level", " Applies weapon with current slider level", subGroup: "Weapon")]
    public static void ApplyWeaponLevel() {
        if (!IsInDungeon()) {
            CultUtils.PlayNotification("Must be in a dungeon!");
            return;
        }
        try {
            var weapons = GetWeapons();
            if (s_currentWeaponIndex >= weapons.Count) s_currentWeaponIndex = 0;
            SetWeapon(weapons[s_currentWeaponIndex]);
        } catch (Exception e) {
            Debug.LogWarning($"[CheatMenu] ApplyWeaponLevel: {e.Message}");
        }
    }

    private static List<EquipmentType> GetWeapons() {
        List<EquipmentType> weapons = new List<EquipmentType> {
            EquipmentType.Sword, EquipmentType.Axe, EquipmentType.Dagger,
            EquipmentType.Gauntlet, EquipmentType.Hammer, EquipmentType.Blunderbuss,
            EquipmentType.Shield
        };
        if (CultUtils.HasMajorDLC()) {
            weapons.Add(EquipmentType.Chain);
        }
        return weapons;
    }

    private static void SetWeapon(EquipmentType weapon) {
        try {
            if (PlayerFarming.Instance?.playerWeapon != null) {
                PlayerFarming.Instance.playerWeapon.SetWeapon(weapon, WeaponLevelSliderValue);
                PlayerFarming.Instance.currentWeaponLevel = WeaponLevelSliderValue;
                CultUtils.PlayNotification($"{weapon} (Lv.{WeaponLevelSliderValue})");
            }
        } catch (Exception e) {
            Debug.LogWarning($"[CheatMenu] SetWeapon: {e.Message}");
        }
    }

    // ==================== RELIC SUBGROUP ====================

    [CheatDetails("Next Relic", " Cycles to the next relic", subGroup: "Relic")]
    public static void NextRelic() {
        if (!IsInDungeon()) {
            CultUtils.PlayNotification("Must be in a dungeon!");
            return;
        }
        try {
            var relics = GetRelics();
            s_currentRelicIndex = (s_currentRelicIndex + 1) % relics.Count;
            SetRelic(relics[s_currentRelicIndex]);
        } catch (Exception e) {
            Debug.LogWarning($"[CheatMenu] NextRelic: {e.Message}");
        }
    }

    [CheatDetails("Prev Relic", " Cycles to the previous relic", subGroup: "Relic")]
    public static void PrevRelic() {
        if (!IsInDungeon()) {
            CultUtils.PlayNotification("Must be in a dungeon!");
            return;
        }
        try {
            var relics = GetRelics();
            s_currentRelicIndex = (s_currentRelicIndex - 1 + relics.Count) % relics.Count;
            SetRelic(relics[s_currentRelicIndex]);
        } catch (Exception e) {
            Debug.LogWarning($"[CheatMenu] PrevRelic: {e.Message}");
        }
    }

    private static List<RelicType> GetRelics() {
        return new List<RelicType> {
            RelicType.LightningStrike, RelicType.SpawnTentacle, RelicType.FreezeTime,
            RelicType.SpawnBombs, RelicType.GungeonBlank, RelicType.FiftyFiftyGamble,
            RelicType.FillUpFervour, RelicType.PoisonAll, RelicType.SpawnDemon,
            RelicType.ProjectileRing, RelicType.RandomEnemyIntoCritter, RelicType.RerollWeapon,
            RelicType.RerollCurse
        };
    }

    private static void SetRelic(RelicType relic) {
        try {
            if (PlayerFarming.Instance?.playerRelic != null) {
                RelicData data = EquipmentManager.GetRelicData(relic);
                if (data != null) {
                    PlayerFarming.Instance.playerRelic.EquipRelic(data, false, false);
                    PlayerFarming.Instance.currentRelicType = relic;
                    CultUtils.PlayNotification($"Relic: {relic}");
                }
            }
        } catch (Exception e) {
            Debug.LogWarning($"[CheatMenu] SetRelic: {e.Message}");
        }
    }

    // ==================== CURSE SUBGROUP ====================

    [CheatDetails("Next Curse", " Cycles to the next curse", subGroup: "Curse")]
    public static void NextCurse() {
        if (!IsInDungeon()) {
            CultUtils.PlayNotification("Must be in a dungeon!");
            return;
        }
        try {
            var curses = GetCurses();
            s_currentCurseIndex = (s_currentCurseIndex + 1) % curses.Count;
            SetCurse(curses[s_currentCurseIndex]);
        } catch (Exception e) {
            Debug.LogWarning($"[CheatMenu] NextCurse: {e.Message}");
        }
    }

    [CheatDetails("Prev Curse", " Cycles to the previous curse", subGroup: "Curse")]
    public static void PrevCurse() {
        if (!IsInDungeon()) {
            CultUtils.PlayNotification("Must be in a dungeon!");
            return;
        }
        try {
            var curses = GetCurses();
            s_currentCurseIndex = (s_currentCurseIndex - 1 + curses.Count) % curses.Count;
            SetCurse(curses[s_currentCurseIndex]);
        } catch (Exception e) {
            Debug.LogWarning($"[CheatMenu] PrevCurse: {e.Message}");
        }
    }

    [CheatDetails("Apply Curse Level", " Applies curse with current slider level", subGroup: "Curse")]
    public static void ApplyCurseLevel() {
        if (!IsInDungeon()) {
            CultUtils.PlayNotification("Must be in a dungeon!");
            return;
        }
        try {
            var curses = GetCurses();
            if (s_currentCurseIndex >= curses.Count) s_currentCurseIndex = 0;
            SetCurse(curses[s_currentCurseIndex]);
        } catch (Exception e) {
            Debug.LogWarning($"[CheatMenu] ApplyCurseLevel: {e.Message}");
        }
    }

    private static List<EquipmentType> GetCurses() {
        return new List<EquipmentType> {
            EquipmentType.Fireball, EquipmentType.Fireball_Swarm, EquipmentType.Fireball_Triple,
            EquipmentType.Fireball_Charm, EquipmentType.ProjectileAOE, EquipmentType.ProjectileAOE_GoopTrail,
            EquipmentType.Tentacles, EquipmentType.Tentacles_Circular, EquipmentType.MegaSlash,
            EquipmentType.EnemyBlast, EquipmentType.Teleport, EquipmentType.Barrier
        };
    }

    private static void SetCurse(EquipmentType curse) {
        try {
            if (PlayerFarming.Instance?.playerSpells != null) {
                PlayerFarming.Instance.playerSpells.SetSpell(curse, CurseLevelSliderValue);
                PlayerFarming.Instance.currentCurse = curse;
                PlayerFarming.Instance.currentCurseLevel = CurseLevelSliderValue;
                CultUtils.PlayNotification($"{curse} (Lv.{CurseLevelSliderValue})");
            }
        } catch (Exception e) {
            Debug.LogWarning($"[CheatMenu] SetCurse: {e.Message}");
        }
    }

    // ==================== TAROT SELECTOR - ALL CARDS ====================

    // Page 1: Health & Hearts
    [CheatDetails("Tarot: Hearts1", " Adds Hearts1 tarot card", subGroup: "Tarot")]
    public static void AddTarotHearts1() { AddTarotCard(TarotCards.Card.Hearts1); }
    [CheatDetails("Tarot: Hearts2", " Adds Hearts2 tarot card", subGroup: "Tarot")]
    public static void AddTarotHearts2() { AddTarotCard(TarotCards.Card.Hearts2); }
    [CheatDetails("Tarot: Hearts3", " Adds Hearts3 tarot card", subGroup: "Tarot")]
    public static void AddTarotHearts3() { AddTarotCard(TarotCards.Card.Hearts3); }
    [CheatDetails("Tarot: Lovers1", " Adds Lovers1 tarot card", subGroup: "Tarot")]
    public static void AddTarotLovers1() { AddTarotCard(TarotCards.Card.Lovers1); }
    [CheatDetails("Tarot: Lovers2", " Adds Lovers2 tarot card", subGroup: "Tarot")]
    public static void AddTarotLovers2() { AddTarotCard(TarotCards.Card.Lovers2); }
    [CheatDetails("Tarot: Sun", " Adds Sun tarot card", subGroup: "Tarot")]
    public static void AddTarotSun() { AddTarotCard(TarotCards.Card.Sun); }
    [CheatDetails("Tarot: Moon", " Adds Moon tarot card", subGroup: "Tarot")]
    public static void AddTarotMoon() { AddTarotCard(TarotCards.Card.Moon); }
    [CheatDetails("Tarot: DiseasedHeart", " Adds DiseasedHeart tarot card", subGroup: "Tarot")]
    public static void AddTarotDiseasedHeart() { AddTarotCard(TarotCards.Card.DiseasedHeart); }
    [CheatDetails("Tarot: GiftFromBelow", " Adds GiftFromBelow tarot card", subGroup: "Tarot")]
    public static void AddTarotGiftFromBelow() { AddTarotCard(TarotCards.Card.GiftFromBelow); }
    [CheatDetails("Tarot: Spider", " Adds Spider tarot card", subGroup: "Tarot")]
    public static void AddTarotSpider() { AddTarotCard(TarotCards.Card.Spider); }

    // Page 2: Stats & Boosts
    [CheatDetails("Tarot: MovementSpeed", " Adds MovementSpeed tarot card", subGroup: "Tarot")]
    public static void AddTarotMovementSpeed() { AddTarotCard(TarotCards.Card.MovementSpeed); }
    [CheatDetails("Tarot: AttackRate", " Adds AttackRate tarot card", subGroup: "Tarot")]
    public static void AddTarotAttackRate() { AddTarotCard(TarotCards.Card.AttackRate); }
    [CheatDetails("Tarot: IncreasedDamage", " Adds IncreasedDamage tarot card", subGroup: "Tarot")]
    public static void AddTarotIncreasedDamage() { AddTarotCard(TarotCards.Card.IncreasedDamage); }
    [CheatDetails("Tarot: IncreaseBlackSoulsDrop", " Adds IncreaseBlackSoulsDrop tarot card", subGroup: "Tarot")]
    public static void AddTarotIncreaseBlackSoulsDrop() { AddTarotCard(TarotCards.Card.IncreaseBlackSoulsDrop); }
    [CheatDetails("Tarot: HealChance", " Adds HealChance tarot card", subGroup: "Tarot")]
    public static void AddTarotHealChance() { AddTarotCard(TarotCards.Card.HealChance); }
    [CheatDetails("Tarot: NegateDamageChance", " Adds NegateDamageChance tarot card", subGroup: "Tarot")]
    public static void AddTarotNegateDamageChance() { AddTarotCard(TarotCards.Card.NegateDamageChance); }
    [CheatDetails("Tarot: EyeOfWeakness", " Adds EyeOfWeakness tarot card", subGroup: "Tarot")]
    public static void AddTarotEyeOfWeakness() { AddTarotCard(TarotCards.Card.EyeOfWeakness); }
    [CheatDetails("Tarot: RabbitFoot", " Adds RabbitFoot tarot card", subGroup: "Tarot")]
    public static void AddTarotRabbitFoot() { AddTarotCard(TarotCards.Card.RabbitFoot); }

    // Page 3: Special Effects
    [CheatDetails("Tarot: DeathsDoor", " Adds DeathsDoor tarot card", subGroup: "Tarot")]
    public static void AddTarotDeathsDoor() { AddTarotCard(TarotCards.Card.DeathsDoor); }
    [CheatDetails("Tarot: TheDeal", " Adds TheDeal tarot card", subGroup: "Tarot")]
    public static void AddTarotTheDeal() { AddTarotCard(TarotCards.Card.TheDeal); }
    [CheatDetails("Tarot: Telescope", " Adds Telescope tarot card", subGroup: "Tarot")]
    public static void AddTarotTelescope() { AddTarotCard(TarotCards.Card.Telescope); }
    [CheatDetails("Tarot: HandsOfRage", " Adds HandsOfRage tarot card", subGroup: "Tarot")]
    public static void AddTarotHandsOfRage() { AddTarotCard(TarotCards.Card.HandsOfRage); }
    [CheatDetails("Tarot: NaturesGift", " Adds NaturesGift tarot card", subGroup: "Tarot")]
    public static void AddTarotNaturesGift() { AddTarotCard(TarotCards.Card.NaturesGift); }
    [CheatDetails("Tarot: Skull", " Adds Skull tarot card", subGroup: "Tarot")]
    public static void AddTarotSkull() { AddTarotCard(TarotCards.Card.Skull); }
    [CheatDetails("Tarot: Potion", " Adds Potion tarot card", subGroup: "Tarot")]
    public static void AddTarotPotion() { AddTarotCard(TarotCards.Card.Potion); }
    [CheatDetails("Tarot: Arrows", " Adds Arrows tarot card", subGroup: "Tarot")]
    public static void AddTarotArrows() { AddTarotCard(TarotCards.Card.Arrows); }

    // Page 4: Bombs & Traps
    [CheatDetails("Tarot: BombOnRoll", " Adds BombOnRoll tarot card", subGroup: "Tarot")]
    public static void AddTarotBombOnRoll() { AddTarotCard(TarotCards.Card.BombOnRoll); }
    [CheatDetails("Tarot: GoopOnDamaged", " Adds GoopOnDamaged tarot card", subGroup: "Tarot")]
    public static void AddTarotGoopOnDamaged() { AddTarotCard(TarotCards.Card.GoopOnDamaged); }
    [CheatDetails("Tarot: GoopOnRoll", " Adds GoopOnRoll tarot card", subGroup: "Tarot")]
    public static void AddTarotGoopOnRoll() { AddTarotCard(TarotCards.Card.GoopOnRoll); }
    [CheatDetails("Tarot: PoisonImmune", " Adds PoisonImmune tarot card", subGroup: "Tarot")]
    public static void AddTarotPoisonImmune() { AddTarotCard(TarotCards.Card.PoisonImmune); }
    [CheatDetails("Tarot: DamageOnRoll", " Adds DamageOnRoll tarot card", subGroup: "Tarot")]
    public static void AddTarotDamageOnRoll() { AddTarotCard(TarotCards.Card.DamageOnRoll); }
    [CheatDetails("Tarot: HealTwiceAmount", " Adds HealTwiceAmount tarot card", subGroup: "Tarot")]
    public static void AddTarotHealTwiceAmount() { AddTarotCard(TarotCards.Card.HealTwiceAmount); }
    [CheatDetails("Tarot: InvincibleWhileHealing", " Adds InvincibleWhileHealing tarot card", subGroup: "Tarot")]
    public static void AddTarotInvincibleWhileHealing() { AddTarotCard(TarotCards.Card.InvincibleWhileHealing); }
    [CheatDetails("Tarot: AmmoEfficient", " Adds AmmoEfficient tarot card", subGroup: "Tarot")]
    public static void AddTarotAmmoEfficient() { AddTarotCard(TarotCards.Card.AmmoEfficient); }

    // Page 5: Resource & Cooldowns
    [CheatDetails("Tarot: BlackSoulAutoRecharge", " Adds BlackSoulAutoRecharge tarot card", subGroup: "Tarot")]
    public static void AddTarotBlackSoulAutoRecharge() { AddTarotCard(TarotCards.Card.BlackSoulAutoRecharge); }
    [CheatDetails("Tarot: BlackSoulOnDamage", " Adds BlackSoulOnDamage tarot card", subGroup: "Tarot")]
    public static void AddTarotBlackSoulOnDamage() { AddTarotCard(TarotCards.Card.BlackSoulOnDamage); }
    [CheatDetails("Tarot: NeptunesCurse", " Adds NeptunesCurse tarot card", subGroup: "Tarot")]
    public static void AddTarotNeptunesCurse() { AddTarotCard(TarotCards.Card.NeptunesCurse); }
    [CheatDetails("Tarot: HoldToHeal", " Adds HoldToHeal tarot card", subGroup: "Tarot")]
    public static void AddTarotHoldToHeal() { AddTarotCard(TarotCards.Card.HoldToHeal); }
    [CheatDetails("Tarot: MoreRelics", " Adds MoreRelics tarot card", subGroup: "Tarot")]
    public static void AddTarotMoreRelics() { AddTarotCard(TarotCards.Card.MoreRelics); }
    [CheatDetails("Tarot: TentacleOnDamaged", " Adds TentacleOnDamaged tarot card", subGroup: "Tarot")]
    public static void AddTarotTentacleOnDamaged() { AddTarotCard(TarotCards.Card.TentacleOnDamaged); }
    [CheatDetails("Tarot: InvincibilityPerRoom", " Adds InvincibilityPerRoom tarot card", subGroup: "Tarot")]
    public static void AddTarotInvincibilityPerRoom() { AddTarotCard(TarotCards.Card.InvincibilityPerRoom); }
    [CheatDetails("Tarot: BombOnDamaged", " Adds BombOnDamaged tarot card", subGroup: "Tarot")]
    public static void AddTarotBombOnDamaged() { AddTarotCard(TarotCards.Card.BombOnDamaged); }

    // Page 6: Immunity & Movement
    [CheatDetails("Tarot: ImmuneToTraps", " Adds ImmuneToTraps tarot card", subGroup: "Tarot")]
    public static void AddTarotImmuneToTraps() { AddTarotCard(TarotCards.Card.ImmuneToTraps); }
    [CheatDetails("Tarot: ImmuneToProjectiles", " Adds ImmuneToProjectiles tarot card", subGroup: "Tarot")]
    public static void AddTarotImmuneToProjectiles() { AddTarotCard(TarotCards.Card.ImmuneToProjectiles); }
    [CheatDetails("Tarot: WalkThroughBlocks", " Adds WalkThroughBlocks tarot card", subGroup: "Tarot")]
    public static void AddTarotWalkThroughBlocks() { AddTarotCard(TarotCards.Card.WalkThroughBlocks); }
    [CheatDetails("Tarot: DecreaseRelicCharge", " Adds DecreaseRelicCharge tarot card", subGroup: "Tarot")]
    public static void AddTarotDecreaseRelicCharge() { AddTarotCard(TarotCards.Card.DecreaseRelicCharge); }
    [CheatDetails("Tarot: AdventureMapFreedom", " Adds AdventureMapFreedom tarot card", subGroup: "Tarot")]
    public static void AddTarotAdventureMapFreedom() { AddTarotCard(TarotCards.Card.AdventureMapFreedom); }
    [CheatDetails("Tarot: Recycle", " Adds Recycle tarot card", subGroup: "Tarot")]
    public static void AddTarotRecycle() { AddTarotCard(TarotCards.Card.Recycle); }
    [CheatDetails("Tarot: StrikeBack", " Adds StrikeBack tarot card", subGroup: "Tarot")]
    public static void AddTarotStrikeBack() { AddTarotCard(TarotCards.Card.StrikeBack); }
    [CheatDetails("Tarot: SurpriseAttack", " Adds SurpriseAttack tarot card", subGroup: "Tarot")]
    public static void AddTarotSurpriseAttack() { AddTarotCard(TarotCards.Card.SurpriseAttack); }

    // Page 7: Boss & Special
    [CheatDetails("Tarot: BossHeal", " Adds BossHeal tarot card", subGroup: "Tarot")]
    public static void AddTarotBossHeal() { AddTarotCard(TarotCards.Card.BossHeal); }
    [CheatDetails("Tarot: NoCorruption", " Adds NoCorruption tarot card", subGroup: "Tarot")]
    public static void AddTarotNoCorruption() { AddTarotCard(TarotCards.Card.NoCorruption); }
    [CheatDetails("Tarot: Sin", " Adds Sin tarot card", subGroup: "Tarot")]
    public static void AddTarotSin() { AddTarotCard(TarotCards.Card.Sin); }
    [CheatDetails("Tarot: ExtraMove", " Adds ExtraMove tarot card", subGroup: "Tarot")]
    public static void AddTarotExtraMove() { AddTarotCard(TarotCards.Card.ExtraMove); }
    [CheatDetails("Tarot: ShuffleNode", " Adds ShuffleNode tarot card", subGroup: "Tarot")]
    public static void AddTarotShuffleNode() { AddTarotCard(TarotCards.Card.ShuffleNode); }

    // Page 8: Corrupted Cards
    [CheatDetails("Tarot: CorruptedBombsAndHealth", " Adds CorruptedBombsAndHealth tarot card", subGroup: "Tarot")]
    public static void AddTarotCorruptedBombsAndHealth() { AddTarotCard(TarotCards.Card.CorruptedBombsAndHealth); }
    [CheatDetails("Tarot: CorruptedHeavy", " Adds CorruptedHeavy tarot card", subGroup: "Tarot")]
    public static void AddTarotCorruptedHeavy() { AddTarotCard(TarotCards.Card.CorruptedHeavy); }
    [CheatDetails("Tarot: CorruptedTradeOff", " Adds CorruptedTradeOff tarot card", subGroup: "Tarot")]
    public static void AddTarotCorruptedTradeOff() { AddTarotCard(TarotCards.Card.CorruptedTradeOff); }
    [CheatDetails("Tarot: CorruptedBlackHeartForRelic", " Adds CorruptedBlackHeartForRelic tarot card", subGroup: "Tarot")]
    public static void AddTarotCorruptedBlackHeartForRelic() { AddTarotCard(TarotCards.Card.CorruptedBlackHeartForRelic); }
    [CheatDetails("Tarot: CorruptedHealForRelic", " Adds CorruptedHealForRelic tarot card", subGroup: "Tarot")]
    public static void AddTarotCorruptedHealForRelic() { AddTarotCard(TarotCards.Card.CorruptedHealForRelic); }
    [CheatDetails("Tarot: CorruptedFullCorruption", " Adds CorruptedFullCorruption tarot card", subGroup: "Tarot")]
    public static void AddTarotCorruptedFullCorruption() { AddTarotCard(TarotCards.Card.CorruptedFullCorruption); }
    [CheatDetails("Tarot: CorruptedPoisonCoins", " Adds CorruptedPoisonCoins tarot card", subGroup: "Tarot")]
    public static void AddTarotCorruptedPoisonCoins() { AddTarotCard(TarotCards.Card.CorruptedPoisonCoins); }
    [CheatDetails("Tarot: CorruptedRelicCharge", " Adds CorruptedRelicCharge tarot card", subGroup: "Tarot")]
    public static void AddTarotCorruptedRelicCharge() { AddTarotCard(TarotCards.Card.CorruptedRelicCharge); }
    [CheatDetails("Tarot: CorruptedGoopyTrail", " Adds CorruptedGoopyTrail tarot card", subGroup: "Tarot")]
    public static void AddTarotCorruptedGoopyTrail() { AddTarotCard(TarotCards.Card.CorruptedGoopyTrail); }

    // Page 9: Weapon & Curse Tarots
    [CheatDetails("Tarot: Sword", " Adds Sword tarot card", subGroup: "Tarot")]
    public static void AddTarotSword() { AddTarotCard(TarotCards.Card.Sword); }
    [CheatDetails("Tarot: Dagger", " Adds Dagger tarot card", subGroup: "Tarot")]
    public static void AddTarotDagger() { AddTarotCard(TarotCards.Card.Dagger); }
    [CheatDetails("Tarot: Axe", " Adds Axe tarot card", subGroup: "Tarot")]
    public static void AddTarotAxe() { AddTarotCard(TarotCards.Card.Axe); }
    [CheatDetails("Tarot: Blunderbuss", " Adds Blunderbuss tarot card", subGroup: "Tarot")]
    public static void AddTarotBlunderbuss() { AddTarotCard(TarotCards.Card.Blunderbuss); }
    [CheatDetails("Tarot: Hammer", " Adds Hammer tarot card", subGroup: "Tarot")]
    public static void AddTarotHammer() { AddTarotCard(TarotCards.Card.Hammer); }
    [CheatDetails("Tarot: Gauntlet", " Adds Gauntlet tarot card", subGroup: "Tarot")]
    public static void AddTarotGauntlet() { AddTarotCard(TarotCards.Card.Gauntlet); }
    [CheatDetails("Tarot: Fireball", " Adds Fireball tarot card", subGroup: "Tarot")]
    public static void AddTarotFireball() { AddTarotCard(TarotCards.Card.Fireball); }
    [CheatDetails("Tarot: Tripleshot", " Adds Tripleshot tarot card", subGroup: "Tarot")]
    public static void AddTarotTripleshot() { AddTarotCard(TarotCards.Card.Tripleshot); }
    [CheatDetails("Tarot: Tentacles", " Adds Tentacles tarot card", subGroup: "Tarot")]
    public static void AddTarotTentacles() { AddTarotCard(TarotCards.Card.Tentacles); }
    [CheatDetails("Tarot: EnemyBlast", " Adds EnemyBlast tarot card", subGroup: "Tarot")]
    public static void AddTarotEnemyBlast() { AddTarotCard(TarotCards.Card.EnemyBlast); }
    [CheatDetails("Tarot: Vortex", " Adds Vortex tarot card", subGroup: "Tarot")]
    public static void AddTarotVortex() { AddTarotCard(TarotCards.Card.Vortex); }

    // Page 10: Co-op & DLC
    [CheatDetails("Tarot: CoopBetterTogether", " Adds CoopBetterTogether tarot card", subGroup: "Tarot")]
    public static void AddTarotCoopBetterTogether() { AddTarotCard(TarotCards.Card.CoopBetterTogether); }
    [CheatDetails("Tarot: CoopBetterApart", " Adds CoopBetterApart tarot card", subGroup: "Tarot")]
    public static void AddTarotCoopBetterApart() { AddTarotCard(TarotCards.Card.CoopBetterApart); }
    [CheatDetails("Tarot: CoopBonded", " Adds CoopBonded tarot card", subGroup: "Tarot")]
    public static void AddTarotCoopBonded() { AddTarotCard(TarotCards.Card.CoopBonded); }
    [CheatDetails("Tarot: CoopGoodTiming", " Adds CoopGoodTiming tarot card", subGroup: "Tarot")]
    public static void AddTarotCoopGoodTiming() { AddTarotCard(TarotCards.Card.CoopGoodTiming); }
    [CheatDetails("Tarot: CoopExplosive", " Adds CoopExplosive tarot card", subGroup: "Tarot")]
    public static void AddTarotCoopExplosive() { AddTarotCard(TarotCards.Card.CoopExplosive); }
    [CheatDetails("Tarot: FrostHeart", " Adds FrostHeart tarot card (DLC)", subGroup: "Tarot")]
    public static void AddTarotFrostHeart() { AddTarotCard(TarotCards.Card.FrostHeart); }
    [CheatDetails("Tarot: FlameHeart", " Adds FlameHeart tarot card (DLC)", subGroup: "Tarot")]
    public static void AddTarotFlameHeart() { AddTarotCard(TarotCards.Card.FlameHeart); }
    [CheatDetails("Tarot: EasyMoney", " Adds EasyMoney tarot card (DLC)", subGroup: "Tarot")]
    public static void AddTarotEasyMoney() { AddTarotCard(TarotCards.Card.EasyMoney); }
    [CheatDetails("Tarot: HighRoller", " Adds HighRoller tarot card (DLC)", subGroup: "Tarot")]
    public static void AddTarotHighRoller() { AddTarotCard(TarotCards.Card.HighRoller); }
    [CheatDetails("Tarot: FrostedEnemies", " Adds FrostedEnemies tarot card (DLC)", subGroup: "Tarot")]
    public static void AddTarotFrostedEnemies() { AddTarotCard(TarotCards.Card.FrostedEnemies); }
    [CheatDetails("Tarot: CursedIce", " Adds CursedIce tarot card (DLC)", subGroup: "Tarot")]
    public static void AddTarotCursedIce() { AddTarotCard(TarotCards.Card.CursedIce); }
    [CheatDetails("Tarot: LastChance", " Adds LastChance tarot card (DLC)", subGroup: "Tarot")]
    public static void AddTarotLastChance() { AddTarotCard(TarotCards.Card.LastChance); }
    [CheatDetails("Tarot: Joker", " Adds Joker tarot card (DLC)", subGroup: "Tarot")]
    public static void AddTarotJoker() { AddTarotCard(TarotCards.Card.Joker); }

    // Page 11: Mutated & Special DLC
    [CheatDetails("Tarot: MutatedNegateHit", " Adds MutatedNegateHit tarot card", subGroup: "Tarot")]
    public static void AddTarotMutatedNegateHit() { AddTarotCard(TarotCards.Card.MutatedNegateHit); }
    [CheatDetails("Tarot: MutatedDropRotburn", " Adds MutatedDropRotburn tarot card", subGroup: "Tarot")]
    public static void AddTarotMutatedDropRotburn() { AddTarotCard(TarotCards.Card.MutatedDropRotburn); }
    [CheatDetails("Tarot: MutatedFreezeOnHit", " Adds MutatedFreezeOnHit tarot card", subGroup: "Tarot")]
    public static void AddTarotMutatedFreezeOnHit() { AddTarotCard(TarotCards.Card.MutatedFreezeOnHit); }
    [CheatDetails("Tarot: MutatedResurrectFullHealth", " Adds MutatedResurrectFullHealth tarot card", subGroup: "Tarot")]
    public static void AddTarotMutatedResurrectFullHealth() { AddTarotCard(TarotCards.Card.MutatedResurrectFullHealth); }
    [CheatDetails("Tarot: MutatedInvincibility", " Adds MutatedInvincibility tarot card", subGroup: "Tarot")]
    public static void AddTarotMutatedInvincibility() { AddTarotCard(TarotCards.Card.MutatedInvincibility); }
    [CheatDetails("Tarot: MutatedSpawnRotDemons", " Adds MutatedSpawnRotDemons tarot card", subGroup: "Tarot")]
    public static void AddTarotMutatedSpawnRotDemons() { AddTarotCard(TarotCards.Card.MutatedSpawnRotDemons); }
    [CheatDetails("Tarot: EmptyFervourCritical", " Adds EmptyFervourCritical tarot card (DLC)", subGroup: "Tarot")]
    public static void AddTarotEmptyFervourCritical() { AddTarotCard(TarotCards.Card.EmptyFervourCritical); }
    [CheatDetails("Tarot: KillEnemiesOnResurrect", " Adds KillEnemiesOnResurrect tarot card (DLC)", subGroup: "Tarot")]
    public static void AddTarotKillEnemiesOnResurrect() { AddTarotCard(TarotCards.Card.KillEnemiesOnResurrect); }
    [CheatDetails("Tarot: RoomEnterCritter", " Adds RoomEnterCritter tarot card (DLC)", subGroup: "Tarot")]
    public static void AddTarotRoomEnterCritter() { AddTarotCard(TarotCards.Card.RoomEnterCritter); }
    [CheatDetails("Tarot: HitKillEnemy", " Adds HitKillEnemy tarot card (DLC)", subGroup: "Tarot")]
    public static void AddTarotHitKillEnemy() { AddTarotCard(TarotCards.Card.HitKillEnemy); }
    [CheatDetails("Tarot: SummonGhost", " Adds SummonGhost tarot card (DLC)", subGroup: "Tarot")]
    public static void AddTarotSummonGhost() { AddTarotCard(TarotCards.Card.SummonGhost); }
    [CheatDetails("Tarot: HeartTarotDrawn", " Adds HeartTarotDrawn tarot card (DLC)", subGroup: "Tarot")]
    public static void AddTarotHeartTarotDrawn() { AddTarotCard(TarotCards.Card.HeartTarotDrawn); }

    // Clear All
    [CheatDetails("Clear All Tarot", " Removes all tarot cards", subGroup: "Tarot")]
    public static void ClearAllTarot() {
        if (!IsInDungeon()) {
            CultUtils.PlayNotification("Must be in a dungeon!");
            return;
        }
        
        try {
            if (PlayerFarming.Instance?.RunTrinkets == null) return;
            
            var cardsCopy = new List<TarotCards.TarotCard>(PlayerFarming.Instance.RunTrinkets);
            foreach (var card in cardsCopy) {
                TrinketManager.RemoveTrinket(card.CardType, PlayerFarming.Instance);
            }
            CultUtils.PlayNotification("Cleared all tarot!");
        } catch (Exception e) {
            Debug.LogWarning($"[CheatMenu] ClearAllTarot: {e.Message}");
            CultUtils.PlayNotification("Failed to clear tarot!");
        }
    }

    private static void AddTarotCard(TarotCards.Card card) {
        if (!IsInDungeon()) {
            CultUtils.PlayNotification("Must be in a dungeon!");
            return;
        }
        
        try {
            if (PlayerFarming.Instance == null) return;
            
            // Check if already has this card
            if (TrinketManager.HasTrinket(card, PlayerFarming.Instance)) {
                CultUtils.PlayNotification($"Already has: {card}");
                return;
            }
            
            TrinketManager.AddTrinket(new TarotCards.TarotCard(card, 0), PlayerFarming.Instance);
            CultUtils.PlayNotification($"Added: {card}");
        } catch (Exception e) {
            Debug.LogWarning($"[CheatMenu] AddTarotCard {card}: {e.Message}");
            CultUtils.PlayNotification("Failed to add tarot!");
        }
    }
}

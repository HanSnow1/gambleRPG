using UnityEngine;

/// <summary>
/// Single source of truth for MVP combat tuning (Role A step 5).
/// Inspector values on CombatUI should match these unless you are playtesting overrides.
/// </summary>
public static class CombatBalance
{
  public const int PlayerStartHp = 100;
  public const int DefaultEnemyHp = 80;

  public const float DiceBossSuccessBonus = 0.05f;
  public const float CardBossSuccessBonus = 0.03f;

  public static CombatBetOption Safe => new()
  {
    label = "Safe",
    betHp = 10,
    successChance = 0.60f,
    damageOnSuccess = 20,
    extraFailDamage = 6
  };

  public static CombatBetOption Risk => new()
  {
    label = "Risk",
    betHp = 18,
    successChance = 0.45f,
    damageOnSuccess = 46,
    extraFailDamage = 12
  };

  public static CombatBetOption AllIn => new()
  {
    label = "All-in",
    betHp = 25,
    successChance = 0.30f,
    damageOnSuccess = 72,
    extraFailDamage = 20
  };

  /// <summary>Expected player HP lost per turn (bet + fail penalty).</summary>
  public static float ExpectedPlayerHpLoss(CombatBetOption option, BossDefinition boss = null)
  {
    float p = BossGambleResolvers.ModifySuccessChance(option.successChance, boss);
    return option.betHp + (1f - p) * option.extraFailDamage;
  }

  /// <summary>Expected enemy damage per turn.</summary>
  public static float ExpectedEnemyDamage(CombatBetOption option, BossDefinition boss = null)
  {
    float p = BossGambleResolvers.ModifySuccessChance(option.successChance, boss);
    return p * option.damageOnSuccess;
  }

  /// <summary>Turns to kill at 100% success rate (planning only).</summary>
  public static int TurnsIfAlwaysSuccess(int enemyHp, CombatBetOption option) =>
    Mathf.CeilToInt(enemyHp / (float)option.damageOnSuccess);
}

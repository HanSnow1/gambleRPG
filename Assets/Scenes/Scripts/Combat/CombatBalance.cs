using UnityEngine;

/// <summary>
/// Single source of truth for MVP combat tuning (Role A step 5).
/// </summary>
public static class CombatBalance
{
  public const int PlayerStartHp = 100;
  /// <summary>Normal fight enemy HP — tuned for ~3–5 turn exchanges with Risk.</summary>
  public const int DefaultEnemyHp = 52;

  /// <summary>Heal % of max HP after clearing a normal fight within a floor.</summary>
  public const float BetweenFightHealPercent = 0.25f;
  /// <summary>Heal % of max HP when advancing to the next floor.</summary>
  public const float BetweenFloorHealPercent = 0.35f;

  public const float DiceBossSuccessBonus = 0.05f;
  public const float CardBossSuccessBonus = 0.03f;

  public static CombatBetOption Safe => new()
  {
    label = "Safe",
    betHp = 10,
    successChance = 0.60f,
    damageOnSuccess = 20,
    extraFailDamage = 6,
    chipDamageOnFail = 3
  };

  public static CombatBetOption Risk => new()
  {
    label = "Risk",
    betHp = 18,
    successChance = 0.45f,
    damageOnSuccess = 46,
    extraFailDamage = 12,
    chipDamageOnFail = 5
  };

  public static CombatBetOption AllIn => new()
  {
    label = "All-in",
    betHp = 25,
    successChance = 0.30f,
    damageOnSuccess = 72,
    extraFailDamage = 20,
    chipDamageOnFail = 8
  };

  /// <summary>Desperation bet when Safe is unaffordable — bet 1 HP, deal 5 on win.</summary>
  public static CombatBetOption Hidden => new()
  {
    label = "Hidden",
    betHp = 1,
    successChance = 0.50f,
    damageOnSuccess = 5,
    extraFailDamage = 1,
    chipDamageOnFail = 1
  };

  public static float ExpectedPlayerHpLoss(CombatBetOption option, BossDefinition boss = null)
  {
    float p = BossGambleResolvers.ModifySuccessChance(option.successChance, boss);
    return p * option.betHp + (1f - p) * option.chipDamageOnFail;
  }

  public static float ExpectedEnemyDamage(CombatBetOption option, BossDefinition boss = null)
  {
    float p = BossGambleResolvers.ModifySuccessChance(option.successChance, boss);
    return p * option.damageOnSuccess;
  }

  public static int TurnsIfAlwaysSuccess(int enemyHp, CombatBetOption option) =>
    Mathf.CeilToInt(enemyHp / (float)option.damageOnSuccess);
}

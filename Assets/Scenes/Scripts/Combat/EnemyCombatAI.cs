using UnityEngine;

/// <summary>Picks enemy gamble option during the enemy attack turn.</summary>
public static class EnemyCombatAI
{
  public static CombatBetOption PickBet(int enemyHp, BossDefinition boss, BossPatternController pattern = null)
  {
    var style = pattern?.GetEnemyBetStyle() ?? BossBetStyle.None;

    if (style == BossBetStyle.PreferAllIn && enemyHp >= 25)
    {
      var allIn = PickIfAffordable(CombatBalance.AllIn, enemyHp);
      if (allIn.betHp <= enemyHp)
        return allIn;
    }

    if (style == BossBetStyle.PreferRisk && enemyHp >= 18)
    {
      var risk = PickIfAffordable(CombatBalance.Risk, enemyHp);
      if (risk.betHp <= enemyHp)
        return risk;
    }

    if (style == BossBetStyle.PreferSafe || enemyHp <= 12)
      return PickIfAffordable(CombatBalance.Safe, enemyHp);

    float roll = Random.value;

    if (enemyHp >= 35 && roll < 0.30f)
    {
      var allIn = PickIfAffordable(CombatBalance.AllIn, enemyHp);
      if (allIn.betHp <= enemyHp)
        return allIn;
    }

    if (enemyHp >= 20 && roll < 0.55f)
    {
      var risk = PickIfAffordable(CombatBalance.Risk, enemyHp);
      if (risk.betHp <= enemyHp)
        return risk;
    }

    return PickIfAffordable(CombatBalance.Safe, enemyHp);
  }

  public static float GetSuccessChance(
    CombatBetOption option,
    BossDefinition boss,
    BossPatternController pattern = null)
  {
    float chance = BossGambleResolvers.ModifySuccessChance(option.successChance, boss);
    if (pattern != null)
      chance = pattern.ModifyEnemySuccess(chance);
    return chance;
  }

  private static CombatBetOption PickIfAffordable(CombatBetOption option, int enemyHp)
  {
    if (enemyHp >= option.betHp)
      return option;

    var fallback = CombatBalance.Safe;
    fallback.betHp = Mathf.Clamp(enemyHp, 1, fallback.betHp);
    return fallback;
  }
}

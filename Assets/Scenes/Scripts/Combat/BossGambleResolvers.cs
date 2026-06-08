using UnityEngine;

public static class BossGambleResolvers
{
  public static float ModifySuccessChance(float baseChance, BossDefinition boss)
  {
    if (boss == null)
      return Mathf.Clamp01(baseChance);

    float bonus = boss.gambleType switch
    {
      BossGambleType.Dice => CombatBalance.DiceBossSuccessBonus,
      BossGambleType.Card => CombatBalance.CardBossSuccessBonus,
      BossGambleType.Shell => CombatBalance.CardBossSuccessBonus,
      _ => 0f
    };

    return Mathf.Clamp01(baseChance + bonus);
  }

  public static string GetRuleLabel(BossDefinition boss)
  {
    if (boss == null)
      return GameUIText.NormalFight;

    return GameUIText.BossRule(boss.gambleType);
  }
}

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
      _ => 0f
    };

    return Mathf.Clamp01(baseChance + bonus);
  }

  public static string GetRuleLabel(BossDefinition boss)
  {
    if (boss == null)
      return "Normal fight";

    return boss.gambleType switch
    {
      BossGambleType.Dice => $"Boss rule: Dice (+{CombatBalance.DiceBossSuccessBonus:P0} success)",
      BossGambleType.Card => $"Boss rule: Card (+{CombatBalance.CardBossSuccessBonus:P0} success)",
      _ => "Boss rule: Unknown"
    };
  }
}

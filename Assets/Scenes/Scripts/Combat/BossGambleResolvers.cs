using UnityEngine;

public static class BossGambleResolvers
{
  // MVP tuning — adjust in playtest
  private const float DiceSuccessBonus = 0.05f;
  private const float CardSuccessBonus = 0.03f;

  public static float ModifySuccessChance(float baseChance, BossDefinition boss)
  {
    if (boss == null)
      return Mathf.Clamp01(baseChance);

    float bonus = boss.gambleType switch
    {
      BossGambleType.Dice => DiceSuccessBonus,
      BossGambleType.Card => CardSuccessBonus,
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
      BossGambleType.Dice => "Boss rule: Dice (+5% success)",
      BossGambleType.Card => "Boss rule: Card (+3% success)",
      _ => "Boss rule: Unknown"
    };
  }
}

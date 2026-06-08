using System;
using UnityEngine;

[Serializable]
public struct CombatBetOption
{
  public string label;
  public int betHp;
  [Range(0f, 1f)] public float successChance;
  public int damageOnSuccess;
  [Tooltip("Legacy — use chipDamageOnFail for turn-based combat.")]
  public int extraFailDamage;
  [Tooltip("Small damage taken on a failed attack turn (no bet HP lost).")]
  public int chipDamageOnFail;

  public string ButtonLabel => FormatButtonLabel(null);

  public string FormatButtonLabel(BossDefinition boss, BossPatternController pattern = null)
  {
    float chance = BossGambleResolvers.ModifySuccessChance(successChance, boss);
    chance = AugmentCombatModifiers.ModifySuccessChance(chance, boss);
    if (pattern != null && pattern.HasActivePattern)
      chance = pattern.ModifyPlayerSuccess(this, chance);

    string risk = pattern?.GetBetRiskSuffix(label) ?? "";
    return GameUIText.LegacyButtonLabel(label, betHp, chance, damageOnSuccess, chipDamageOnFail, risk);
  }

  public string FormatDuelButtonLabel(BossDefinition boss, BossPatternController pattern = null)
  {
    string risk = pattern?.GetBetRiskSuffix(label) ?? "";
    return GameUIText.DuelButtonLabel(label, betHp, damageOnSuccess, risk);
  }
}

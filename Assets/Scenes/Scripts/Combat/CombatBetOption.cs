using System;
using UnityEngine;

[Serializable]
public struct CombatBetOption
{
  public string label;
  public int betHp;
  [Range(0f, 1f)] public float successChance;
  public int damageOnSuccess;
  public int extraFailDamage;

  public string ButtonLabel => FormatButtonLabel(null);

  public string FormatButtonLabel(BossDefinition boss)
  {
    float chance = BossGambleResolvers.ModifySuccessChance(successChance, boss);
    return $"{label}\nBet {betHp} | {chance:P0}\nDmg {damageOnSuccess} / Fail +{extraFailDamage}";
  }
}

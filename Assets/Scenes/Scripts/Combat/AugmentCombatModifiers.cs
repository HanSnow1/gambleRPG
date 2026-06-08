using System.Text;
using UnityEngine;

/// <summary>
/// Applies owned augment combat effects (Double Down, Loaded Dice, relic damage).
/// </summary>
public static class AugmentCombatModifiers
{
  public static float ModifySuccessChance(float baseChance, BossDefinition boss = null)
  {
    float chance = baseChance;

    if (PlayerAugmentState.Instance != null)
    {
      chance += PlayerAugmentState.Instance.GetBossGambleSuccessBonus(
        boss, AugmentEffectType.DiceBossExtraPip);
      chance += PlayerAugmentState.Instance.GetBossGambleSuccessBonus(
        boss, AugmentEffectType.ShellGameRevealCup);

      float doubleDownBonus = 0f;
      foreach (var owned in PlayerAugmentState.Instance.Owned)
      {
        if (owned.definition == null ||
            owned.definition.effectType != AugmentEffectType.GambleDamageDoubleChance)
          continue;

        doubleDownBonus += owned.definition.GetSecondaryValue(owned.level) / 100f;
      }

      chance += doubleDownBonus;
    }

    return Mathf.Clamp01(chance);
  }

  public static int ModifyDamageOnSuccess(int baseDamage, BossDefinition boss, out string logAppend)
  {
    logAppend = null;
    int damage = baseDamage;

    float relicMul = PlayerRelicState.Instance != null
      ? PlayerRelicState.Instance.GetDealDamageMultiplier()
      : 1f;
    if (relicMul > 1f)
      damage = Mathf.RoundToInt(damage * relicMul);

    if (PlayerAugmentState.Instance != null && boss != null)
    {
      float bossBonusPct = PlayerAugmentState.Instance.GetBossOnlyDamageBonusPercent(boss);
      if (bossBonusPct > 0f)
      {
        int before = damage;
        damage = Mathf.RoundToInt(damage * (1f + bossBonusPct / 100f));
        logAppend = GameUIText.BossSlayerLog(bossBonusPct, before, damage);
      }
    }

    if (PlayerAugmentState.Instance == null)
      return damage;

    var logs = new StringBuilder();
    if (!string.IsNullOrEmpty(logAppend))
      logs.AppendLine(logAppend);

    foreach (var owned in PlayerAugmentState.Instance.Owned)
    {
      if (owned.definition == null)
        continue;

      if (owned.definition.effectType == AugmentEffectType.DealDamageBonusChance)
      {
        float procChance = owned.definition.GetValue(owned.level) / 100f;
        float bonusPct = owned.definition.GetSecondaryValue(owned.level) / 100f;
        if (UnityEngine.Random.value <= procChance)
        {
          int before = damage;
          damage = Mathf.RoundToInt(damage * (1f + bonusPct));
          logs.AppendLine(GameUIText.LoadedDiceLog(owned.level, bonusPct, before, damage));
        }
      }

      if (owned.definition.effectType == AugmentEffectType.GambleDamageDoubleChance)
      {
        float procChance = owned.definition.GetValue(owned.level) / 100f;
        if (UnityEngine.Random.value <= procChance)
        {
          int before = damage;
          damage *= 2;
          logs.AppendLine(GameUIText.DoubleDownDealLog(owned.level, before, damage));
        }
      }
    }

    if (logs.Length > 0)
      logAppend = logs.ToString().TrimEnd();

    return damage;
  }

  public static int ModifyChipDamageToPlayer(int baseChip, out string logAppend)
  {
    logAppend = null;
    int chip = baseChip;

    float takeMul = PlayerRelicState.Instance != null
      ? PlayerRelicState.Instance.GetTakeDamageMultiplier()
      : 1f;
    if (!Mathf.Approximately(takeMul, 1f))
      chip = Mathf.Max(1, Mathf.RoundToInt(chip * takeMul));

    if (PlayerAugmentState.Instance == null)
      return chip;

    var logs = new StringBuilder();
    foreach (var owned in PlayerAugmentState.Instance.Owned)
    {
      if (owned.definition == null ||
          owned.definition.effectType != AugmentEffectType.GambleDamageDoubleChance)
        continue;

      float procChance = owned.definition.GetValue(owned.level) / 100f;
      if (UnityEngine.Random.value > procChance)
        continue;

      int before = chip;
      chip *= 2;
      logs.AppendLine(GameUIText.DoubleDownChipLog(owned.level, before, chip));
    }

    if (logs.Length > 0)
      logAppend = logs.ToString().TrimEnd();

    return chip;
  }

  public static int ModifyIncomingEnemyDamage(int baseDamage, out string logAppend)
  {
    logAppend = null;
    int damage = baseDamage;

    float takeMul = PlayerRelicState.Instance != null
      ? PlayerRelicState.Instance.GetTakeDamageMultiplier()
      : 1f;
    if (!Mathf.Approximately(takeMul, 1f))
      damage = Mathf.Max(1, Mathf.RoundToInt(damage * takeMul));

    if (PlayerAugmentState.Instance == null)
      return damage;

    var logs = new StringBuilder();
    foreach (var owned in PlayerAugmentState.Instance.Owned)
    {
      if (owned.definition == null ||
          owned.definition.effectType != AugmentEffectType.GambleDamageDoubleChance)
        continue;

      float procChance = owned.definition.GetValue(owned.level) / 100f;
      if (UnityEngine.Random.value > procChance)
        continue;

      int before = damage;
      damage *= 2;
      logs.AppendLine(GameUIText.DoubleDownTakenLog(owned.level, before, damage));
    }

    if (logs.Length > 0)
      logAppend = logs.ToString().TrimEnd();

    return damage;
  }
}

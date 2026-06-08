using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAugmentState : MonoBehaviour
{
  public static PlayerAugmentState Instance { get; private set; }

  [Serializable]
  public struct OwnedAugment
  {
    public AugmentDefinition definition;
    public int level;
  }

  [SerializeField] private int basePlayerMaxHp = 100;

  private readonly List<OwnedAugment> _owned = new();
  private readonly Dictionary<string, int> _runChargesByAugmentId = new();

  public IReadOnlyList<OwnedAugment> Owned => _owned;

  private void Awake()
  {
    if (Instance != null && Instance != this)
    {
      Destroy(this);
      return;
    }

    Instance = this;
  }

  public void ClearAll()
  {
    _owned.Clear();
    _runChargesByAugmentId.Clear();
  }

  public void ClearOwnedForReroll()
  {
    _owned.Clear();
    _runChargesByAugmentId.Clear();
  }

  public void AddOwnedFromReroll(AugmentDefinition def)
  {
    if (def == null)
      return;

    _owned.Add(new OwnedAugment { definition = def, level = 1 });
    RefreshPreventDeathCharges(def, 1);
    RefreshDraftUpgradeCharges(def, 1);
  }

  public bool HasAugmentEffect(AugmentEffectType type)
  {
    foreach (var owned in _owned)
    {
      if (owned.definition != null && owned.definition.effectType == type)
        return true;
    }

    return false;
  }

  public float GetBossGambleSuccessBonus(BossDefinition boss, AugmentEffectType type)
  {
    if (boss == null)
      return 0f;

    return GetGambleBonusForContext(boss, type, boss.gambleType);
  }

  public float GetGambleBonusForContext(
    BossDefinition boss,
    AugmentEffectType type,
    BossGambleType contextType)
  {
    float bonus = 0f;
    foreach (var owned in _owned)
    {
      if (owned.definition == null || owned.definition.effectType != type)
        continue;
      if (owned.definition.restrictToBossGambleType &&
          !MatchesBossGambleFilterForContext(boss, owned.definition.requiredBossGambleType, contextType))
        continue;

      bonus += owned.definition.GetValue(owned.level) / 100f;
    }

    return bonus;
  }

  private static bool MatchesBossGambleFilterForContext(
    BossDefinition boss,
    BossGambleType required,
    BossGambleType contextType)
  {
    if (boss != null)
      return MatchesBossGambleFilter(boss, required);

    if (required == BossGambleType.Shell)
      return contextType == BossGambleType.Card || contextType == BossGambleType.Shell;

    return contextType == required;
  }

  public float GetBossOnlyDamageBonusPercent(BossDefinition boss)
  {
    if (boss == null)
      return 0f;

    float bonus = 0f;
    foreach (var owned in _owned)
    {
      if (owned.definition == null ||
          owned.definition.effectType != AugmentEffectType.BossOnlyDamageBonus)
        continue;

      bonus += owned.definition.GetValue(owned.level);
    }

    return bonus;
  }

  public int GetConsecutiveHitHealAmount()
  {
    int heal = 0;
    foreach (var owned in _owned)
    {
      if (owned.definition == null ||
          owned.definition.effectType != AugmentEffectType.ConsecutiveHitHeal)
        continue;

      heal += Mathf.RoundToInt(owned.definition.GetValue(owned.level));
    }

    return heal;
  }

  private static bool MatchesBossGambleFilter(BossDefinition boss, BossGambleType required)
  {
    if (boss.gambleType == required)
      return true;

    if (required == BossGambleType.Shell && boss.gambleType == BossGambleType.Card)
      return true;

    return false;
  }

  /// <summary>
  /// Resets per-run counters (e.g. cheat-death charges). Call once when a new run starts.
  /// </summary>
  public void ResetRunCounters()
  {
    _runChargesByAugmentId.Clear();

    foreach (var owned in _owned)
    {
      RefreshPreventDeathCharges(owned.definition, owned.level);
      RefreshDraftUpgradeCharges(owned.definition, owned.level);
    }
  }

  private void RefreshPreventDeathCharges(AugmentDefinition def, int level)
  {
    if (def == null || string.IsNullOrEmpty(def.augmentId))
      return;

    if (def.effectType != AugmentEffectType.PreventDeathHealPercent &&
        def.effectType != AugmentEffectType.PreventDeathSurviveOneHp)
      return;

    int charges = Mathf.Max(0, Mathf.RoundToInt(def.GetValue(level)));
    if (charges <= 0)
      return;

    _runChargesByAugmentId[def.augmentId] = charges;
  }

  private void RefreshDraftUpgradeCharges(AugmentDefinition def, int level)
  {
    if (def == null || string.IsNullOrEmpty(def.augmentId))
      return;

    if (def.effectType != AugmentEffectType.NextAugmentTierUp)
      return;

    int charges = Mathf.Max(0, Mathf.RoundToInt(def.GetValue(level)));
    if (charges <= 0)
      return;

    _runChargesByAugmentId[def.augmentId] = charges;
  }

  /// <summary>
  /// Crown Upgrade: returns how many tier steps to raise the next draft (0–1 per charge).
  /// </summary>
  public int ConsumeCrownUpgradeTierBias()
  {
    const string crownId = "aug_crown_upgrade";
    if (!_runChargesByAugmentId.TryGetValue(crownId, out int remaining) || remaining <= 0)
      return 0;

    _runChargesByAugmentId[crownId] = remaining - 1;
    return 1;
  }

  public bool TryPreventDeath(BettingCombatSystem combat, out string logLine)
  {
    logLine = null;
    if (combat == null)
      return false;

    if (combat.State.playerHp > 0)
      return false;

    // Priority: heal version first, then "survive at 1".
    if (TryConsumePreventDeathByType(combat, AugmentEffectType.PreventDeathHealPercent, out logLine))
      return true;

    if (TryConsumePreventDeathByType(combat, AugmentEffectType.PreventDeathSurviveOneHp, out logLine))
      return true;

    return false;
  }

  private bool TryConsumePreventDeathByType(BettingCombatSystem combat, AugmentEffectType type, out string logLine)
  {
    logLine = null;

    for (int i = 0; i < _owned.Count; i++)
    {
      var owned = _owned[i];
      if (owned.definition == null)
        continue;
      if (owned.definition.effectType != type)
        continue;

      string id = owned.definition.augmentId;
      if (string.IsNullOrEmpty(id))
        continue;

      int remaining = 0;
      _runChargesByAugmentId.TryGetValue(id, out remaining);
      if (remaining <= 0)
        continue;

      remaining--;
      _runChargesByAugmentId[id] = remaining;

      if (type == AugmentEffectType.PreventDeathHealPercent)
      {
        float healPct = owned.definition.GetSecondaryValue(owned.level);
        int healAmount = Mathf.RoundToInt(combat.State.playerMaxHp * (healPct / 100f));
        combat.State.playerHp = Mathf.Clamp(healAmount, 1, combat.State.playerMaxHp);
        logLine = GameUIText.CheatDeathHeal(owned.definition.displayName, healPct, remaining);
        return true;
      }

      if (type == AugmentEffectType.PreventDeathSurviveOneHp)
      {
        combat.State.playerHp = 1;
        logLine = GameUIText.CheatDeathSurvive(owned.definition.displayName, remaining);
        return true;
      }
    }

    return false;
  }

  public bool IsMaxedOut(AugmentDefinition def)
  {
    if (def == null || !def.removeFromPoolAtMaxLevel)
      return false;

    int level = GetLevel(def);
    return level >= def.maxLevel;
  }

  public int GetLevel(AugmentDefinition def)
  {
    for (int i = 0; i < _owned.Count; i++)
    {
      if (_owned[i].definition == def)
        return _owned[i].level;
    }

    return 0;
  }

  public void AddOrLevelUp(AugmentDefinition def)
  {
    if (def == null)
      return;

    for (int i = 0; i < _owned.Count; i++)
    {
      if (_owned[i].definition != def)
        continue;

      var entry = _owned[i];
      entry.level = Mathf.Min(entry.level + 1, def.maxLevel);
      _owned[i] = entry;
      OnAugmentAcquiredOrLeveled(def, entry.level);
      return;
    }

    _owned.Add(new OwnedAugment { definition = def, level = 1 });
    OnAugmentAcquiredOrLeveled(def, 1);
  }

  private void OnAugmentAcquiredOrLeveled(AugmentDefinition def, int level)
  {
    RefreshPreventDeathCharges(def, level);
    RefreshDraftUpgradeCharges(def, level);

    switch (def.effectType)
    {
      case AugmentEffectType.DoubleRandomItemStat:
        if (PlayerRelicState.Instance != null)
        {
          string msg = PlayerRelicState.Instance.ApplyGildedMirror(level, out _);
          if (!string.IsNullOrEmpty(msg))
            Debug.Log(msg);
        }
        break;

      case AugmentEffectType.RerollAllAugments:
        Debug.Log(ApplyRerollAugments());
        break;

      case AugmentEffectType.RerollAllRelics:
        Debug.Log(ApplyRerollRelics());
        break;

      case AugmentEffectType.RerollAugmentsAndRelics:
        Debug.Log(ApplyRerollAugmentsAndRelics());
        break;
    }
  }

  private static AugmentCatalog LoadCatalog() =>
    Resources.Load<AugmentCatalog>("AugmentCatalog");

  private string ApplyRerollAugments() =>
    AugmentRerollService.RerollAllAugments(this, LoadCatalog());

  private string ApplyRerollRelics() =>
    AugmentRerollService.RerollAllRelics(PlayerRelicState.Instance);

  private string ApplyRerollAugmentsAndRelics() =>
    AugmentRerollService.RerollAugmentsAndRelics(this, PlayerRelicState.Instance, LoadCatalog());

  public int GetFloorMaxHpBonus(int baseMaxHp)
  {
    int bonus = 0;
    foreach (var owned in _owned)
    {
      if (owned.definition == null ||
          owned.definition.effectType != AugmentEffectType.FloorMaxHpPercent)
        continue;

      float pct = owned.definition.GetValue(owned.level);
      bonus += Mathf.RoundToInt(baseMaxHp * (pct / 100f));
    }

    return bonus;
  }

  public void ApplyPreCombat(BettingCombatSystem combat)
  {
    if (combat == null)
      return;

    int baseMaxHp = basePlayerMaxHp;
    int carriedHp = baseMaxHp;
    if (RunManager.Instance != null && RunManager.Instance.State.isActive)
    {
      baseMaxHp = RunManager.Instance.State.persistentMaxHp;
      carriedHp = RunManager.Instance.State.currentHp;
    }

    combat.State.playerMaxHp = baseMaxHp;

    int anchorBonus = GetFloorMaxHpBonus(baseMaxHp);
    if (anchorBonus > 0)
    {
      combat.State.playerMaxHp += anchorBonus;
      foreach (var owned in _owned)
      {
        if (owned.definition?.effectType != AugmentEffectType.FloorMaxHpPercent)
          continue;
        float pct = owned.definition.GetValue(owned.level);
        combat.LogMessage(GameUIText.AugmentFloorMaxHp(owned.definition.displayName, owned.level, pct));
        break;
      }
    }

    int prevMax = baseMaxHp + anchorBonus;
    PlayerRelicState.Instance?.ApplyToCombat(combat);

    int maxGain = combat.State.playerMaxHp - prevMax;
    if (carriedHp <= 0 || carriedHp > prevMax)
      carriedHp = prevMax;
    carriedHp = Mathf.Min(carriedHp + maxGain, combat.State.playerMaxHp);
    combat.State.playerHp = Mathf.Clamp(carriedHp, 1, combat.State.playerMaxHp);
  }

  public string GetSummaryLine()
  {
    if (_owned.Count == 0)
      return GameUIText.AugmentsNone;

    var parts = new List<string>();
    foreach (var owned in _owned)
    {
      if (owned.definition != null)
        parts.Add($"{owned.definition.displayName} Lv{owned.level}");
    }

    return GameUIText.AugmentsSummary(string.Join(", ", parts));
  }
}

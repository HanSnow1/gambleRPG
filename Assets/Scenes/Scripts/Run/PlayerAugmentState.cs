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

  /// <summary>
  /// Resets per-run counters (e.g. cheat-death charges). Call once when a new run starts.
  /// </summary>
  public void ResetRunCounters()
  {
    _runChargesByAugmentId.Clear();

    foreach (var owned in _owned)
    {
      if (owned.definition == null)
        continue;

      if (owned.definition.effectType != AugmentEffectType.PreventDeathHealPercent &&
          owned.definition.effectType != AugmentEffectType.PreventDeathSurviveOneHp)
        continue;

      int charges = Mathf.Max(0, Mathf.RoundToInt(owned.definition.GetValue(owned.level)));
      if (charges <= 0)
        continue;

      _runChargesByAugmentId[owned.definition.augmentId] = charges;
    }
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
        logLine = $"Cheat death: {owned.definition.displayName} (heal {healPct:0}% max HP)  [{remaining} left]";
        return true;
      }

      if (type == AugmentEffectType.PreventDeathSurviveOneHp)
      {
        combat.State.playerHp = 1;
        logLine = $"Cheat death: {owned.definition.displayName} (survive at 1 HP)  [{remaining} left]";
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
      return;
    }

    _owned.Add(new OwnedAugment { definition = def, level = 1 });
  }

  public void ApplyPreCombat(BettingCombatSystem combat)
  {
    if (combat == null)
      return;

    int baseHp = basePlayerMaxHp;
    if (RunManager.Instance != null && RunManager.Instance.State.isActive)
      baseHp = RunManager.Instance.State.maxHp;

    combat.State.playerMaxHp = baseHp;
    combat.State.playerHp = baseHp;

    foreach (var owned in _owned)
    {
      if (owned.definition == null)
        continue;

      switch (owned.definition.effectType)
      {
        case AugmentEffectType.FloorMaxHpPercent:
          float pct = owned.definition.GetValue(owned.level);
          int bonus = Mathf.RoundToInt(combat.State.playerMaxHp * (pct / 100f));
          combat.State.playerMaxHp += bonus;
          combat.State.playerHp = combat.State.playerMaxHp;
          combat.LogMessage(
            $"Augment: {owned.definition.displayName} Lv{owned.level} (+{pct:0}% max HP this floor)");
          break;
      }
    }
  }

  public string GetSummaryLine()
  {
    if (_owned.Count == 0)
      return "Augments: none";

    var parts = new List<string>();
    foreach (var owned in _owned)
    {
      if (owned.definition != null)
        parts.Add($"{owned.definition.displayName} Lv{owned.level}");
    }

    return "Augments: " + string.Join(", ", parts);
  }
}

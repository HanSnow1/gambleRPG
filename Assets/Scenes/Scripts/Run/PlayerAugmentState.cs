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

using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRelicState : MonoBehaviour
{
  public static PlayerRelicState Instance { get; private set; }

  [Serializable]
  public struct OwnedRelic
  {
    public RelicDefinition definition;
    public float statValue;
  }

  [SerializeField] private RelicCatalog catalog;

  private readonly List<OwnedRelic> _owned = new();

  public IReadOnlyList<OwnedRelic> Owned => _owned;

  private void Awake()
  {
    if (Instance != null && Instance != this)
    {
      Destroy(this);
      return;
    }

    Instance = this;
    if (catalog == null)
      catalog = Resources.Load<RelicCatalog>("RelicCatalog");
    if (catalog == null)
      catalog = RelicCatalog.CreateRuntimeFallback();
  }

  public void ClearAll()
  {
    _owned.Clear();
    SyncRunStateRelicIds();
  }

  public bool HasAnyRelic()
  {
    for (int i = 0; i < _owned.Count; i++)
    {
      if (_owned[i].definition != null)
        return true;
    }

    return false;
  }

  public string ApplyGildedMirror(int level, out bool grantedNewRelic)
  {
    grantedNewRelic = false;
    if (!HasAnyRelic())
    {
      var granted = GrantRandomCommonRelic();
      if (granted == null)
        return "Gilded Mirror: no relic catalog available.";
      grantedNewRelic = true;
      return $"Gilded Mirror: no relics — gained {granted.displayName} ({FormatStat(granted)}).";
    }

    int index = UnityEngine.Random.Range(0, _owned.Count);
    var entry = _owned[index];
    if (entry.definition == null)
      return "Gilded Mirror: invalid relic slot.";

    float before = entry.statValue;
    entry.statValue *= 2f;
    _owned[index] = entry;
    BumpPersistentMaxHpFromRelics();

    return
      $"Gilded Mirror Lv{level}: {entry.definition.displayName} {FormatStat(entry.definition, before)} → {FormatStat(entry.definition, entry.statValue)}";
  }

  public RelicDefinition GrantRandomCommonRelic()
  {
    if (catalog == null)
      catalog = RelicCatalog.CreateRuntimeFallback();

    var def = catalog.PickRandomCommon();
    if (def == null)
      return null;

    if (_owned.Count >= RunState.MaxRelicSlots)
      return null;

    _owned.Add(new OwnedRelic { definition = def, statValue = def.baseStatValue });
    SyncRunStateRelicIds();
    BumpPersistentMaxHpFromRelics();
    return def;
  }

  private void BumpPersistentMaxHpFromRelics()
  {
    if (RunManager.Instance == null || !RunManager.Instance.State.isActive)
      return;

    int bonus = 0;
    foreach (var owned in _owned)
    {
      if (owned.definition == null)
        continue;
      if (owned.definition.statType == RelicStatType.MaxHpFlat)
        bonus += Mathf.RoundToInt(owned.statValue);
    }

    if (bonus > 0)
      RunManager.Instance.State.persistentMaxHp = Mathf.Max(
        RunManager.Instance.State.persistentMaxHp,
        RunState.DefaultMaxHp + bonus);
  }

  public void RerollAllInPlace()
  {
    int count = _owned.Count;
    _owned.Clear();

    for (int i = 0; i < count; i++)
    {
      var def = catalog.PickRandomCommon();
      if (def == null)
        break;
      _owned.Add(new OwnedRelic { definition = def, statValue = def.baseStatValue });
    }

    SyncRunStateRelicIds();
    BumpPersistentMaxHpFromRelics();
  }

  public void ApplyToCombat(BettingCombatSystem combat)
  {
    if (combat == null)
      return;

    foreach (var owned in _owned)
    {
      if (owned.definition == null)
        continue;

      switch (owned.definition.statType)
      {
        case RelicStatType.MaxHpPercent:
          int pctBonus = Mathf.RoundToInt(combat.State.playerMaxHp * (owned.statValue / 100f));
          combat.State.playerMaxHp += pctBonus;
          combat.LogMessage(GameUIText.RelicMaxHp(owned.definition.displayName, owned.statValue, pctBonus));
          break;
      }
    }
  }

  public float GetDealDamageMultiplier()
  {
    float bonus = 0f;
    foreach (var owned in _owned)
    {
      if (owned.definition == null || owned.definition.statType != RelicStatType.DealDamagePercent)
        continue;
      bonus += owned.statValue / 100f;
    }

    return 1f + bonus;
  }

  public float GetTakeDamageMultiplier()
  {
    float bonus = 0f;
    foreach (var owned in _owned)
    {
      if (owned.definition == null || owned.definition.statType != RelicStatType.TakeDamagePercent)
        continue;
      bonus += owned.statValue / 100f;
    }

    return 1f + bonus;
  }

  public string GetSummaryLine()
  {
    if (_owned.Count == 0)
      return GameUIText.RelicsNone;

    var parts = new List<string>();
    foreach (var owned in _owned)
    {
      if (owned.definition != null)
        parts.Add($"{owned.definition.displayName} ({FormatStat(owned.definition, owned.statValue)})");
    }

    return GameUIText.RelicsSummary(string.Join(", ", parts));
  }

  private void SyncRunStateRelicIds()
  {
    if (RunManager.Instance == null)
      return;

    var state = RunManager.Instance.State;
    state.ResetRelicSlots();

    int slot = 0;
    foreach (var owned in _owned)
    {
      if (owned.definition == null || slot >= RunState.MaxRelicSlots)
        continue;
      state.relicIds[slot] = owned.definition.relicId;
      slot++;
    }
  }

  private static string FormatStat(RelicDefinition def) => FormatStat(def, def.baseStatValue);

  private static string FormatStat(RelicDefinition def, float value)
  {
    if (def == null)
      return "?";

    return def.statType switch
    {
      RelicStatType.MaxHpFlat => $"+{value:0} HP",
      RelicStatType.MaxHpPercent => $"+{value:0}% max HP",
      RelicStatType.DealDamagePercent => $"+{value:0}% deal dmg",
      RelicStatType.TakeDamagePercent => value >= 0 ? $"+{value:0}% taken" : $"{value:0}% taken",
      _ => $"{value:0}"
    };
  }
}

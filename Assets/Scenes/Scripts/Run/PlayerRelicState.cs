using System;
using System.Collections.Generic;
using UnityEngine;

public enum FloorRewardType
{
  None = 0,
  BreadHat = 1,
  Spear = 2,
  MysticCoin = 3
}

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
  private FloorRewardType _floorReward = FloorRewardType.None;
  private int _floorRewardTurnCounter;
  private bool _coinTriggeredThisCombat;

  public IReadOnlyList<OwnedRelic> Owned => _owned;
  public FloorRewardType FloorReward => _floorReward;

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
    ClearFloorReward();
    SyncRunStateRelicIds();
  }

  public void ClearFloorReward()
  {
    _floorReward = FloorRewardType.None;
    _floorRewardTurnCounter = 0;
    _coinTriggeredThisCombat = false;
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

  public FloorRewardType GrantRandomFloorReward()
  {
    _floorReward = (FloorRewardType)UnityEngine.Random.Range(1, 4);
    _floorRewardTurnCounter = 0;
    _coinTriggeredThisCombat = false;
    return _floorReward;
  }

  public static string GetFloorRewardName(FloorRewardType reward) =>
    reward switch
    {
      FloorRewardType.BreadHat => "허름한 투명모자",
      FloorRewardType.Spear => "마나모니",
      FloorRewardType.MysticCoin => "비트코인",
      _ => "없음"
    };

  public static string GetFloorRewardDescription(FloorRewardType reward) =>
    reward switch
    {
      FloorRewardType.BreadHat => "3턴마다 한 번, 내가 받는 데미지를 무시한다.",
      FloorRewardType.Spear => "전투 시작 시 상대방 HP의 25%를 깎고 들어간다.",
      FloorRewardType.MysticCoin => "신비한 코인이다, 오래가지고있으면 좋은 느낌이 들꺼같다.",
      _ => "효과 없음"
    };

  public static string GetFloorRewardSpritePath(FloorRewardType reward) =>
    reward switch
    {
      FloorRewardType.BreadHat => "UI/bread_hat",
      FloorRewardType.Spear => "UI/spear",
      FloorRewardType.MysticCoin => "UI/mystic_coin",
      _ => string.Empty
    };

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

  public void ApplyFloorRewardCombatStart(BettingCombatSystem combat)
  {
    if (!IsFloorRewardActive() || combat == null)
      return;

    _floorRewardTurnCounter = 0;
    _coinTriggeredThisCombat = false;

    if (_floorReward == FloorRewardType.BreadHat)
      combat.LogMessage("[아이템 준비] 허름한 투명모자: 3턴마다 받는 데미지를 1회 무시합니다.");
    else if (_floorReward == FloorRewardType.MysticCoin)
      combat.LogMessage("[아이템 준비] 비트코인: 9턴이 지나면 특별한 일이 일어납니다.");

    if (_floorReward == FloorRewardType.Spear)
    {
      int cut = Mathf.Max(1, Mathf.RoundToInt(combat.State.enemyMaxHp * 0.25f));
      int before = combat.State.enemyHp;
      combat.State.enemyHp = Mathf.Max(0, combat.State.enemyHp - cut);
      combat.LogMessage($"[아이템 발동] 마나모니: 전투 시작 피해 {before - combat.State.enemyHp} (적 HP 25%)");
    }
  }

  public int ModifyIncomingDamageForFloorReward(BettingCombatSystem combat, int incomingDamage)
  {
    if (!IsFloorRewardActive() || combat == null || incomingDamage <= 0)
      return incomingDamage;

    if (_floorReward == FloorRewardType.BreadHat && ((combat.TurnCount + 1) % 3 == 0))
    {
      combat.LogMessage($"[아이템 발동] 허름한 투명모자: {combat.TurnCount + 1}턴 피해를 무시했습니다.");
      return 0;
    }

    return incomingDamage;
  }

  public void OnFloorRewardTurnResolved(BettingCombatSystem combat)
  {
    if (!IsFloorRewardActive() || combat == null)
      return;

    _floorRewardTurnCounter++;
    if (_floorReward == FloorRewardType.MysticCoin && !_coinTriggeredThisCombat && _floorRewardTurnCounter >= 9)
    {
      _coinTriggeredThisCombat = true;
      combat.State.playerMaxHp = 999999;
      combat.State.playerHp = 999999;
      combat.LogMessage("[아이템 발동] 비트코인: 9턴 경과! HP가 무한대가 되었습니다.");
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

  private bool IsFloorRewardActive()
  {
    if (_floorReward == FloorRewardType.None || RunManager.Instance == null || !RunManager.Instance.State.isActive)
      return false;

    return RunManager.Instance.State.currentFloor == 2;
  }
}

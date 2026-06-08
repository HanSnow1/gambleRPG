using System;
using UnityEngine;

public class RunManager : MonoBehaviour
{
  public static RunManager Instance { get; private set; }

  [Header("Boss pool — Floor 1: Dice, Floor 2: Card Queen")]
  [SerializeField] private BossDefinition diceBoss;
  [SerializeField] private BossDefinition cardBoss;

  [Header("Run tuning")]
  [SerializeField] private int startingMaxHp = RunState.DefaultMaxHp;

  public RunState State { get; private set; } = new RunState();

  public event Action<RunState> OnRunStarted;
  public event Action<RunState> OnStateChanged;

  private void Awake()
  {
    if (Instance != null && Instance != this)
    {
      Destroy(gameObject);
      return;
    }

    Instance = this;
    State.ResetRelicSlots();
    State.phase = RunPhase.Title;
  }

  private void OnDestroy()
  {
    if (Instance == this)
      Instance = null;
  }

  public void StartNewRun()
  {
    if (diceBoss == null || cardBoss == null)
    {
      Debug.LogError("RunManager: Assign Dice Boss (Floor 1) and Card Boss (Floor 2) in the Inspector.");
      return;
    }

    State = new RunState
    {
      isActive = true,
      phase = RunPhase.BossPreview,
      currentFloor = 1,
      maxHp = startingMaxHp,
      currentHp = startingMaxHp,
      persistentMaxHp = startingMaxHp,
      normalFightsCompleted = 0,
      normalFightsBeforeBoss = RunState.DefaultNormalFightsBeforeBoss,
      assignedBoss = diceBoss
    };
    State.ResetRelicSlots();

    NotifyChanged();
    OnRunStarted?.Invoke(State);
  }

  public void SetPhase(RunPhase phase)
  {
    State.phase = phase;
    NotifyChanged();
  }

  public bool TryAddAugment(string augmentId)
  {
    if (string.IsNullOrEmpty(augmentId) || State.augmentIds.Contains(augmentId))
      return false;
    State.augmentIds.Add(augmentId);
    NotifyChanged();
    return true;
  }

  public BossDefinition GetBossForFloor(int floor) =>
    floor >= RunState.TotalFloors ? cardBoss : diceBoss;

  /// <summary>After a floor boss (non-final). Returns HP healed between floors.</summary>
  public int AdvanceToNextFloor()
  {
    int healed = ApplyBetweenFloorHeal();
    State.currentFloor = Mathf.Min(State.currentFloor + 1, RunState.TotalFloors);
    State.normalFightsCompleted = 0;
    State.assignedBoss = GetBossForFloor(State.currentFloor);
    NotifyChanged();
    return healed;
  }

  public void RecordNormalFightVictory()
  {
    State.normalFightsCompleted++;
    NotifyChanged();
  }

  public void SyncHpFromCombat(int currentHp, int maxHp)
  {
    State.currentHp = Mathf.Max(0, currentHp);

    int anchorBonus = PlayerAugmentState.Instance != null
      ? PlayerAugmentState.Instance.GetFloorMaxHpBonus(State.persistentMaxHp)
      : 0;
    int persistentMax = Mathf.Max(State.persistentMaxHp, maxHp - anchorBonus);
    State.persistentMaxHp = Mathf.Max(1, persistentMax);
    State.maxHp = maxHp;
    NotifyChanged();
  }

  public int ApplyBetweenFightHeal()
  {
    int before = State.currentHp;
    int healAmount = Mathf.RoundToInt(State.maxHp * CombatBalance.BetweenFightHealPercent);
    State.currentHp = Mathf.Min(State.currentHp + healAmount, State.maxHp);
    NotifyChanged();
    return State.currentHp - before;
  }

  private int ApplyBetweenFloorHeal()
  {
    int before = State.currentHp;
    int healAmount = Mathf.RoundToInt(State.maxHp * CombatBalance.BetweenFloorHealPercent);
    State.currentHp = Mathf.Min(State.currentHp + healAmount, State.maxHp);
    NotifyChanged();
    return State.currentHp - before;
  }

  public void EndRunToTitle()
  {
    State.isActive = false;
    State.phase = RunPhase.Title;
    NotifyChanged();
  }

  public void SetBossPool(BossDefinition dice, BossDefinition card)
  {
    diceBoss = dice;
    cardBoss = card;
  }

  private void NotifyChanged() => OnStateChanged?.Invoke(State);
}

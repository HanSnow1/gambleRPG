using System;
using UnityEngine;

public class RunManager : MonoBehaviour
{
  public static RunManager Instance { get; private set; }

  [Header("Boss pool (A: BossDefinition assets)")]
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
    var boss = PickRandomBoss();
    if (boss == null)
    {
      Debug.LogError("RunManager: Assign Dice Boss and Card Boss in the Inspector.");
      return;
    }

    State = new RunState
    {
      isActive = true,
      phase = RunPhase.BossPreview,
      currentFloor = 0,
      maxHp = startingMaxHp,
      currentHp = startingMaxHp,
      assignedBoss = boss
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

  public void AdvanceFloor()
  {
    State.currentFloor++;
    NotifyChanged();
  }

  public void SetBossPool(BossDefinition dice, BossDefinition card)
  {
    diceBoss = dice;
    cardBoss = card;
  }

  private BossDefinition PickRandomBoss()
  {
    if (diceBoss != null && cardBoss != null)
      return UnityEngine.Random.value < 0.5f ? diceBoss : cardBoss;
    if (diceBoss != null)
      return diceBoss;
    return cardBoss;
  }

  private void NotifyChanged() => OnStateChanged?.Invoke(State);
}

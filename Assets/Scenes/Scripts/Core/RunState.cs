using System;
using System.Collections.Generic;

[Serializable]
public class RunState
{
  public const int DefaultMaxHp = 100;
  public const int MaxRelicSlots = 5;

  public const int TotalFloors = 2;
  public const int DefaultNormalFightsBeforeBoss = 1;

  public bool isActive;
  public RunPhase phase = RunPhase.None;
  /// <summary>Current run floor (1 = Dice, 2 = Card Queen).</summary>
  public int currentFloor = 1;
  public int currentHp = DefaultMaxHp;
  public int maxHp = DefaultMaxHp;
  /// <summary>Max HP without temporary floor buffs (relics included).</summary>
  public int persistentMaxHp = DefaultMaxHp;

  /// <summary>Normal fights won this run before the boss.</summary>
  public int normalFightsCompleted;
  public int normalFightsBeforeBoss = DefaultNormalFightsBeforeBoss;

  public BossDefinition assignedBoss;

  public List<string> relicIds = new List<string>(MaxRelicSlots);
  public List<string> augmentIds = new List<string>();

  public bool IsPlayerAlive => currentHp > 0;
  public bool IsBossFightNext => normalFightsCompleted >= normalFightsBeforeBoss;
  public bool IsFinalFloor => currentFloor >= TotalFloors;

  public void ResetRelicSlots()
  {
    relicIds.Clear();
    for (int i = 0; i < MaxRelicSlots; i++)
      relicIds.Add(string.Empty);
  }
}

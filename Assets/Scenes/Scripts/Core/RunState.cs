using System;
using System.Collections.Generic;

[Serializable]
public class RunState
{
  public const int DefaultMaxHp = 100;
  public const int MaxRelicSlots = 5;

  public bool isActive;
  public RunPhase phase = RunPhase.None;
  public int currentFloor;
  public int currentHp = DefaultMaxHp;
  public int maxHp = DefaultMaxHp;

  public BossDefinition assignedBoss;

  public List<string> relicIds = new List<string>(MaxRelicSlots);
  public List<string> augmentIds = new List<string>();

  public bool IsPlayerAlive => currentHp > 0;

  public void ResetRelicSlots()
  {
    relicIds.Clear();
    for (int i = 0; i < MaxRelicSlots; i++)
      relicIds.Add(string.Empty);
  }
}

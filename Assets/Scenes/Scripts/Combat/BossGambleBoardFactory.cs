using UnityEngine;

public static class BossGambleBoardFactory
{
  public static BossGambleBoard EnsureBoard(
    BettingCombatSystem combat,
    BossDefinition boss,
    BossPatternRuntimeState state)
  {
    if (combat == null || boss == null || state?.Pattern == null)
      return null;

    foreach (var old in combat.GetComponentsInChildren<BossGambleBoard>(true))
    {
      if (old != null)
        Object.Destroy(old.gameObject);
    }

    if (state.Pattern.minigameType == BossMinigameType.None)
      return null;

    var host = new GameObject("BossGambleBoardHost");
    host.transform.SetParent(combat.transform, false);

    BossGambleBoard board = state.Pattern.minigameType switch
    {
      BossMinigameType.DiceFaces => host.AddComponent<DiceTyrantBoard>(),
      BossMinigameType.CardTable => host.AddComponent<QueenCardBoard>(),
      _ => null
    };

    board?.Setup(boss, state);
    return board;
  }
}

using UnityEngine;

public static class BossGambleBoardFactory
{
  public static BossGambleBoard EnsureBoard(
    BettingCombatSystem combat,
    BossDefinition boss,
    BossPatternRuntimeState state)
  {
    // User request: remove boss info mini-board UI from all screens.
    RemoveAllBoardPanels();

    if (combat == null || boss == null || state?.Pattern == null)
      return null;

    foreach (var old in combat.GetComponentsInChildren<BossGambleBoard>(true))
    {
      if (old != null)
        Object.Destroy(old.gameObject);
    }

    // Keep pattern logic, but never spawn the visual board panel.
    return null;
  }

  private static void RemoveAllBoardPanels()
  {
    var panels = Object.FindObjectsByType<RectTransform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
    foreach (var rt in panels)
    {
      if (rt != null && rt.name == "BossGambleBoardPanel")
        Object.Destroy(rt.gameObject);
    }
  }
}

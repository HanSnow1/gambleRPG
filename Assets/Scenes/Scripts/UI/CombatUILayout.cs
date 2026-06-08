using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Repositions combat HUD so panels do not overlap.</summary>
public static class CombatUILayout
{
  public static void Apply(Transform canvasRoot)
  {
    if (canvasRoot == null)
      return;

    LayoutRect(canvasRoot, "PlayerHpText", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(18f, -52f), new Vector2(280f, 44f));
    LayoutRect(canvasRoot, "PlayerHpPanel", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(18f, -52f), new Vector2(300f, 52f));

    LayoutRect(canvasRoot, "EnemyHpText", new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-18f, -52f), new Vector2(280f, 44f));
    LayoutRect(canvasRoot, "EnemyHpPanel", new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-18f, -52f), new Vector2(300f, 52f));

    LayoutRect(canvasRoot, "BossRuleToggleButton", new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-18f, 110f), new Vector2(220f, 32f));
    LayoutRect(canvasRoot, "BossRuleText", new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-18f, 24f), new Vector2(360f, 128f));
    LayoutRect(canvasRoot, "BossRulePanel", new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-18f, 24f), new Vector2(380f, 136f));

    LayoutRect(canvasRoot, "StatusText", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 100f), new Vector2(700f, 72f));
    LayoutRect(canvasRoot, "StatusPanel", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 100f), new Vector2(720f, 80f));

    LayoutRect(canvasRoot, "CombatLogText", new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(16f, 16f), new Vector2(420f, 120f));
    LayoutRect(canvasRoot, "CombatLogPanel", new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(16f, 16f), new Vector2(440f, 132f));

    LayoutRect(canvasRoot, "BetSmall", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-230f, 100f), new Vector2(148f, 76f));
    LayoutRect(canvasRoot, "BetMedium", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 100f), new Vector2(148f, 76f));
    LayoutRect(canvasRoot, "BetLarge", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(230f, 100f), new Vector2(148f, 76f));
    LayoutRect(canvasRoot, "BetHidden", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 188f), new Vector2(200f, 72f));
    LayoutRect(canvasRoot, "RestartButton", new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-18f, 18f), new Vector2(120f, 36f));

    LayoutBossBoard(canvasRoot);
  }

  private static void LayoutBossBoard(Transform root)
  {
    var board = FindDeep(root, "BossGambleBoardPanel");
    if (board == null)
      return;

    var rt = board.GetComponent<RectTransform>();
    if (rt == null)
      return;

    rt.anchorMin = new Vector2(1f, 1f);
    rt.anchorMax = new Vector2(1f, 1f);
    rt.pivot = new Vector2(1f, 1f);
    rt.anchoredPosition = new Vector2(-16f, -420f);
    rt.sizeDelta = new Vector2(280f, 120f);
  }

  private static void LayoutRect(
    Transform root,
    string name,
    Vector2 anchorMin,
    Vector2 anchorMax,
    Vector2 pivot,
    Vector2 anchoredPos,
    Vector2 sizeDelta)
  {
    var t = FindDeep(root, name);
    if (t == null)
      return;

    var rt = t.GetComponent<RectTransform>();
    if (rt == null)
      return;

    rt.anchorMin = anchorMin;
    rt.anchorMax = anchorMax;
    rt.pivot = pivot;
    rt.anchoredPosition = anchoredPos;
    rt.sizeDelta = sizeDelta;

    var tmp = t.GetComponentInChildren<TMP_Text>();
    if (tmp != null && name.Contains("Log"))
      tmp.alignment = TextAlignmentOptions.BottomLeft;
  }

  private static Transform FindDeep(Transform root, string objectName)
  {
    if (root.name == objectName)
      return root;

    for (int i = 0; i < root.childCount; i++)
    {
      var found = FindDeep(root.GetChild(i), objectName);
      if (found != null)
        return found;
    }

    return null;
  }
}

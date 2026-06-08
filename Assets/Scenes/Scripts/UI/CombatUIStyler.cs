using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Applies themed layout to combat HUD elements at runtime.</summary>
[DefaultExecutionOrder(-80)]
public class CombatUIStyler : MonoBehaviour
{
  [SerializeField] private CombatUI combatUI;

  private Image _playerHpFill;
  private Image _enemyHpFill;
  private TMP_Text _topBarText;

  private void Awake()
  {
    if (combatUI == null)
      combatUI = GetComponent<CombatUI>();

    var scaler = GetComponent<CanvasScaler>();
    GameUITheme.ConfigureCanvasScaler(scaler);
    GameUITheme.ApplyCanvasBackground(transform);

    BuildTopBar();
    StyleCombatElements();
    CombatUILayout.Apply(transform);
  }

  public void RefreshHpBars(int playerHp, int playerMax, int enemyHp, int enemyMax)
  {
    GameUITheme.SetHpBar(_playerHpFill, playerHp, playerMax);
    GameUITheme.SetHpBar(_enemyHpFill, enemyHp, enemyMax);
    RefreshTopBar();
  }

  private void BuildTopBar()
  {
    if (transform.Find("RunTopBar") != null)
      return;

    var bar = new GameObject("RunTopBar", typeof(RectTransform), typeof(Image));
    bar.transform.SetParent(transform, false);

    var rt = bar.GetComponent<RectTransform>();
    rt.anchorMin = new Vector2(0, 1);
    rt.anchorMax = new Vector2(1, 1);
    rt.pivot = new Vector2(0.5f, 1);
    rt.anchoredPosition = Vector2.zero;
    rt.sizeDelta = new Vector2(0, 36);

    bar.GetComponent<Image>().color = new Color(0.08f, 0.1f, 0.18f, 0.92f);

    _topBarText = GameUITheme.CreateText(
      bar.transform,
      "RunTopBarText",
      "GambleRogue",
      13,
      FontStyles.Bold,
      TextAlignmentOptions.MidlineLeft);
    var textRt = _topBarText.GetComponent<RectTransform>();
    textRt.anchorMin = Vector2.zero;
    textRt.anchorMax = Vector2.one;
    textRt.offsetMin = new Vector2(16, 0);
    textRt.offsetMax = new Vector2(-16, 0);
    _topBarText.color = GameUITheme.TextMuted;
  }

  private void RefreshTopBar()
  {
    if (_topBarText == null)
      return;

    string floor = "";
    string loadout = "";

    if (RunManager.Instance != null && RunManager.Instance.State.isActive)
    {
      var s = RunManager.Instance.State;
      floor = $"{s.currentFloor}층 / {RunState.TotalFloors}층";
    }

    if (PlayerAugmentState.Instance != null)
      loadout = PlayerAugmentState.Instance.GetSummaryLine();

    if (PlayerRelicState.Instance != null)
    {
      string relics = PlayerRelicState.Instance.GetSummaryLine();
      loadout = string.IsNullOrEmpty(loadout) ? relics : $"{loadout}  |  {relics}";
    }

    _topBarText.text = string.IsNullOrEmpty(floor)
      ? "GambleRogue"
      : $"{floor}   |   {loadout}";
  }

  private void StyleCombatElements()
  {
    StyleBetButton("BetSmall", "Safe");
    StyleBetButton("BetMedium", "Risk");
    StyleBetButton("BetLarge", "All-in");
    StyleBetButton("BetHidden", "Hidden");
    StyleRestartButton();

    var playerHp = FindTmp("PlayerHpText");
    var enemyHp = FindTmp("EnemyHpText");
    var status = FindTmp("StatusText");
    var bossRule = FindTmp("BossRuleText");
    var log = FindTmp("CombatLogText");

    if (playerHp != null)
    {
      GameUITheme.StyleTitleText(playerHp);
      playerHp.richText = true;
      var panel = GameUITheme.WrapTextInPanel(playerHp, "PlayerHpPanel", new Vector2(20, 28));
      if (panel != null)
        GameUITheme.CreateHpBarUnder(panel.transform, "PlayerHpBar", out _playerHpFill);
    }

    if (enemyHp != null)
    {
      GameUITheme.StyleTitleText(enemyHp);
      enemyHp.richText = true;
      var panel = GameUITheme.WrapTextInPanel(enemyHp, "EnemyHpPanel", new Vector2(20, 28));
      if (panel != null)
        GameUITheme.CreateHpBarUnder(panel.transform, "EnemyHpBar", out _enemyHpFill);
    }

    if (status != null)
    {
      GameUITheme.WrapTextInPanel(status, "StatusPanel", new Vector2(16, 12));
      GameUITheme.StyleBodyText(status);
      status.fontSize = 15;
    }

    if (bossRule != null)
    {
      GameUITheme.WrapTextInPanel(bossRule, "BossRulePanel", new Vector2(16, 12));
      GameUITheme.StyleMutedText(bossRule);
      bossRule.fontSize = 13;
      bossRule.fontStyle = FontStyles.Normal;
      bossRule.gameObject.SetActive(false);
    }

    if (log != null)
    {
      GameUITheme.WrapTextInPanel(log, "CombatLogPanel", new Vector2(12, 8));
      GameUITheme.StyleBodyText(log);
      log.fontSize = 12;
      log.alignment = TextAlignmentOptions.TopLeft;
      log.color = new Color(0.85f, 0.9f, 1f, 0.95f);
    }
  }

  private void StyleBetButton(string objectName, string betLabel)
  {
    var t = FindChildRecursive(transform, objectName);
    if (t == null)
      return;

    var btn = t.GetComponent<Button>();
    if (btn != null)
    {
      var rt = btn.GetComponent<RectTransform>();
      if (rt != null)
        rt.sizeDelta = new Vector2(Mathf.Max(rt.sizeDelta.x, 140), Mathf.Max(rt.sizeDelta.y, 72));
      GameUITheme.StyleBetButton(btn, betLabel);
    }
  }

  private void StyleRestartButton()
  {
    var t = FindChildRecursive(transform, "RestartButton");
    if (t == null)
      return;

    var btn = t.GetComponent<Button>();
    var img = t.GetComponent<Image>();
    if (img != null)
      img.color = GameUITheme.Border;

    if (btn != null)
    {
      var tmp = btn.GetComponentInChildren<TMP_Text>();
      if (tmp != null)
      {
        tmp.text = GameUIText.Restart;
        GameUITheme.StyleMutedText(tmp);
        tmp.fontStyle = FontStyles.Normal;
      }
    }
  }

  private TMP_Text FindTmp(string name) =>
    FindChildRecursive(transform, name)?.GetComponent<TMP_Text>();

  private static Transform FindChildRecursive(Transform root, string objectName)
  {
    if (root.name == objectName)
      return root;

    for (int i = 0; i < root.childCount; i++)
    {
      var found = FindChildRecursive(root.GetChild(i), objectName);
      if (found != null)
        return found;
    }

    return null;
  }
}

using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Base HUD for boss gamble minigames — builds UI at runtime if not assigned.</summary>
public abstract class BossGambleBoard : MonoBehaviour
{
  [SerializeField] protected TMP_Text boardTitleText;
  [SerializeField] protected TMP_Text boardBodyText;
  [SerializeField] protected TMP_Text boardFooterText;

  protected BossDefinition Boss;
  protected BossPatternRuntimeState State;

  public virtual void Setup(BossDefinition boss, BossPatternRuntimeState state)
  {
    Boss = boss;
    State = state;
    EnsureUi();
    Refresh();
    gameObject.SetActive(boss != null && state?.Pattern != null);
  }

  public virtual void Hide()
  {
    if (gameObject != null)
      gameObject.SetActive(false);
  }

  public void RefreshBoard() => Refresh();

  public abstract float GetPlayerSuccessModifier(CombatBetOption option);
  public abstract float ResolvePlayerRoll(CombatBetOption option, out string logLine);
  public abstract string GetBoardStatusLine();
  public abstract string GetBetRiskIcon(string betLabel);

  public virtual void OnPlayerTurnStarted() { }
  public virtual void OnPlayerMiss(CombatBetOption option) { }
  public virtual void OnPhaseEnraged(string logSink) { }

  protected void EnsureUi()
  {
    if (boardBodyText != null)
      return;

    var canvas = FindFirstObjectByType<Canvas>();
    if (canvas == null)
      return;

    var panel = new GameObject("BossGambleBoardPanel");
    panel.transform.SetParent(canvas.transform, false);
    var rect = panel.AddComponent<RectTransform>();
    rect.anchorMin = new Vector2(0.68f, 0.72f);
    rect.anchorMax = new Vector2(0.98f, 0.94f);
    rect.offsetMin = Vector2.zero;
    rect.offsetMax = Vector2.zero;

    var bg = panel.AddComponent<Image>();
    bg.color = new Color(GameUITheme.BgPanelLight.r, GameUITheme.BgPanelLight.g, GameUITheme.BgPanelLight.b, 0.94f);

    boardTitleText = CreateTmp(panel.transform, "BoardTitle", 16, FontStyles.Bold,
      new Vector2(0.04f, 0.78f), new Vector2(0.96f, 0.96f));
    boardBodyText = CreateTmp(panel.transform, "BoardBody", 13, FontStyles.Normal,
      new Vector2(0.04f, 0.22f), new Vector2(0.96f, 0.76f));
    boardFooterText = CreateTmp(panel.transform, "BoardFooter", 12, FontStyles.Italic,
      new Vector2(0.04f, 0.04f), new Vector2(0.96f, 0.20f));

    transform.SetParent(panel.transform, false);
  }

  protected static TMP_Text CreateTmp(
    Transform parent,
    string name,
    float fontSize,
    FontStyles style,
    Vector2 anchorMin,
    Vector2 anchorMax)
  {
    var go = new GameObject(name);
    go.transform.SetParent(parent, false);
    var rect = go.AddComponent<RectTransform>();
    rect.anchorMin = anchorMin;
    rect.anchorMax = anchorMax;
    rect.offsetMin = Vector2.zero;
    rect.offsetMax = Vector2.zero;

    var tmp = go.AddComponent<TextMeshProUGUI>();
    tmp.fontSize = fontSize;
    tmp.fontStyle = style;
    tmp.color = GameUITheme.TextPrimary;
    tmp.alignment = TextAlignmentOptions.TopLeft;
    tmp.enableWordWrapping = true;
    return tmp;
  }

  protected void SetTexts(string title, string body, string footer)
  {
    if (boardTitleText != null)
      boardTitleText.text = title;
    if (boardBodyText != null)
      boardBodyText.text = body;
    if (boardFooterText != null)
      boardFooterText.text = footer;
  }

  protected abstract void Refresh();
}

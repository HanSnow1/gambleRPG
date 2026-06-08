using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Small Isaac-style route preview shown on the right side of the HUD.</summary>
[DefaultExecutionOrder(-70)]
public class RunMiniMapUI : MonoBehaviour
{
  private const float NodeSize = 22f;
  private const float StepY = 25f;

  private static readonly MapNode[] Nodes =
  {
    new("전투시작", "S", NodeKind.Start, false),
    new("첫증강선택", "+", NodeKind.Augment, false),
    new("1층 주사위보스", "D", NodeKind.Boss, false),
    new("아이템 선택", "I", NodeKind.Augment, false),
    new("2층 카드보스", "C", NodeKind.Boss, false)
  };

  private readonly Image[] _nodeImages = new Image[Nodes.Length];
  private readonly TMP_Text[] _iconTexts = new TMP_Text[Nodes.Length];
  private readonly TMP_Text[] _labelTexts = new TMP_Text[Nodes.Length];
  private readonly Image[] _connectors = new Image[Nodes.Length - 1];

  private RectTransform _panel;
  private RunManager _runManager;
  private bool _showOnlyInPause;

  private void Awake()
  {
    Build();
    ResolveRunManager();
    Refresh(_runManager != null ? _runManager.State : null);
  }

  private void OnEnable()
  {
    ResolveRunManager();
    if (_runManager != null)
      _runManager.OnStateChanged += Refresh;
  }

  private void OnDisable()
  {
    if (_runManager != null)
      _runManager.OnStateChanged -= Refresh;
  }

  private void LateUpdate()
  {
    if (_panel != null && _panel.gameObject.activeSelf)
      _panel.SetAsLastSibling();
  }

  private void ResolveRunManager()
  {
    if (_runManager == null)
      _runManager = RunManager.Instance ?? FindFirstObjectByType<RunManager>();
  }

  public void Refresh(RunState state)
  {
    if (_panel == null)
      return;

    bool visible = _showOnlyInPause &&
      state != null &&
      state.isActive &&
      state.phase != RunPhase.None &&
      state.phase != RunPhase.Title;

    _panel.gameObject.SetActive(visible);
    if (!visible)
      return;

    int currentIndex = GetCurrentNodeIndex(state);
    bool runEnded = state.phase == RunPhase.Victory;

    for (int i = 0; i < Nodes.Length; i++)
    {
      bool complete = runEnded || i < currentIndex;
      bool current = i == currentIndex && !runEnded;
      ApplyNodeStyle(i, complete, current);

      if (i < _connectors.Length)
        _connectors[i].color = complete || runEnded ? GameUITheme.Heal : GameUITheme.Border;
    }
  }

  public void SetShowOnlyInPause(bool visibleInPause)
  {
    _showOnlyInPause = visibleInPause;
    Refresh(_runManager != null ? _runManager.State : null);
  }

  private static int GetCurrentNodeIndex(RunState state)
  {
    if (state == null)
      return 0;

    if (state.phase == RunPhase.Victory)
      return Nodes.Length - 1;

    if (state.phase == RunPhase.GameOver)
      return GetCombatRouteIndex(state);

    if (state.phase == RunPhase.BossCombat || (state.phase == RunPhase.BossPreview && state.IsBossFightNext))
      return state.currentFloor >= 2 ? 4 : 2;

    if (state.phase == RunPhase.AugmentPick)
    {
      if (state.currentFloor <= 1 && state.normalFightsCompleted <= 0)
        return 1;

      return state.currentFloor >= 2 && state.normalFightsCompleted <= 0 ? 3 : GetCombatRouteIndex(state);
    }

    if (state.phase == RunPhase.FloorClear)
      return 3;

    if (state.phase == RunPhase.Combat || state.phase == RunPhase.BossPreview || state.phase == RunPhase.StageClear)
      return GetCombatRouteIndex(state);

    return 0;
  }

  private static int GetCombatRouteIndex(RunState state)
  {
    if (state.currentFloor >= 2)
      return 4;

    return state.normalFightsCompleted > 0 ? 2 : 0;
  }

  private void Build()
  {
    var existing = transform.Find("RunMiniMapPanel");
    if (existing != null)
    {
      _panel = existing.GetComponent<RectTransform>();
      return;
    }

    var panelGo = new GameObject("RunMiniMapPanel", typeof(RectTransform), typeof(Image));
    panelGo.transform.SetParent(transform, false);
    _panel = panelGo.GetComponent<RectTransform>();
    _panel.anchorMin = _panel.anchorMax = new Vector2(1f, 0.5f);
    _panel.pivot = new Vector2(1f, 0.5f);
    _panel.anchoredPosition = new Vector2(-18f, 0f);
    _panel.sizeDelta = new Vector2(196f, 180f);

    var panelImage = panelGo.GetComponent<Image>();
    GameUITheme.ApplyRoundedFrameStyle(panelImage, new Color(GameUITheme.BgPanel.r, GameUITheme.BgPanel.g, GameUITheme.BgPanel.b, 0.82f));

    var title = GameUITheme.CreateText(panelGo.transform, "RunMiniMapTitle", "RUN MAP", 11, FontStyles.Bold, TextAlignmentOptions.MidlineLeft);
    var titleRt = title.GetComponent<RectTransform>();
    titleRt.anchorMin = new Vector2(0f, 1f);
    titleRt.anchorMax = new Vector2(1f, 1f);
    titleRt.pivot = new Vector2(0.5f, 1f);
    titleRt.anchoredPosition = new Vector2(0f, -8f);
    titleRt.sizeDelta = new Vector2(-22f, 18f);
    title.color = GameUITheme.TextMuted;

    for (int i = 0; i < Nodes.Length; i++)
      BuildNode(panelGo.transform, i);

    Refresh(null);
  }

  private void BuildNode(Transform parent, int index)
  {
    float y = -38f - StepY * index;
    if (index > 0)
      BuildConnector(parent, index - 1, y + StepY * 0.5f);

    var node = new GameObject($"RunMiniMapNode{index}", typeof(RectTransform), typeof(Image));
    node.transform.SetParent(parent, false);
    var rt = node.GetComponent<RectTransform>();
    rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
    rt.pivot = new Vector2(0.5f, 0.5f);
    rt.anchoredPosition = new Vector2(24f, y);
    rt.sizeDelta = new Vector2(NodeSize, NodeSize);

    _nodeImages[index] = node.GetComponent<Image>();

    var icon = GameUITheme.CreateText(node.transform, "Icon", Nodes[index].icon, 10, FontStyles.Bold);
    var iconRt = icon.GetComponent<RectTransform>();
    iconRt.anchorMin = Vector2.zero;
    iconRt.anchorMax = Vector2.one;
    iconRt.offsetMin = Vector2.zero;
    iconRt.offsetMax = Vector2.zero;
    _iconTexts[index] = icon;

    var label = GameUITheme.CreateText(parent, $"RunMiniMapLabel{index}", Nodes[index].label, 10, FontStyles.Normal, TextAlignmentOptions.MidlineLeft);
    var labelRt = label.GetComponent<RectTransform>();
    labelRt.anchorMin = labelRt.anchorMax = new Vector2(0f, 1f);
    labelRt.pivot = new Vector2(0f, 0.5f);
    labelRt.anchoredPosition = new Vector2(42f, y);
    labelRt.sizeDelta = new Vector2(136f, 18f);
    _labelTexts[index] = label;
  }

  private void BuildConnector(Transform parent, int index, float y)
  {
    var connector = new GameObject($"RunMiniMapConnector{index}", typeof(RectTransform), typeof(Image));
    connector.transform.SetParent(parent, false);
    connector.transform.SetAsFirstSibling();

    var rt = connector.GetComponent<RectTransform>();
    rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
    rt.pivot = new Vector2(0.5f, 0.5f);
    rt.anchoredPosition = new Vector2(24f, y);
    rt.sizeDelta = new Vector2(3f, StepY - NodeSize + 7f);

    _connectors[index] = connector.GetComponent<Image>();
  }

  private void ApplyNodeStyle(int index, bool complete, bool current)
  {
    var node = Nodes[index];
    Color baseColor = GetKindColor(node.kind);
    Color fill = node.optional
      ? new Color(GameUITheme.BgPanelLight.r, GameUITheme.BgPanelLight.g, GameUITheme.BgPanelLight.b, 0.72f)
      : GameUITheme.BgPanelLight;

    if (complete)
      fill = new Color(GameUITheme.Heal.r, GameUITheme.Heal.g, GameUITheme.Heal.b, 0.85f);
    else if (current)
      fill = new Color(0.95f, 0.97f, 1f, 0.98f);

    if (_nodeImages[index] != null)
      _nodeImages[index].color = fill;

    if (_iconTexts[index] != null)
      _iconTexts[index].color = current
        ? GameUITheme.BgDeep
        : complete ? Color.white : baseColor;

    if (_labelTexts[index] != null)
    {
      _labelTexts[index].color = current
        ? GameUITheme.TextPrimary
        : complete ? GameUITheme.Heal : GameUITheme.TextMuted;
      _labelTexts[index].fontStyle = current ? FontStyles.Bold : FontStyles.Normal;
      _labelTexts[index].text = node.optional ? $"{node.label} <size=75%>(선택)</size>" : node.label;
    }
  }

  private static Color GetKindColor(NodeKind kind) =>
    kind switch
    {
      NodeKind.Start => GameUITheme.Accent,
      NodeKind.Combat => GameUITheme.AllInBet,
      NodeKind.Augment => GameUITheme.RiskBet,
      NodeKind.Event => GameUITheme.AccentHover,
      NodeKind.Boss => GameUITheme.HpLow,
      _ => GameUITheme.TextPrimary
    };

  private enum NodeKind
  {
    Start,
    Combat,
    Augment,
    Event,
    Boss
  }

  private readonly struct MapNode
  {
    public readonly string label;
    public readonly string icon;
    public readonly NodeKind kind;
    public readonly bool optional;

    public MapNode(string label, string icon, NodeKind kind, bool optional)
    {
      this.label = label;
      this.icon = icon;
      this.kind = kind;
      this.optional = optional;
    }
  }
}

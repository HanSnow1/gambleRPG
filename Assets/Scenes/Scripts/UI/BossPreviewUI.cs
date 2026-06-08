using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Boss preview before combat. Role C calls ShowPreview(boss) from RunManager / GameFlowController.
/// </summary>
[DefaultExecutionOrder(-50)]
public class BossPreviewUI : MonoBehaviour
{
  private static readonly string[] CombatHudNames =
  {
    "PlayerHpText",
    "EnemyHpText",
    "CombatLogText",
    "StatusText",
    "BossRuleText",
    "BetSmall",
    "BetMedium",
    "BetLarge"
  };

  [Header("References")]
  [SerializeField] private CombatUI combatUI;
  [SerializeField] private GameObject previewPanel;

  [Header("Preview text (optional — auto-find under previewPanel)")]
  [SerializeField] private TMP_Text titleText;
  [SerializeField] private TMP_Text bossNameText;
  [SerializeField] private TMP_Text ruleText;
  [SerializeField] private TMP_Text hintsText;

  [Header("Buttons (optional — auto-wire by name)")]
  [SerializeField] private Button startFightButton;
  [SerializeField] private Button previewDiceButton;
  [SerializeField] private Button previewCardButton;

  [Header("Boss for first preview")]
  [SerializeField] private BossDefinition defaultBoss;
  [SerializeField] private BossDefinition diceBoss;
  [SerializeField] private BossDefinition cardBoss;

  public BossDefinition DiceBoss => diceBoss;
  public BossDefinition CardBoss => cardBoss;

  private BossDefinition _currentBoss;
  private bool _isFinalBossPreview;
  private readonly List<GameObject> _combatHudRoots = new();

  private void Awake()
  {
    if (combatUI == null)
      combatUI = FindFirstObjectByType<CombatUI>();

    EnsurePreviewPanelExists();
    ResolvePreviewReferences();
    CacheCombatHudRoots();
    SetCombatHudVisible(false);
  }

  private void Start()
  {
    WireButtons();

    if (FindFirstObjectByType<GameFlowController>() != null)
    {
      HidePreview();
      return;
    }

    if (_currentBoss == null)
      _currentBoss = defaultBoss != null ? defaultBoss : diceBoss;

    if (_currentBoss != null)
      ShowPreview(_currentBoss);
    else
      Debug.LogWarning("BossPreviewUI: Assign Default Boss or Dice/Card boss assets in Inspector.");
  }

  public void HidePreview()
  {
    if (previewPanel != null)
      previewPanel.SetActive(false);
    SetCombatHudVisible(false);
  }

  public void ShowCombatHud() => SetCombatHudVisible(true);

  /// <summary>Called by RunManager (Role C) when the run's boss is chosen.</summary>
  public void ShowPreview(BossDefinition boss, bool finalBoss = false, int normalFightsCompleted = 0)
  {
    _currentBoss = boss != null ? boss : GetFloorBossFallback();
    _isFinalBossPreview = finalBoss;
    SetCombatHudVisible(false);

    if (previewPanel != null)
      previewPanel.SetActive(true);

    RefreshPreviewText(normalFightsCompleted);
    WireButtons();
  }

  public void OnStartFight()
  {
    if (_currentBoss == null && !_isFinalBossPreview)
    {
      Debug.LogWarning("BossPreviewUI: No boss selected.");
      return;
    }

    var flow = FindFirstObjectByType<GameFlowController>();
    if (flow != null)
    {
      flow.BeginRunCombat();
      return;
    }

    if (_currentBoss == null)
    {
      Debug.LogWarning("BossPreviewUI: No boss selected.");
      return;
    }

    if (previewPanel != null)
      previewPanel.SetActive(false);

    SetCombatHudVisible(true);

    if (RunManager.Instance != null)
    {
      RunManager.Instance.State.assignedBoss = _currentBoss;
      RunManager.Instance.SetPhase(RunPhase.Combat);
    }

    if (combatUI != null)
      combatUI.BeginCombat(_currentBoss);
    else
      Debug.LogError("BossPreviewUI: CombatUI not found.");
  }

  public void OnPreviewDice()
  {
    if (diceBoss != null)
      ShowPreview(diceBoss);
  }

  public void OnPreviewCard()
  {
    if (cardBoss != null)
      ShowPreview(cardBoss);
  }

  private void RefreshPreviewText(int normalFightsCompleted = 0)
  {
    ResolvePreviewReferences();

    if (_currentBoss == null)
      _currentBoss = GetFloorBossFallback();

    if (_currentBoss == null)
      return;

    int floor = RunManager.Instance != null ? RunManager.Instance.State.currentFloor : 1;
    int totalFloors = RunState.TotalFloors;
    bool isRunFinalBoss = _isFinalBossPreview && floor >= totalFloors;

    if (_isFinalBossPreview)
    {
      SetText(titleText, isRunFinalBoss ? GameUIText.FinalBossTitle(floor) : GameUIText.FloorBossTitle(floor));
      SetText(bossNameText, GameUIText.BossHpLine(_currentBoss.displayName, _currentBoss.maxHp));
      SetText(ruleText, BossGambleResolvers.GetRuleLabel(_currentBoss));
      SetText(
        hintsText,
        (isRunFinalBoss ? GameUIText.FinalBossIntro : GameUIText.FloorBossIntro(floor)) +
        $"{GameUIText.Weakness(_currentBoss.weaknessHint)}\n\n" +
        BossPreviewHints.BuildPatternHints(_currentBoss));
      return;
    }

    int total = RunManager.Instance != null
      ? RunManager.Instance.State.normalFightsBeforeBoss
      : RunState.DefaultNormalFightsBeforeBoss;

    SetText(titleText, GameUIText.PreviewFloor(floor, totalFloors));
    SetText(bossNameText, GameUIText.BossHpLine(_currentBoss.displayName, _currentBoss.maxHp));
    SetText(ruleText, BossGambleResolvers.GetRuleLabel(_currentBoss));
    SetText(
      hintsText,
      GameUIText.FloorBossPreview(floor, _currentBoss.displayName, total, normalFightsCompleted) +
      $"{GameUIText.Weakness(_currentBoss.weaknessHint)}\n\n" +
      BossPreviewHints.BuildPatternHints(_currentBoss));
  }

  private static void SetText(TMP_Text text, string value)
  {
    if (text != null)
      text.text = value;
  }

  private void WireButtons()
  {
    BindButton(startFightButton, "StartFightButton", OnStartFight);
    BindButton(previewDiceButton, "PreviewDiceButton", OnPreviewDice);
    BindButton(previewCardButton, "PreviewCardButton", OnPreviewCard);
  }

  private void BindButton(Button button, string objectName, UnityEngine.Events.UnityAction action)
  {
    if (button == null && previewPanel != null)
      button = FindButtonInPanel(objectName);

    if (button == null)
      button = GameObject.Find(objectName)?.GetComponent<Button>();

    if (button == null)
      return;

    button.onClick.RemoveListener(action);
    button.onClick.AddListener(action);
  }

  private void ResolvePreviewReferences()
  {
    if (previewPanel == null)
      return;

    titleText ??= FindTmpInPanel("PreviewTitleText");
    if (titleText == null)
      titleText = FindTmpRecursive(previewPanel.transform, "title");

    bossNameText ??= FindTmpInPanel("PreviewBossNameText");
    if (bossNameText == null)
      bossNameText = FindTmpRecursive(previewPanel.transform, "boss");

    ruleText ??= FindTmpInPanel("PreviewRuleText");
    if (ruleText == null)
      ruleText = FindTmpRecursive(previewPanel.transform, "rule");

    hintsText ??= FindTmpInPanel("PreviewHintsText");
    if (hintsText == null)
      hintsText = FindTmpRecursive(previewPanel.transform, "hint");

    startFightButton ??= FindButtonInPanel("StartFightButton");
    if (startFightButton == null)
      startFightButton = FindButtonRecursive(previewPanel.transform, "start");

    previewDiceButton ??= FindButtonInPanel("PreviewDiceButton");
    previewCardButton ??= FindButtonInPanel("PreviewCardButton");
  }

  private TMP_Text FindTmpInPanel(string childName) =>
    previewPanel.transform.Find(childName)?.GetComponent<TMP_Text>();

  private Button FindButtonInPanel(string childName) =>
    previewPanel.transform.Find(childName)?.GetComponent<Button>();

  private BossDefinition GetFloorBossFallback()
  {
    int floor = RunManager.Instance != null && RunManager.Instance.State.isActive
      ? RunManager.Instance.State.currentFloor
      : 1;
    return floor <= 1 ? diceBoss : cardBoss;
  }

  private static TMP_Text FindTmpRecursive(Transform root, string nameTokenLower)
  {
    if (root == null)
      return null;

    var tmp = root.GetComponent<TMP_Text>();
    if (tmp != null && root.name.ToLower().Contains(nameTokenLower))
      return tmp;

    for (int i = 0; i < root.childCount; i++)
    {
      var found = FindTmpRecursive(root.GetChild(i), nameTokenLower);
      if (found != null)
        return found;
    }

    return null;
  }

  private static Button FindButtonRecursive(Transform root, string nameTokenLower)
  {
    if (root == null)
      return null;

    var btn = root.GetComponent<Button>();
    if (btn != null && root.name.ToLower().Contains(nameTokenLower))
      return btn;

    for (int i = 0; i < root.childCount; i++)
    {
      var found = FindButtonRecursive(root.GetChild(i), nameTokenLower);
      if (found != null)
        return found;
    }

    return null;
  }

  private void EnsurePreviewPanelExists()
  {
    if (previewPanel != null)
      return;

    var canvas = GetComponent<Canvas>() ?? FindFirstObjectByType<Canvas>();
    if (canvas == null)
    {
      Debug.LogError("BossPreviewUI: No Canvas found.");
      return;
    }

    previewPanel = BuildRuntimePreviewPanel(canvas.transform);
    ResolvePreviewReferences();
  }

  private static GameObject BuildRuntimePreviewPanel(Transform canvasRoot)
  {
    var panel = new GameObject("BossPreviewPanel", typeof(RectTransform), typeof(Image));
    panel.transform.SetParent(canvasRoot, false);

    var panelRect = panel.GetComponent<RectTransform>();
    panelRect.anchorMin = Vector2.zero;
    panelRect.anchorMax = Vector2.one;
    panelRect.offsetMin = Vector2.zero;
    panelRect.offsetMax = Vector2.zero;

    var bg = panel.GetComponent<Image>();
    bg.sprite = LoadSprite("UI/dice_boss_preview");
    bg.color = bg.sprite != null ? Color.white : GameUITheme.BgPanel;
    bg.preserveAspect = false;

    var scrim = new GameObject("PreviewScrim", typeof(RectTransform), typeof(Image));
    scrim.transform.SetParent(panel.transform, false);
    var scrimRt = scrim.GetComponent<RectTransform>();
    scrimRt.anchorMin = Vector2.zero;
    scrimRt.anchorMax = Vector2.one;
    scrimRt.offsetMin = Vector2.zero;
    scrimRt.offsetMax = Vector2.zero;
    scrim.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.22f);

    var contentBox = new GameObject("PreviewContent", typeof(RectTransform), typeof(Image));
    contentBox.transform.SetParent(panel.transform, false);
    var boxRt = contentBox.GetComponent<RectTransform>();
    boxRt.anchorMin = boxRt.anchorMax = new Vector2(0.5f, 0.5f);
    boxRt.anchoredPosition = new Vector2(-330, 10);
    boxRt.sizeDelta = new Vector2(470, 540);
    GameUITheme.ApplyRoundedFrameStyle(contentBox.GetComponent<Image>(), new Color(GameUITheme.BgPanel.r, GameUITheme.BgPanel.g, GameUITheme.BgPanel.b, 0.84f));

    GameUITheme.CreateText(contentBox.transform, "PreviewTitleText", GameUIText.PreviewTitle, 30, FontStyles.Bold,
      TextAlignmentOptions.Top);
    var titleRt = contentBox.transform.Find("PreviewTitleText").GetComponent<RectTransform>();
    titleRt.anchorMin = new Vector2(0, 1);
    titleRt.anchorMax = new Vector2(1, 1);
    titleRt.pivot = new Vector2(0.5f, 1);
    titleRt.anchoredPosition = new Vector2(0, -20);
    titleRt.sizeDelta = new Vector2(-40, 44);
    contentBox.transform.Find("PreviewTitleText").GetComponent<TMP_Text>().color = GameUITheme.Accent;

    GameUITheme.CreateText(contentBox.transform, "PreviewBossNameText", "보스 이름", 22, FontStyles.Bold,
      TextAlignmentOptions.Top);
    var nameRt = contentBox.transform.Find("PreviewBossNameText").GetComponent<RectTransform>();
    nameRt.anchorMin = new Vector2(0, 1);
    nameRt.anchorMax = new Vector2(1, 1);
    nameRt.pivot = new Vector2(0.5f, 1);
    nameRt.anchoredPosition = new Vector2(0, -76);
    nameRt.sizeDelta = new Vector2(-40, 48);

    GameUITheme.CreateText(contentBox.transform, "PreviewRuleText", "보스 규칙", 16, FontStyles.Italic,
      TextAlignmentOptions.Top);
    var ruleRt = contentBox.transform.Find("PreviewRuleText").GetComponent<RectTransform>();
    ruleRt.anchorMin = new Vector2(0, 1);
    ruleRt.anchorMax = new Vector2(1, 1);
    ruleRt.pivot = new Vector2(0.5f, 1);
    ruleRt.anchoredPosition = new Vector2(0, -132);
    ruleRt.sizeDelta = new Vector2(-40, 42);
    contentBox.transform.Find("PreviewRuleText").GetComponent<TMP_Text>().color = GameUITheme.TextMuted;

    GameUITheme.CreateText(contentBox.transform, "PreviewHintsText", "힌트...", 14, FontStyles.Normal,
      TextAlignmentOptions.TopLeft);
    var hintsRt = contentBox.transform.Find("PreviewHintsText").GetComponent<RectTransform>();
    hintsRt.anchorMin = new Vector2(0, 0);
    hintsRt.anchorMax = new Vector2(1, 1);
    hintsRt.offsetMin = new Vector2(24, 102);
    hintsRt.offsetMax = new Vector2(-24, -190);
    contentBox.transform.Find("PreviewHintsText").GetComponent<TMP_Text>().color = GameUITheme.TextPrimary;

    GameUITheme.CreateButton(contentBox.transform, "StartFightButton", GameUIText.StartFight, new Vector2(0, -220),
      new Vector2(260, 48), GameUITheme.Accent);

    return panel;
  }

  private static Sprite LoadSprite(string resourcePath)
  {
    var tex = Resources.Load<Texture2D>(resourcePath);
    if (tex == null)
      return null;

    return Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
  }

  private static void CreateTmp(
    Transform parent,
    string name,
    string text,
    int fontSize,
    Vector2 anchoredPos,
    Vector2 size,
    FontStyles fontStyle = FontStyles.Normal,
    TextAlignmentOptions alignment = TextAlignmentOptions.Center)
  {
    var go = new GameObject(name, typeof(RectTransform));
    go.transform.SetParent(parent, false);

    var rect = go.GetComponent<RectTransform>();
    rect.anchorMin = new Vector2(0.5f, 0.5f);
    rect.anchorMax = new Vector2(0.5f, 0.5f);
    rect.anchoredPosition = anchoredPos;
    rect.sizeDelta = size;

    var tmp = go.AddComponent<TextMeshProUGUI>();
    if (TMP_Settings.defaultFontAsset != null)
      KoreanFontSetup.Apply(tmp);
    tmp.text = text;
    tmp.fontSize = fontSize;
    tmp.fontStyle = fontStyle;
    tmp.alignment = alignment;
    tmp.color = Color.white;
    tmp.textWrappingMode = TextWrappingModes.Normal;
  }

  private static void CreateButton(Transform parent, string name, string label, Vector2 anchoredPos, Vector2 size)
  {
    var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
    go.transform.SetParent(parent, false);

    var rect = go.GetComponent<RectTransform>();
    rect.anchorMin = new Vector2(0.5f, 0.5f);
    rect.anchorMax = new Vector2(0.5f, 0.5f);
    rect.anchoredPosition = anchoredPos;
    rect.sizeDelta = size;

    var image = go.GetComponent<Image>();
    GameUITheme.ApplyRoundedFrameStyle(image, new Color(0.16f, 0.2f, 0.34f, 0.85f));

    var labelGo = new GameObject("Text", typeof(RectTransform));
    labelGo.transform.SetParent(go.transform, false);
    var labelRect = labelGo.GetComponent<RectTransform>();
    labelRect.anchorMin = Vector2.zero;
    labelRect.anchorMax = Vector2.one;
    labelRect.offsetMin = Vector2.zero;
    labelRect.offsetMax = Vector2.zero;

    var tmp = labelGo.AddComponent<TextMeshProUGUI>();
    if (TMP_Settings.defaultFontAsset != null)
      KoreanFontSetup.Apply(tmp);
    tmp.text = label;
    tmp.fontSize = 16;
    tmp.alignment = TextAlignmentOptions.Center;
    tmp.color = Color.white;
  }

  private void CacheCombatHudRoots()
  {
    _combatHudRoots.Clear();

    var canvas = GetComponent<Canvas>() ?? FindFirstObjectByType<Canvas>();
    if (canvas == null)
      return;

    foreach (var objectName in CombatHudNames)
    {
      var t = FindChildRecursive(canvas.transform, objectName);
      if (t != null)
        _combatHudRoots.Add(t.gameObject);
    }
  }

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

  private void SetCombatHudVisible(bool visible)
  {
    if (_combatHudRoots.Count == 0)
      CacheCombatHudRoots();

    foreach (var go in _combatHudRoots)
    {
      if (go != null && go != previewPanel)
        go.SetActive(visible);
    }
  }
}

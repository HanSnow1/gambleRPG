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
    "BetLarge",
    "RestartButton"
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

  /// <summary>Called by RunManager (Role C) when the run's boss is chosen.</summary>
  public void ShowPreview(BossDefinition boss)
  {
    _currentBoss = boss;
    SetCombatHudVisible(false);

    if (previewPanel != null)
      previewPanel.SetActive(true);

    RefreshPreviewText();
    WireButtons();
  }

  public void OnStartFight()
  {
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
      RunManager.Instance.AdvanceFloor();
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

  private void RefreshPreviewText()
  {
    if (_currentBoss == null)
      return;

    SetText(titleText, "BOSS PREVIEW");
    SetText(bossNameText, $"{_currentBoss.displayName}  |  HP {_currentBoss.maxHp}");
    SetText(ruleText, BossGambleResolvers.GetRuleLabel(_currentBoss));
    SetText(
      hintsText,
      $"Weakness: {_currentBoss.weaknessHint}\n\n" +
      $"Pattern A: {_currentBoss.riskPatternHintA}\n\n" +
      $"Pattern B: {_currentBoss.riskPatternHintB}");
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
    bossNameText ??= FindTmpInPanel("PreviewBossNameText");
    ruleText ??= FindTmpInPanel("PreviewRuleText");
    hintsText ??= FindTmpInPanel("PreviewHintsText");
    startFightButton ??= FindButtonInPanel("StartFightButton");
    previewDiceButton ??= FindButtonInPanel("PreviewDiceButton");
    previewCardButton ??= FindButtonInPanel("PreviewCardButton");
  }

  private TMP_Text FindTmpInPanel(string childName) =>
    previewPanel.transform.Find(childName)?.GetComponent<TMP_Text>();

  private Button FindButtonInPanel(string childName) =>
    previewPanel.transform.Find(childName)?.GetComponent<Button>();

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
    bg.color = new Color(0.08f, 0.08f, 0.12f, 0.92f);

    CreateTmp(panel.transform, "PreviewTitleText", "BOSS PREVIEW", 28, new Vector2(0, 220), new Vector2(700, 50),
      FontStyles.Bold);
    CreateTmp(panel.transform, "PreviewBossNameText", "Boss Name", 22, new Vector2(0, 170), new Vector2(700, 40));
    CreateTmp(panel.transform, "PreviewRuleText", "Boss rule", 18, new Vector2(0, 130), new Vector2(700, 36),
      FontStyles.Italic);
    CreateTmp(panel.transform, "PreviewHintsText", "Hints...", 14, new Vector2(0, -20), new Vector2(720, 260),
      alignment: TextAlignmentOptions.TopLeft);

    CreateButton(panel.transform, "StartFightButton", "START FIGHT", new Vector2(0, -200), new Vector2(220, 44));
    CreateButton(panel.transform, "PreviewDiceButton", "Preview Dice", new Vector2(-130, -250), new Vector2(160, 36));
    CreateButton(panel.transform, "PreviewCardButton", "Preview Card", new Vector2(130, -250), new Vector2(160, 36));

    return panel;
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
      tmp.font = TMP_Settings.defaultFontAsset;
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
    image.color = new Color(0.25f, 0.45f, 0.75f, 1f);

    var labelGo = new GameObject("Text", typeof(RectTransform));
    labelGo.transform.SetParent(go.transform, false);
    var labelRect = labelGo.GetComponent<RectTransform>();
    labelRect.anchorMin = Vector2.zero;
    labelRect.anchorMax = Vector2.one;
    labelRect.offsetMin = Vector2.zero;
    labelRect.offsetMax = Vector2.zero;

    var tmp = labelGo.AddComponent<TextMeshProUGUI>();
    if (TMP_Settings.defaultFontAsset != null)
      tmp.font = TMP_Settings.defaultFontAsset;
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

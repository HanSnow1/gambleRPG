using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Title -> opening augment -> boss preview -> normal fights -> augments -> boss -> victory/game over.
/// </summary>
[DefaultExecutionOrder(-150)]
public class GameFlowController : MonoBehaviour
{
  [Header("References")]
  [SerializeField] private RunManager runManager;
  [SerializeField] private BossPreviewUI bossPreviewUI;
  [SerializeField] private AugmentPickUI augmentPickUI;
  [SerializeField] private CombatUI combatUI;

  [Header("Flow")]
  [SerializeField] private bool requireOpeningAugment = true;

  [Header("Title (auto-created if empty)")]
  [SerializeField] private GameObject titlePanel;

  [Header("End screens (auto-created if empty)")]
  [SerializeField] private GameObject victoryPanel;
  [SerializeField] private GameObject gameOverPanel;
  [SerializeField] private GameObject stageClearPanel;

  private TMP_Text _stageClearTitleText;
  private TMP_Text _stageClearBodyText;

  [Header("Boss assets (fallback if RunManager pool empty)")]
  [SerializeField] private BossDefinition diceBoss;
  [SerializeField] private BossDefinition cardBoss;

  private void Awake()
  {
    if (runManager == null)
      runManager = GetComponent<RunManager>();
    if (runManager == null)
      runManager = gameObject.AddComponent<RunManager>();

    if (bossPreviewUI == null)
      bossPreviewUI = FindFirstObjectByType<BossPreviewUI>();

    if (combatUI == null)
      combatUI = FindFirstObjectByType<CombatUI>();

    if (bossPreviewUI != null)
    {
      if (diceBoss == null)
        diceBoss = bossPreviewUI.DiceBoss;
      if (cardBoss == null)
        cardBoss = bossPreviewUI.CardBoss;
    }

    if (diceBoss != null && cardBoss != null)
      runManager.SetBossPool(diceBoss, cardBoss);

    EnsureAugmentServices();
    EnsureTitlePanel();
    EnsureEndPanels();
    HideEndPanels();
    ApplyPhase(RunPhase.Title);
  }

  private void OnEnable()
  {
    if (runManager == null)
      return;
    runManager.OnRunStarted += HandleRunStarted;
    runManager.OnStateChanged += HandleStateChanged;
  }

  private void OnDisable()
  {
    if (runManager == null)
      return;
    runManager.OnRunStarted -= HandleRunStarted;
    runManager.OnStateChanged -= HandleStateChanged;
  }

  public void OnStartRunClicked()
  {
    HideEndPanels();
    runManager.StartNewRun();
  }

  /// <summary>Called by BossPreviewUI START FIGHT.</summary>
  public void BeginRunCombat()
  {
    HideEndPanels();
    augmentPickUI?.Hide();

    if (runManager == null || !runManager.State.isActive)
      return;

    bossPreviewUI?.HidePreview();

    if (runManager.State.IsBossFightNext)
      StartBossCombat();
    else
      StartNormalCombat();
  }

  public void HandleCombatVictory()
  {
    if (runManager == null || !runManager.State.isActive)
      return;

    SyncCombatHpToRun();

    if (runManager.State.phase == RunPhase.BossCombat)
    {
      if (runManager.State.IsFinalFloor)
      {
        runManager.SetPhase(RunPhase.Victory);
        ShowVictoryScreen();
        return;
      }

      int floorHpBefore = runManager.State.currentHp;
      int floorMaxHp = runManager.State.maxHp;
      int floorHealed = runManager.AdvanceToNextFloor();
      int floorHpAfter = runManager.State.currentHp;

      runManager.SetPhase(RunPhase.FloorClear);
      ShowFloorClearScreen(floorHpBefore, floorHpAfter, floorHealed, floorMaxHp);
      return;
    }

    runManager.RecordNormalFightVictory();

    int hpBefore = runManager.State.currentHp;
    int maxHp = runManager.State.maxHp;
    int healed = runManager.ApplyBetweenFightHeal();
    int hpAfter = runManager.State.currentHp;

    runManager.SetPhase(RunPhase.StageClear);
    ShowStageClearScreen(hpBefore, hpAfter, healed, maxHp);
  }

  private void OnContinueFromStageClear()
  {
    if (stageClearPanel != null)
      stageClearPanel.SetActive(false);

    runManager.SetPhase(RunPhase.AugmentPick);
    ShowPostCombatAugmentPick();
  }

  private void OnContinueFromFloorClear()
  {
    if (stageClearPanel != null)
      stageClearPanel.SetActive(false);

    runManager.SetPhase(RunPhase.BossPreview);
    ShowBossPreview(runManager.State, false);
  }

  private void ShowStageClearScreen(int hpBefore, int hpAfter, int healed, int maxHp)
  {
    if (stageClearPanel == null)
      EnsureEndPanels();

    HideTitleAndPreview();
    bossPreviewUI?.HidePreview();
    augmentPickUI?.Hide();
    if (victoryPanel != null)
      victoryPanel.SetActive(false);
    if (gameOverPanel != null)
      gameOverPanel.SetActive(false);

    if (_stageClearTitleText != null)
      _stageClearTitleText.text = GameUIText.StageClear;

    if (_stageClearBodyText != null)
    {
      int completed = runManager.State.normalFightsCompleted;
      int total = runManager.State.normalFightsBeforeBoss;
      int floor = runManager.State.currentFloor;
      bool bossNext = runManager.State.IsBossFightNext;

      string healLine = healed > 0
        ? GameUIText.HpRecovered(healed, CombatBalance.BetweenFightHealPercent)
        : GameUIText.HpFullNoHeal;

      string nextLine = bossNext
        ? GameUIText.NextBossLine(floor)
        : GameUIText.NextAugmentLine;

      _stageClearBodyText.text = GameUIText.StageClearBody(
        floor, completed, total, hpBefore, hpAfter, maxHp, healLine, nextLine);

      _stageClearBodyText.richText = true;
    }

    if (stageClearPanel != null)
    {
      WireStageClearContinueButton(OnContinueFromStageClear);
      stageClearPanel.transform.SetAsLastSibling();
      stageClearPanel.SetActive(true);
    }
  }

  private void ShowFloorClearScreen(int hpBefore, int hpAfter, int healed, int maxHp)
  {
    if (stageClearPanel == null)
      EnsureEndPanels();

    HideTitleAndPreview();
    bossPreviewUI?.HidePreview();
    augmentPickUI?.Hide();
    if (victoryPanel != null)
      victoryPanel.SetActive(false);
    if (gameOverPanel != null)
      gameOverPanel.SetActive(false);

    int clearedFloor = runManager.State.currentFloor - 1;
    var nextBoss = runManager.State.assignedBoss;

    if (_stageClearTitleText != null)
      _stageClearTitleText.text = GameUIText.FloorClearTitle(clearedFloor);

    if (_stageClearBodyText != null)
    {
      string healLine = healed > 0
        ? GameUIText.HpRecovered(healed, CombatBalance.BetweenFloorHealPercent)
        : GameUIText.HpFullNoHeal;

      string nextBossName = nextBoss != null ? nextBoss.displayName : "???";

      _stageClearBodyText.text = GameUIText.FloorClearBody(
        clearedFloor, hpBefore, hpAfter, maxHp, healLine,
        runManager.State.currentFloor, nextBossName);

      _stageClearBodyText.richText = true;
    }

    if (stageClearPanel != null)
    {
      WireStageClearContinueButton(OnContinueFromFloorClear);
      stageClearPanel.transform.SetAsLastSibling();
      stageClearPanel.SetActive(true);
    }
  }

  private void WireStageClearContinueButton(UnityEngine.Events.UnityAction onClick)
  {
    if (stageClearPanel == null)
      return;

    var btn = stageClearPanel.transform.Find("StageClearContinueButton")?.GetComponent<Button>();
    if (btn == null)
      return;

    btn.onClick.RemoveAllListeners();
    btn.onClick.AddListener(onClick);
  }

  public void HandleCombatDefeat()
  {
    if (runManager == null)
      return;

    SyncCombatHpToRun();
    runManager.SetPhase(RunPhase.GameOver);
    ShowGameOverScreen();
  }

  /// <summary>Restart from combat: new run + opening augment pick.</summary>
  public void RestartFromCombat()
  {
    var combat = FindFirstObjectByType<BettingCombatSystem>();
    combat?.EndCombat();

    HideEndPanels();
    augmentPickUI?.Hide();
    bossPreviewUI?.HidePreview();
    PlayerAugmentState.Instance?.ClearAll();
    PlayerRelicState.Instance?.ClearAll();

    runManager.StartNewRun();
  }

  public void ReturnToTitle()
  {
    var combat = FindFirstObjectByType<BettingCombatSystem>();
    combat?.EndCombat();

    HideEndPanels();
    augmentPickUI?.Hide();
    bossPreviewUI?.HidePreview();
    PlayerAugmentState.Instance?.ClearAll();
    PlayerRelicState.Instance?.ClearAll();

    runManager.EndRunToTitle();
    ApplyPhase(RunPhase.Title);
  }

  private void StartNormalCombat()
  {
    runManager.SetPhase(RunPhase.Combat);

    if (combatUI != null)
    {
      combatUI.BeginCombat(null, isBossFight: false);
      return;
    }

    Debug.LogError("GameFlowController: CombatUI not found.");
  }

  private void StartBossCombat()
  {
    var boss = runManager.State.assignedBoss;
    if (boss == null)
    {
      Debug.LogError("GameFlowController: No assigned boss for final fight.");
      return;
    }

    runManager.SetPhase(RunPhase.BossCombat);

    if (combatUI != null)
      combatUI.BeginCombat(boss, isBossFight: true);
  }

  private void ShowPostCombatAugmentPick()
  {
    if (augmentPickUI == null)
    {
      ContinueAfterAugment(null);
      return;
    }

    augmentPickUI.ShowPick(OnMidRunAugmentPicked);
  }

  private void EnsureAugmentServices()
  {
    if (augmentPickUI == null)
      augmentPickUI = FindFirstObjectByType<AugmentPickUI>();

    if (augmentPickUI == null)
    {
      var canvas = FindFirstObjectByType<Canvas>();
      if (canvas != null)
        augmentPickUI = canvas.gameObject.AddComponent<AugmentPickUI>();
    }

    if (runManager != null && runManager.GetComponent<PlayerAugmentState>() == null)
      runManager.gameObject.AddComponent<PlayerAugmentState>();

    if (runManager != null && runManager.GetComponent<PlayerRelicState>() == null)
      runManager.gameObject.AddComponent<PlayerRelicState>();
  }

  private void GrantStarterRelicIfNeeded()
  {
    if (PlayerRelicState.Instance == null || PlayerRelicState.Instance.HasAnyRelic())
      return;

    var granted = PlayerRelicState.Instance.GrantRandomCommonRelic();
    if (granted != null)
      Debug.Log($"Starter relic: {granted.displayName}");
  }

  private void HandleRunStarted(RunState state)
  {
    HideEndPanels();
    PlayerAugmentState.Instance?.ResetRunCounters();
    GrantStarterRelicIfNeeded();

    if (requireOpeningAugment && augmentPickUI != null)
    {
      runManager.SetPhase(RunPhase.AugmentPick);
      if (titlePanel != null)
        titlePanel.SetActive(false);
      bossPreviewUI?.HidePreview();
      augmentPickUI.ShowPick(OnOpeningAugmentPicked);
      return;
    }

    ShowBossPreview(state, false);
  }

  private void OnOpeningAugmentPicked(AugmentDefinition picked)
  {
    ApplyAugmentPick(picked, "Opening augment");
    runManager.SetPhase(RunPhase.BossPreview);
  }

  private void OnMidRunAugmentPicked(AugmentDefinition picked)
  {
    ApplyAugmentPick(picked, "Augment");
    ContinueAfterAugment(picked);
  }

  private void ApplyAugmentPick(AugmentDefinition picked, string logPrefix)
  {
    if (picked == null)
      return;

    PlayerAugmentState.Instance?.AddOrLevelUp(picked);
    PlayerAugmentState.Instance?.ResetRunCounters();
    runManager.TryAddAugment(picked.augmentId);
    Debug.Log($"{logPrefix}: {picked.displayName}");
  }

  private void ContinueAfterAugment(AugmentDefinition _)
  {
    if (runManager == null || !runManager.State.isActive)
      return;

    if (runManager.State.IsBossFightNext)
    {
      runManager.SetPhase(RunPhase.BossPreview);
      ShowBossPreview(runManager.State, true);
      return;
    }

    StartNormalCombat();
  }

  private void HandleStateChanged(RunState state)
  {
    if (state.phase == RunPhase.Title)
      ApplyPhase(RunPhase.Title);
    else if (state.phase == RunPhase.StageClear || state.phase == RunPhase.FloorClear)
    {
      // Panel content set when phase changes.
    }
    else if (state.phase == RunPhase.AugmentPick)
    {
      if (titlePanel != null)
        titlePanel.SetActive(false);
      bossPreviewUI?.HidePreview();
      if (stageClearPanel != null)
        stageClearPanel.SetActive(false);
      if (victoryPanel != null)
        victoryPanel.SetActive(false);
      if (gameOverPanel != null)
        gameOverPanel.SetActive(false);
    }
    else if (state.phase == RunPhase.BossPreview)
      ShowBossPreview(state, state.IsBossFightNext);
    else if (state.phase == RunPhase.Combat || state.phase == RunPhase.BossCombat)
    {
      HideTitleAndPreview();
      HideEndPanels();
    }
    else if (state.phase == RunPhase.Victory)
      ShowVictoryScreen();
    else if (state.phase == RunPhase.GameOver)
      ShowGameOverScreen();
  }

  private void ShowBossPreview(RunState state, bool finalBoss)
  {
    if (titlePanel != null)
      titlePanel.SetActive(false);

    HideEndPanels();

    if (bossPreviewUI != null && state.assignedBoss != null)
    {
      bossPreviewUI.ShowPreview(state.assignedBoss, finalBoss, state.normalFightsCompleted);
      return;
    }

    Debug.LogWarning("GameFlowController: BossPreviewUI or assigned boss is missing.");
  }

  private void ShowVictoryScreen()
  {
    HideTitleAndPreview();
    bossPreviewUI?.HidePreview();
    augmentPickUI?.Hide();
    if (gameOverPanel != null)
      gameOverPanel.SetActive(false);
    if (stageClearPanel != null)
      stageClearPanel.SetActive(false);

    var victoryBody = victoryPanel != null
      ? victoryPanel.transform.Find("VictoryBody")?.GetComponent<TMP_Text>()
      : null;
    if (victoryBody != null)
      victoryBody.text = GameUIText.VictoryFullBody;

    if (victoryPanel != null)
      victoryPanel.SetActive(true);
  }

  private void ShowGameOverScreen()
  {
    HideTitleAndPreview();
    bossPreviewUI?.HidePreview();
    augmentPickUI?.Hide();
    if (victoryPanel != null)
      victoryPanel.SetActive(false);
    if (stageClearPanel != null)
      stageClearPanel.SetActive(false);
    if (gameOverPanel != null)
      gameOverPanel.SetActive(true);
  }

  private void HideEndPanels()
  {
    if (stageClearPanel != null)
      stageClearPanel.SetActive(false);
    if (victoryPanel != null)
      victoryPanel.SetActive(false);
    if (gameOverPanel != null)
      gameOverPanel.SetActive(false);
  }

  private void SyncCombatHpToRun()
  {
    var combat = FindFirstObjectByType<BettingCombatSystem>();
    if (combat == null || runManager == null)
      return;

    runManager.SyncHpFromCombat(combat.State.playerHp, combat.State.playerMaxHp);
  }

  private void HideTitleAndPreview()
  {
    if (titlePanel != null)
      titlePanel.SetActive(false);
  }

  private void ApplyPhase(RunPhase phase)
  {
    var showTitle = phase == RunPhase.Title || phase == RunPhase.None;

    if (titlePanel != null)
      titlePanel.SetActive(showTitle);

    if (showTitle)
    {
      bossPreviewUI?.HidePreview();
      HideEndPanels();
    }
  }

  private void EnsureTitlePanel()
  {
    if (titlePanel != null)
      return;

    var canvas = FindFirstObjectByType<Canvas>();
    if (canvas == null)
      return;

    titlePanel = CreateOverlayPanel(canvas.transform, "TitlePanel");
    GameUITheme.ApplyCanvasBackground(canvas.transform);

    var titleTmp = CreateCenteredText(titlePanel.transform, "TitleText", "GambleRogue", 36, new Vector2(0, 170), new Vector2(640, 72),
      FontStyles.Bold);
    titleTmp.color = GameUITheme.Accent;

    CreateCenteredText(titlePanel.transform, "TitleSubtitle", GameUIText.TitleSubtitle, 17, new Vector2(0, 120),
      new Vector2(640, 36));
    CreateCenteredText(
      titlePanel.transform,
      "TitleGuide",
      GameUIText.TitleGuide,
      14,
      new Vector2(0, 50),
      new Vector2(720, 80));
    CreatePanelButton(titlePanel.transform, "StartRunButton", GameUIText.StartRun, new Vector2(0, -40), OnStartRunClicked);
  }

  private void EnsureEndPanels()
  {
    var canvas = FindFirstObjectByType<Canvas>();
    if (canvas == null)
      return;

    if (victoryPanel == null)
    {
      victoryPanel = CreateOverlayPanel(canvas.transform, "VictoryPanel");
      var victoryTitle = CreateCenteredText(victoryPanel.transform, "VictoryTitle", GameUIText.VictoryTitle, 32, new Vector2(0, 100), new Vector2(500, 60),
        FontStyles.Bold);
      victoryTitle.color = GameUITheme.Heal;
      CreateCenteredText(victoryPanel.transform, "VictoryBody", GameUIText.VictoryBody, 18,
        new Vector2(0, 30), new Vector2(600, 80));
      CreatePanelButton(victoryPanel.transform, "VictoryNewRunButton", GameUIText.NewRun, new Vector2(0, -60),
        RestartFromCombat);
      CreatePanelButton(victoryPanel.transform, "VictoryTitleButton", GameUIText.ToTitle, new Vector2(0, -120), ReturnToTitle);
      victoryPanel.SetActive(false);
    }

    if (gameOverPanel == null)
    {
      gameOverPanel = CreateOverlayPanel(canvas.transform, "GameOverPanel");
      var gameOverTitle = CreateCenteredText(gameOverPanel.transform, "GameOverTitle", GameUIText.GameOver, 32, new Vector2(0, 100),
        new Vector2(500, 60), FontStyles.Bold);
      gameOverTitle.color = GameUITheme.HpLow;
      CreateCenteredText(gameOverPanel.transform, "GameOverBody", GameUIText.GameOverBody, 18, new Vector2(0, 30),
        new Vector2(600, 80));
      CreatePanelButton(gameOverPanel.transform, "GameOverNewRunButton", GameUIText.NewRun, new Vector2(0, -60),
        RestartFromCombat);
      CreatePanelButton(gameOverPanel.transform, "GameOverTitleButton", GameUIText.ToTitle, new Vector2(0, -120),
        ReturnToTitle);
      gameOverPanel.SetActive(false);
    }

    if (stageClearPanel == null)
    {
      stageClearPanel = CreateOverlayPanel(canvas.transform, "StageClearPanel");
      _stageClearTitleText = CreateCenteredText(
        stageClearPanel.transform,
        "StageClearTitle",
        GameUIText.StageClear,
        34,
        new Vector2(0, 130),
        new Vector2(560, 56),
        FontStyles.Bold);
      _stageClearTitleText.color = GameUITheme.RiskBet;
      _stageClearBodyText = CreateCenteredText(
        stageClearPanel.transform,
        "StageClearBody",
        "",
        18,
        new Vector2(0, 0),
        new Vector2(560, 180));
      CreatePanelButton(
        stageClearPanel.transform,
        "StageClearContinueButton",
        GameUIText.Continue,
        new Vector2(0, -140),
        OnContinueFromStageClear);
      stageClearPanel.SetActive(false);
    }
  }

  private static GameObject CreateOverlayPanel(Transform canvasRoot, string name)
  {
    var panel = new GameObject(name, typeof(RectTransform), typeof(Image));
    panel.transform.SetParent(canvasRoot, false);

    var rt = panel.GetComponent<RectTransform>();
    rt.anchorMin = Vector2.zero;
    rt.anchorMax = Vector2.one;
    rt.offsetMin = Vector2.zero;
    rt.offsetMax = Vector2.zero;

    panel.GetComponent<Image>().color = new Color(GameUITheme.BgPanel.r, GameUITheme.BgPanel.g, GameUITheme.BgPanel.b, 0.96f);
    return panel;
  }

  private static TextMeshProUGUI CreateCenteredText(
    Transform parent,
    string name,
    string text,
    int fontSize,
    Vector2 anchoredPos,
    Vector2 size,
    FontStyles fontStyle = FontStyles.Normal)
  {
    var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
    go.transform.SetParent(parent, false);

    var rt = go.GetComponent<RectTransform>();
    rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
    rt.anchoredPosition = anchoredPos;
    rt.sizeDelta = size;

    var tmp = go.GetComponent<TextMeshProUGUI>();
    GameUITheme.StyleBodyText(tmp);
    tmp.text = text;
    tmp.fontSize = fontSize;
    tmp.fontStyle = fontStyle;
    tmp.alignment = TextAlignmentOptions.Center;
    tmp.textWrappingMode = TextWrappingModes.Normal;
    return tmp;
  }

  private static void CreatePanelButton(
    Transform parent,
    string name,
    string label,
    Vector2 anchoredPos,
    UnityEngine.Events.UnityAction onClick)
  {
    GameUITheme.CreateButton(parent, name, label, anchoredPos, new Vector2(240, 46), GameUITheme.Accent, onClick);
  }
}

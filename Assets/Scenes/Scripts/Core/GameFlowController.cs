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
  [SerializeField] private RunMiniMapUI runMiniMapUI;
  [SerializeField] private BossInfoOverlay bossInfoOverlay;

  [Header("Flow")]
  [SerializeField] private bool requireOpeningAugment = true;

  [Header("Title (auto-created if empty)")]
  [SerializeField] private GameObject titlePanel;

  [Header("End screens (auto-created if empty)")]
  [SerializeField] private GameObject victoryPanel;
  [SerializeField] private GameObject gameOverPanel;
  [SerializeField] private GameObject stageClearPanel;
  [SerializeField] private GameObject floorRewardPanel;

  private TMP_Text _stageClearTitleText;
  private TMP_Text _stageClearBodyText;
  private TMP_Text _rewardTitleText;
  private TMP_Text _rewardBodyText;
  private Image _rewardImage;
  private bool _floorRewardShownThisRun;

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
    EnsureBossInfoOverlay();
    EnsureEndPanels();
    EnsureFloorRewardPanel();
    EnsureRunMiniMap();
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

  public void OnTitleBossInfoClicked()
  {
    EnsureBossInfoOverlay();
    bossInfoOverlay?.Show(diceBoss, cardBoss);
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

    if (TryShowFloorRewardAfterFloor1Boss())
      return;

    runManager.SetPhase(RunPhase.BossPreview);
    ShowBossPreview(runManager.State, false);
  }

  private void ShowStageClearScreen(int hpBefore, int hpAfter, int healed, int maxHp)
  {
    if (stageClearPanel == null)
      EnsureEndPanels();

    ApplyDefaultStageClearPresentation();
    HideTitleAndPreview();
    bossPreviewUI?.HidePreview();
    augmentPickUI?.Hide();
    HideCombatHud();
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
    HideCombatHud();
    if (victoryPanel != null)
      victoryPanel.SetActive(false);
    if (gameOverPanel != null)
      gameOverPanel.SetActive(false);

    int clearedFloor = runManager.State.currentFloor - 1;
    var nextBoss = runManager.State.assignedBoss;
    ApplyFloorClearPresentation(clearedFloor);

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

  private void ApplyDefaultStageClearPresentation()
  {
    ApplyStageClearBackground(null);
    SetStageClearTextBox(false, Vector2.zero, Vector2.zero);
    SetStageClearLayout(
      new Vector2(0, 130),
      new Vector2(560, 56),
      new Vector2(0, 0),
      new Vector2(560, 180),
      new Vector2(0, -140),
      TextAlignmentOptions.Center);
  }

  private void ApplyFloorClearPresentation(int clearedFloor)
  {
    if (clearedFloor != 1)
    {
      ApplyDefaultStageClearPresentation();
      return;
    }

    ApplyStageClearBackground(LoadSprite("UI/floor1_clear_bg"));
    SetStageClearTextBox(true, new Vector2(-390, 20), new Vector2(500, 360));
    SetStageClearLayout(
      new Vector2(-390, 145),
      new Vector2(420, 60),
      new Vector2(-390, 15),
      new Vector2(430, 220),
      new Vector2(-390, -165),
      TextAlignmentOptions.Left);
  }

  private void ApplyStageClearBackground(Sprite sprite)
  {
    if (stageClearPanel == null)
      return;

    var image = stageClearPanel.GetComponent<Image>();
    if (image == null)
      return;

    if (sprite != null)
    {
      image.sprite = sprite;
      image.type = Image.Type.Simple;
      image.preserveAspect = false;
      image.color = Color.white;

      var outline = image.GetComponent<Outline>();
      if (outline != null)
        outline.enabled = false;
      return;
    }

    image.sprite = null;
    GameUITheme.StyleOverlayPanel(image);
    var restoredOutline = image.GetComponent<Outline>();
    if (restoredOutline != null)
      restoredOutline.enabled = true;
  }

  private void SetStageClearTextBox(bool visible, Vector2 anchoredPos, Vector2 size)
  {
    if (stageClearPanel == null)
      return;

    var box = stageClearPanel.transform.Find("StageClearTextBox") as RectTransform;
    if (box == null)
    {
      var boxGo = new GameObject("StageClearTextBox", typeof(RectTransform), typeof(Image));
      boxGo.transform.SetParent(stageClearPanel.transform, false);
      box = boxGo.GetComponent<RectTransform>();

      var image = boxGo.GetComponent<Image>();
      image.color = new Color(0f, 0f, 0f, 0.48f);

      var outline = boxGo.AddComponent<Outline>();
      outline.effectColor = new Color(1f, 1f, 1f, 0.16f);
      outline.effectDistance = new Vector2(1.5f, -1.5f);
    }

    box.anchorMin = box.anchorMax = new Vector2(0.5f, 0.5f);
    box.pivot = new Vector2(0.5f, 0.5f);
    box.anchoredPosition = anchoredPos;
    box.sizeDelta = size;
    box.gameObject.SetActive(visible);

    if (visible)
    {
      box.SetAsFirstSibling();
      if (_stageClearTitleText != null)
        _stageClearTitleText.transform.SetAsLastSibling();
      if (_stageClearBodyText != null)
        _stageClearBodyText.transform.SetAsLastSibling();
      var button = stageClearPanel.transform.Find("StageClearContinueButton");
      if (button != null)
        button.SetAsLastSibling();
    }
  }

  private void SetStageClearLayout(
    Vector2 titlePos,
    Vector2 titleSize,
    Vector2 bodyPos,
    Vector2 bodySize,
    Vector2 buttonPos,
    TextAlignmentOptions bodyAlignment)
  {
    if (_stageClearTitleText != null)
    {
      var titleRt = _stageClearTitleText.GetComponent<RectTransform>();
      titleRt.anchoredPosition = titlePos;
      titleRt.sizeDelta = titleSize;
      _stageClearTitleText.alignment = TextAlignmentOptions.Center;
    }

    if (_stageClearBodyText != null)
    {
      var bodyRt = _stageClearBodyText.GetComponent<RectTransform>();
      bodyRt.anchoredPosition = bodyPos;
      bodyRt.sizeDelta = bodySize;
      _stageClearBodyText.alignment = bodyAlignment;
    }

    var buttonRt = stageClearPanel != null
      ? stageClearPanel.transform.Find("StageClearContinueButton")?.GetComponent<RectTransform>()
      : null;
    if (buttonRt != null)
      buttonRt.anchoredPosition = buttonPos;
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

  private void EnsureRunMiniMap()
  {
    if (runMiniMapUI == null)
      runMiniMapUI = FindFirstObjectByType<RunMiniMapUI>();

    if (runMiniMapUI != null)
      return;

    var canvas = FindFirstObjectByType<Canvas>();
    if (canvas != null)
      runMiniMapUI = canvas.gameObject.AddComponent<RunMiniMapUI>();
  }

  private void EnsureBossInfoOverlay()
  {
    if (bossInfoOverlay == null)
      bossInfoOverlay = FindFirstObjectByType<BossInfoOverlay>();

    if (bossInfoOverlay != null)
      return;

    var canvas = FindFirstObjectByType<Canvas>();
    if (canvas != null)
      bossInfoOverlay = canvas.gameObject.AddComponent<BossInfoOverlay>();
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
    if (floorRewardPanel != null)
      floorRewardPanel.SetActive(false);
    _floorRewardShownThisRun = false;
    PlayerAugmentState.Instance?.ResetRunCounters();
    PlayerRelicState.Instance?.ClearFloorReward();
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
      HideCombatHud();
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
    HideCombatHud();

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
    HideCombatHud();
    if (gameOverPanel != null)
      gameOverPanel.SetActive(false);
    if (stageClearPanel != null)
      stageClearPanel.SetActive(false);

    var victoryTitleGo = victoryPanel != null ? victoryPanel.transform.Find("VictoryTitle")?.gameObject : null;
    if (victoryTitleGo != null)
      victoryTitleGo.SetActive(false);

    var victoryBodyGo = victoryPanel != null ? victoryPanel.transform.Find("VictoryBody")?.gameObject : null;
    if (victoryBodyGo != null)
    {
      EnsureVictorySpeechBubble(victoryBodyGo.transform);

      var bodyText = victoryBodyGo.GetComponent<TMP_Text>();
      if (bodyText != null)
      {
        bodyText.text = GameUIText.VictoryFullBody;
        bodyText.fontSize = 36;
        bodyText.fontStyle = FontStyles.Bold;
        bodyText.alignment = TextAlignmentOptions.Center;
        bodyText.color = Color.white;
      }

      var bodyRt = victoryBodyGo.GetComponent<RectTransform>();
      if (bodyRt != null)
      {
        bodyRt.anchoredPosition = new Vector2(345f, 205f);
        bodyRt.sizeDelta = new Vector2(420f, 120f);
      }
      victoryBodyGo.SetActive(true);
      victoryBodyGo.transform.SetAsLastSibling();
    }

    if (victoryPanel != null)
      victoryPanel.SetActive(true);
  }

  private void EnsureVictorySpeechBubble(Transform textTransform)
  {
    if (victoryPanel == null || textTransform == null)
      return;

    var bubble = victoryPanel.transform.Find("VictorySpeechBubble") as RectTransform;
    if (bubble == null)
    {
      var bubbleGo = new GameObject("VictorySpeechBubble", typeof(RectTransform), typeof(Image));
      bubbleGo.transform.SetParent(victoryPanel.transform, false);
      bubble = bubbleGo.GetComponent<RectTransform>();

      var image = bubbleGo.GetComponent<Image>();
      image.color = new Color(1f, 1f, 1f, 0.04f);

      var outline = bubbleGo.AddComponent<Outline>();
      outline.effectColor = new Color(1f, 1f, 1f, 0.95f);
      outline.effectDistance = new Vector2(4f, -4f);

      var tailGo = new GameObject("Tail", typeof(RectTransform), typeof(Image));
      tailGo.transform.SetParent(bubbleGo.transform, false);
      var tailImage = tailGo.GetComponent<Image>();
      tailImage.color = image.color;

      var tailOutline = tailGo.AddComponent<Outline>();
      tailOutline.effectColor = outline.effectColor;
      tailOutline.effectDistance = outline.effectDistance;
    }

    bubble.anchorMin = bubble.anchorMax = new Vector2(0.5f, 0.5f);
    bubble.pivot = new Vector2(0.5f, 0.5f);
    bubble.anchoredPosition = new Vector2(345f, 205f);
    bubble.sizeDelta = new Vector2(480f, 145f);
    bubble.gameObject.SetActive(true);

    var bubbleImage = bubble.GetComponent<Image>();
    if (bubbleImage != null)
      bubbleImage.color = new Color(1f, 1f, 1f, 0.04f);

    var bubbleOutline = bubble.GetComponent<Outline>();
    if (bubbleOutline != null)
    {
      bubbleOutline.effectColor = new Color(1f, 1f, 1f, 0.95f);
      bubbleOutline.effectDistance = new Vector2(4f, -4f);
    }

    var tail = bubble.Find("Tail") as RectTransform;
    if (tail != null)
    {
      tail.gameObject.SetActive(false);
    }
    bubble.SetSiblingIndex(textTransform.GetSiblingIndex());
  }

  private void ShowGameOverScreen()
  {
    HideTitleAndPreview();
    bossPreviewUI?.HidePreview();
    augmentPickUI?.Hide();
    HideCombatHud();
    if (victoryPanel != null)
      victoryPanel.SetActive(false);
    if (stageClearPanel != null)
      stageClearPanel.SetActive(false);
    if (gameOverPanel != null)
      gameOverPanel.SetActive(true);

    var title = gameOverPanel != null ? gameOverPanel.transform.Find("GameOverTitle")?.GetComponent<TMP_Text>() : null;
    if (title != null)
      title.text = "당신은 ai한테 패배했습니다";
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

  /// <summary>Hide the in-combat HUD boxes (HP panels/bars, status, log) on non-combat screens.</summary>
  private void HideCombatHud()
  {
    if (combatUI == null)
      combatUI = FindFirstObjectByType<CombatUI>();
    combatUI?.SetHudPanelsVisible(false);
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
      HideCombatHud();
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

    // Let the blurred title background show through with a light readability scrim.
    var titleScrim = titlePanel.GetComponent<Image>();
    if (titleScrim != null)
      titleScrim.color = new Color(GameUITheme.BgDeep.r, GameUITheme.BgDeep.g, GameUITheme.BgDeep.b, 0.16f);

    CreateTitleLogoFrame(titlePanel.transform, new Vector2(0, 170), new Vector2(660, 168));

    var titleTmp = CreateCenteredText(titlePanel.transform, "TitleText", "GambleRogue", 58, new Vector2(0, 170), new Vector2(760, 94),
      FontStyles.Bold);
    StyleTitleLogo(titleTmp);

    var subtitleTmp = CreateCenteredText(titlePanel.transform, "TitleSubtitle", GameUIText.TitleSubtitle, 19, new Vector2(0, 112),
      new Vector2(640, 40), FontStyles.Bold);
    StyleTitleSubtitle(subtitleTmp);
    CreateCenteredText(
      titlePanel.transform,
      "TitleGuide",
      GameUIText.TitleGuide,
      14,
      new Vector2(0, 50),
      new Vector2(720, 80));
    CreatePanelButton(titlePanel.transform, "StartRunButton", GameUIText.StartRun, new Vector2(0, -40), OnStartRunClicked);
    CreatePanelButton(titlePanel.transform, "TitleBossInfoButton", GameUIText.BossInfoShow, new Vector2(0, -98), OnTitleBossInfoClicked);
  }

  private static void StyleTitleLogo(TextMeshProUGUI titleTmp)
  {
    if (titleTmp == null)
      return;

    titleTmp.color = GameUITheme.Hex("#EAF2FF");
    titleTmp.characterSpacing = 5f;
    titleTmp.fontStyle = FontStyles.Bold;
    titleTmp.outlineColor = GameUITheme.Hex("#10192E");
    titleTmp.outlineWidth = 0.32f;
    titleTmp.enableVertexGradient = true;
    titleTmp.colorGradient = new VertexGradient(
      GameUITheme.Hex("#FFFFFF"),
      GameUITheme.Hex("#D7E7FF"),
      GameUITheme.Hex("#9CBFFF"),
      GameUITheme.Hex("#6FA3FF"));

    var outline = titleTmp.GetComponent<Outline>() ?? titleTmp.gameObject.AddComponent<Outline>();
    outline.effectColor = new Color(0.02f, 0.04f, 0.1f, 0.92f);
    outline.effectDistance = new Vector2(4f, -4f);

    var shadow = titleTmp.GetComponent<Shadow>() ?? titleTmp.gameObject.AddComponent<Shadow>();
    shadow.effectColor = new Color(0f, 0f, 0f, 0.75f);
    shadow.effectDistance = new Vector2(7f, -7f);
  }

  private static void StyleTitleSubtitle(TMP_Text subtitleTmp)
  {
    if (subtitleTmp == null)
      return;

    subtitleTmp.characterSpacing = 8f;
    subtitleTmp.fontStyle = FontStyles.Bold;
    subtitleTmp.color = GameUITheme.Hex("#F6E0A4");
    subtitleTmp.enableVertexGradient = true;
    subtitleTmp.colorGradient = new VertexGradient(
      GameUITheme.Hex("#FFF4CF"),
      GameUITheme.Hex("#FFF4CF"),
      GameUITheme.Hex("#E9B85A"),
      GameUITheme.Hex("#E9B85A"));

    var outline = subtitleTmp.GetComponent<Outline>() ?? subtitleTmp.gameObject.AddComponent<Outline>();
    outline.effectColor = new Color(0f, 0f, 0f, 0.85f);
    outline.effectDistance = new Vector2(2f, -2f);

    var shadow = subtitleTmp.GetComponent<Shadow>() ?? subtitleTmp.gameObject.AddComponent<Shadow>();
    shadow.effectColor = new Color(0f, 0f, 0f, 0.55f);
    shadow.effectDistance = new Vector2(3f, -3f);
  }

  private static void CreateTitleLogoFrame(Transform parent, Vector2 anchoredPos, Vector2 size)
  {
    // Inner semi-transparent rounded fill (the body of the "black hole" plate).
    var fill = CreateFrameImage(parent, "TitleLogoFill", anchoredPos, size, new Color(0f, 0f, 0f, 0.62f));
    fill.sprite = GetTitleRoundedFillSprite();
    fill.type = Image.Type.Sliced;

    // Radial void overlay: darkest at the center, fading toward the edges.
    var voidGlow = CreateFrameImage(parent, "TitleLogoVoid", anchoredPos, new Vector2(size.x - 26f, size.y - 26f), new Color(0f, 0f, 0f, 0.9f));
    voidGlow.sprite = GetTitleVoidSprite();
    voidGlow.type = Image.Type.Simple;

    // Gold rounded border on top.
    var border = CreateFrameImage(parent, "TitleLogoBorder", anchoredPos, size, GameUITheme.Hex("#F4C95D"));
    border.sprite = GetTitleGoldRingSprite();
    border.type = Image.Type.Sliced;
  }

  private static Image CreateFrameImage(Transform parent, string name, Vector2 anchoredPos, Vector2 size, Color color)
  {
    var go = new GameObject(name, typeof(RectTransform), typeof(Image));
    go.transform.SetParent(parent, false);
    var rt = go.GetComponent<RectTransform>();
    rt.anchorMin = new Vector2(0.5f, 0.5f);
    rt.anchorMax = new Vector2(0.5f, 0.5f);
    rt.pivot = new Vector2(0.5f, 0.5f);
    rt.anchoredPosition = anchoredPos;
    rt.sizeDelta = size;
    var img = go.GetComponent<Image>();
    img.color = color;
    img.raycastTarget = false;
    return img;
  }

  private static Sprite _titleRoundedFillSprite;
  private static Sprite _titleGoldRingSprite;
  private static Sprite _titleVoidSprite;

  private static Sprite GetTitleRoundedFillSprite() =>
    _titleRoundedFillSprite ??= BuildRoundedSprite(128, 34f, -1f, Color.white);

  private static Sprite GetTitleGoldRingSprite() =>
    _titleGoldRingSprite ??= BuildRoundedSprite(128, 34f, 9f, Color.white);

  private static Sprite GetTitleVoidSprite()
  {
    if (_titleVoidSprite != null)
      return _titleVoidSprite;

    const int size = 128;
    var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };
    var px = new Color[size * size];
    float cx = (size - 1) * 0.5f;
    float cy = (size - 1) * 0.5f;
    float maxR = size * 0.5f;
    for (int y = 0; y < size; y++)
    {
      for (int x = 0; x < size; x++)
      {
        float d = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy)) / maxR;
        float a = Mathf.Clamp01(1f - d);
        a = a * a;
        px[y * size + x] = new Color(0f, 0f, 0f, a);
      }
    }
    tex.SetPixels(px);
    tex.Apply();
    _titleVoidSprite = Sprite.Create(tex, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 100f);
    return _titleVoidSprite;
  }

  // Builds a rounded rectangle. borderThickness < 0 => filled interior; otherwise only a ring of that thickness.
  private static Sprite BuildRoundedSprite(int size, float radius, float borderThickness, Color color)
  {
    var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };
    var px = new Color[size * size];
    float half = size * 0.5f;
    float inner = half - radius;
    for (int y = 0; y < size; y++)
    {
      for (int x = 0; x < size; x++)
      {
        float qx = Mathf.Abs(x + 0.5f - half) - inner;
        float qy = Mathf.Abs(y + 0.5f - half) - inner;
        float ax = Mathf.Max(qx, 0f);
        float ay = Mathf.Max(qy, 0f);
        float dist = Mathf.Sqrt(ax * ax + ay * ay) + Mathf.Min(Mathf.Max(qx, qy), 0f) - radius;

        float a;
        if (borderThickness < 0f)
          a = Mathf.Clamp01(0.5f - dist);                              // antialiased filled interior
        else
          a = Mathf.Clamp01(0.5f - Mathf.Abs(dist + borderThickness * 0.5f) + borderThickness * 0.5f) // ring band
              * Mathf.Clamp01(0.5f - dist);
        px[y * size + x] = new Color(color.r, color.g, color.b, color.a * a);
      }
    }
    tex.SetPixels(px);
    tex.Apply();
    int b = Mathf.CeilToInt(radius) + 2;
    return Sprite.Create(tex, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(b, b, b, b));
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

      var victoryBg = victoryPanel.GetComponent<Image>();
      var tex = Resources.Load<Texture2D>("UI/victory_final");
      if (victoryBg != null && tex != null)
      {
        victoryBg.sprite = Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
        victoryBg.type = Image.Type.Simple;
        victoryBg.color = Color.white;
      }
      victoryPanel.SetActive(false);
    }

    if (gameOverPanel == null)
    {
      gameOverPanel = CreateOverlayPanel(canvas.transform, "GameOverPanel");
      var gameOverTitle = CreateCenteredText(gameOverPanel.transform, "GameOverTitle", GameUIText.GameOver, 32, new Vector2(0, 100),
        new Vector2(500, 60), FontStyles.Bold);
      gameOverTitle.color = GameUITheme.HpLow;
      CreateCenteredText(gameOverPanel.transform, "GameOverBody", "", 18, new Vector2(0, 30),
        new Vector2(600, 20));
      CreatePanelButton(gameOverPanel.transform, "GameOverTitleButton", "초기화면", new Vector2(0, -20),
        ReturnToTitle);
      CreatePanelButton(gameOverPanel.transform, "GameOverNewRunButton", "다시시작하기", new Vector2(0, -82),
        RestartFromCombat);

      var bg = gameOverPanel.GetComponent<Image>();
      var tex = Resources.Load<Texture2D>("UI/dice_boss_preview");
      if (bg != null && tex != null)
      {
        bg.sprite = Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
        bg.color = new Color(0.45f, 0.1f, 0.1f, 0.9f);
      }
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

  private bool TryShowFloorRewardAfterFloor1Boss()
  {
    if (_floorRewardShownThisRun || runManager == null || runManager.State.currentFloor != 2 || PlayerRelicState.Instance == null)
      return false;

    EnsureFloorRewardPanel();
    if (floorRewardPanel == null || _rewardTitleText == null || _rewardBodyText == null || _rewardImage == null)
      return false;

    var reward = PlayerRelicState.Instance.GrantRandomFloorReward();
    _floorRewardShownThisRun = true;

    _rewardTitleText.text = $"획득 아이템: {PlayerRelicState.GetFloorRewardName(reward)}";
    _rewardBodyText.text = PlayerRelicState.GetFloorRewardDescription(reward);
    _rewardImage.sprite = LoadSprite(PlayerRelicState.GetFloorRewardSpritePath(reward));
    _rewardImage.color = _rewardImage.sprite != null ? Color.white : new Color(1f, 1f, 1f, 0f);

    floorRewardPanel.transform.SetAsLastSibling();
    floorRewardPanel.SetActive(true);
    return true;
  }

  private void OnConfirmFloorReward()
  {
    if (floorRewardPanel != null)
      floorRewardPanel.SetActive(false);

    runManager.SetPhase(RunPhase.BossPreview);
    ShowBossPreview(runManager.State, false);
  }

  private void EnsureFloorRewardPanel()
  {
    if (floorRewardPanel != null)
      return;

    var canvas = FindFirstObjectByType<Canvas>();
    if (canvas == null)
      return;

    floorRewardPanel = CreateOverlayPanel(canvas.transform, "FloorRewardPanel");
    var panelImage = floorRewardPanel.GetComponent<Image>();
    if (panelImage != null)
      panelImage.color = new Color(GameUITheme.BgPanel.r, GameUITheme.BgPanel.g, GameUITheme.BgPanel.b, 0.93f);

    _rewardTitleText = CreateCenteredText(
      floorRewardPanel.transform, "RewardTitle", "획득 아이템", 30, new Vector2(0, 180), new Vector2(760, 56), FontStyles.Bold);
    _rewardTitleText.color = GameUITheme.Accent;

    var imageGo = new GameObject("RewardImage", typeof(RectTransform), typeof(Image));
    imageGo.transform.SetParent(floorRewardPanel.transform, false);
    var imgRt = imageGo.GetComponent<RectTransform>();
    imgRt.anchorMin = imgRt.anchorMax = new Vector2(0.5f, 0.5f);
    imgRt.anchoredPosition = new Vector2(0, 30);
    imgRt.sizeDelta = new Vector2(320, 200);
    _rewardImage = imageGo.GetComponent<Image>();
    _rewardImage.preserveAspect = true;

    _rewardBodyText = CreateCenteredText(
      floorRewardPanel.transform, "RewardBody", "", 18, new Vector2(0, -120), new Vector2(860, 120));
    _rewardBodyText.richText = true;

    CreatePanelButton(floorRewardPanel.transform, "RewardConfirmButton", "확인", new Vector2(0, -230), OnConfirmFloorReward);
    floorRewardPanel.SetActive(false);
  }

  private static Sprite LoadSprite(string resourcePath)
  {
    if (string.IsNullOrEmpty(resourcePath))
      return null;

    var tex = Resources.Load<Texture2D>(resourcePath);
    if (tex == null)
      return null;

    return Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
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

    GameUITheme.StyleOverlayPanel(panel.GetComponent<Image>());
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

using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CombatUI : MonoBehaviour
{
  [Header("Combat")]
  [SerializeField] private BettingCombatSystem combat;

  [Header("Boss (test until RunPlanner from C)")]
  [SerializeField] private BossDefinition testBoss;
  [SerializeField] private TMP_Text bossRuleText;

  [Header("HUD")]
  [SerializeField] private TMP_Text playerHpText;
  [SerializeField] private TMP_Text enemyHpText;
  [SerializeField] private TMP_Text combatLogText;
  [SerializeField] private TMP_Text statusText;

  [Header("Bet options (shown on buttons)")]
  [SerializeField] private CombatBetOption safeOption = CombatBalance.Safe;
  [SerializeField] private CombatBetOption riskOption = CombatBalance.Risk;
  [SerializeField] private CombatBetOption allInOption = CombatBalance.AllIn;
  [SerializeField] private CombatBetOption hiddenOption = CombatBalance.Hidden;

  [Header("Normal fight")]
  [SerializeField] private int normalEnemyHp = CombatBalance.DefaultEnemyHp;

  [Header("Buttons (optional — auto-find by name if empty)")]
  [SerializeField] private Button betSafeButton;
  [SerializeField] private Button betRiskButton;
  [SerializeField] private Button betAllInButton;
  [SerializeField] private Button restartButton;
  [SerializeField] private Button betHiddenButton;
  [SerializeField] private Button bossRuleToggleButton;

  private bool _isBossFight;
  private bool _bossRuleExpanded;
  private string _bossRuleFullText = "";
  private bool _combatEndReported;
  private bool _turnPresentationRunning;
  private CombatUIStyler _styler;
  private CombatPresentationUI _presentation;

  private void Awake()
  {
    ResolveReferences();
    RemoveBossRuleUi();
    EnsureUIStyler();
    EnsurePresentation();
    EnsureHiddenBetButton();
    EnsureBossRuleToggle();
    ApplyButtonLabels();
    WireRestartButton();

    combat.OnCombatLog += OnLog;
    combat.OnCombatStateChanged += OnCombatStateChanged;

    if (FindFirstObjectByType<BossPreviewUI>() == null)
      StartNewCombat();
    else
      SetHudPanelsVisible(false);
  }

  /// <summary>Starts combat from run flow. Pass null boss for normal fights.</summary>
  public void BeginCombat(BossDefinition boss, bool isBossFight = true)
  {
    _isBossFight = isBossFight && boss != null;
    testBoss = _isBossFight ? boss : null;
    _combatEndReported = false;
    SetBossRuleExpanded(false);

    SetCombatHudVisible(true);
    StartNewCombat();
    PlayerAugmentState.Instance?.ApplyPreCombat(combat);
    SyncRunHpToRun();
    RefreshAll();
    LogAugmentSummary();
  }

  /// <summary>Legacy entry for direct boss combat.</summary>
  public void BeginCombat(BossDefinition boss) => BeginCombat(boss, isBossFight: boss != null);

  private void SyncRunHpToRun()
  {
    if (combat == null || RunManager.Instance == null || !RunManager.Instance.State.isActive)
      return;

    RunManager.Instance.SyncHpFromCombat(combat.State.playerHp, combat.State.playerMaxHp);
  }

  private void LogAugmentSummary()
  {
    if (PlayerAugmentState.Instance == null)
      return;

    string summary = PlayerAugmentState.Instance.GetSummaryLine();
    if (!string.IsNullOrEmpty(summary))
      OnLog(summary);

    if (PlayerRelicState.Instance != null)
    {
      string relicSummary = PlayerRelicState.Instance.GetSummaryLine();
      if (!string.IsNullOrEmpty(relicSummary))
        OnLog(relicSummary);
    }
  }

  private void EnsureUIStyler()
  {
    var canvas = GetComponent<Canvas>() ?? FindFirstObjectByType<Canvas>();
    if (canvas == null)
      return;

    _styler = canvas.GetComponent<CombatUIStyler>();
    if (_styler == null)
      _styler = canvas.gameObject.AddComponent<CombatUIStyler>();
  }

  private void EnsurePresentation()
  {
    var canvas = GetComponent<Canvas>() ?? FindFirstObjectByType<Canvas>();
    if (canvas == null)
      return;

    _presentation = CombatPresentationUI.Ensure(canvas);
  }

  private void ResolveReferences()
  {
    if (combat == null)
      combat = FindFirstObjectByType<BettingCombatSystem>();

    if (playerHpText == null)
      playerHpText = FindTmp("PlayerHpText");
    if (enemyHpText == null)
      enemyHpText = FindTmp("EnemyHpText");
    if (combatLogText == null)
      combatLogText = FindTmp("CombatLogText");
    if (statusText == null)
      statusText = FindTmp("StatusText");
    if (bossRuleText == null)
      bossRuleText = FindTmp("BossRuleText");

    ResolveButtons();

    if (testBoss == null)
      Debug.LogWarning("CombatUI: Test Boss is empty. Drag Boss_Dice or Boss_Card into Combat UI → Test Boss, then save the scene (Ctrl+S).");
  }

  private TMP_Text FindTmp(string objectName)
  {
    var t = FindChildRecursive(transform, objectName);
    return t != null ? t.GetComponent<TMP_Text>() : null;
  }

  private void ResolveButtons()
  {
    if (betSafeButton == null)
      betSafeButton = FindButton("BetSmall");
    if (betRiskButton == null)
      betRiskButton = FindButton("BetMedium");
    if (betAllInButton == null)
      betAllInButton = FindButton("BetLarge");
    if (restartButton == null)
      restartButton = FindButton("RestartButton");
  }

  private Button FindButton(string objectName)
  {
    var t = FindChildRecursive(transform, objectName);
    return t != null ? t.GetComponent<Button>() : null;
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

  private void OnDestroy()
  {
    if (combat == null)
      return;

    combat.OnCombatLog -= OnLog;
    combat.OnCombatStateChanged -= OnCombatStateChanged;
  }

  public void StartNewCombat()
  {
    if (combat == null)
    {
      Debug.LogError("CombatUI: BettingCombatSystem not found on GameRoot.");
      return;
    }

    if (combatLogText != null)
      combatLogText.text = string.Empty;

    int enemyHp = _isBossFight && testBoss != null ? testBoss.maxHp : normalEnemyHp;
    combat.ResetCombat(testBoss, enemyHp);
    RefreshAll();
  }

  public void SetTestBoss(BossDefinition boss)
  {
    testBoss = boss;
    StartNewCombat();
  }

  public void OnBetSmall() => DoBet(safeOption);
  public void OnBetMedium() => DoBet(riskOption);
  public void OnBetLarge() => DoBet(allInOption);
  public void OnBetHidden() => DoBet(hiddenOption);

  public void OnToggleBossRule() => SetBossRuleExpanded(!_bossRuleExpanded);

  public void OnRestartCombat()
  {
    var flow = FindFirstObjectByType<GameFlowController>();
    if (flow != null)
    {
      flow.RestartFromCombat();
      return;
    }

    StartNewCombat();
  }

  private void DoBet(CombatBetOption option)
  {
    if (!combat.IsPlayerTurn || _turnPresentationRunning)
      return;

    StartCoroutine(ResolveDuelRound(option));
  }

  private IEnumerator ResolveDuelRound(CombatBetOption playerOption)
  {
    _turnPresentationRunning = true;
    RefreshButtons();
    RefreshStatus();

    var enemyOption = EnemyCombatAI.PickBet(
      combat.State.enemyHp, testBoss, combat.PatternController);
    var theme = CombatPresentationThemeResolver.Resolve(testBoss);

    if (!combat.TryComputeDuel(playerOption, enemyOption, theme, out CombatTurnResult result))
    {
      _turnPresentationRunning = false;
      RefreshButtons();
      yield break;
    }

    string enemyName = GetEnemyDisplayName();
    if (_presentation != null)
      yield return _presentation.PlayTurn(theme, result, enemyName);

    combat.ApplyDuelResult(result);

    var postFx = combat.ConsumePostApplyFx();
    if (_presentation != null && postFx.Count > 0)
      yield return _presentation.PlayPostApplyFx(postFx);

    RefreshAll();

    _turnPresentationRunning = false;
    RefreshButtons();
    RefreshStatus();
  }

  private void OnLog(string msg)
  {
    if (combatLogText != null)
    {
      string prev = combatLogText.text;
      combatLogText.text = string.IsNullOrEmpty(prev) ? msg : msg + "\n" + prev;
    }
    else
    {
      Debug.LogWarning($"CombatUI: CombatLogText missing. Log: {msg}");
    }

    RefreshHud();
  }

  private void OnCombatStateChanged()
  {
    RefreshAll();

    if (combat == null || combat.IsCombatActive || _combatEndReported)
      return;

    _combatEndReported = true;
    SyncRunHpToRun();

    var flow = FindFirstObjectByType<GameFlowController>();
    if (flow == null)
      return;

    if (combat.State.IsPlayerDead)
      flow.HandleCombatDefeat();
    else if (combat.State.IsEnemyDead)
      flow.HandleCombatVictory();
  }

  private void RefreshAll()
  {
    RefreshHud();
    RefreshStatus();
    RefreshButtons();
    ApplyButtonLabels();
  }

  private void RefreshHud()
  {
    if (combat == null)
      return;

    var s = combat.State;
    if (playerHpText != null)
    {
      playerHpText.richText = true;
      playerHpText.text = GameUITheme.FormatHpRich(s.playerHp, s.playerMaxHp, "자신");
    }

    if (enemyHpText != null)
    {
      string enemyName = GetEnemyDisplayName();
      enemyHpText.richText = true;
      enemyHpText.text = GameUITheme.FormatHpRich(s.enemyHp, s.enemyMaxHp, enemyName.ToUpper());
    }

    _styler?.RefreshHpBars(s.playerHp, s.playerMaxHp, s.enemyHp, s.enemyMaxHp);

    RefreshBossRuleDisplay();
  }

  private string GetEnemyDisplayName()
  {
    if (_isBossFight)
    {
      if (RunManager.Instance != null && RunManager.Instance.State.isActive)
        return RunManager.Instance.State.currentFloor <= 1 ? "주사위 보스" : "카드보스";

      if (testBoss != null)
        return testBoss.displayName;
    }

    if (RunManager.Instance != null && RunManager.Instance.State.isActive)
      return RunManager.Instance.State.currentFloor <= 1 ? "주사위 쫄병" : "카드 쫄병";

    return GameUIText.Enemy;
  }

  private void RefreshBossRuleDisplay()
  {
    // User request: remove the translucent navy boss-info UI entirely.
    RemoveBossRuleUi();
  }

  private void SetBossRuleExpanded(bool expanded)
  {
    _bossRuleExpanded = expanded;
    RefreshBossRuleDisplay();
  }

  private void RefreshStatus()
  {
    if (statusText == null)
      return;

    if (!combat.IsCombatActive)
    {
      if (combat.State.IsPlayerDead)
        statusText.text = GameUIText.GameOver;
      else if (combat.State.IsEnemyDead)
        statusText.text = _isBossFight ? GameUIText.BossDefeated : GameUIText.Victory;
      else
        statusText.text = GameUIText.FightEnded;
    }
    else if (_turnPresentationRunning)
    {
      statusText.text = GameUIText.DuelInProgress;
    }
    else if (_isBossFight && testBoss != null)
    {
      string floor = BuildFloorProgressLine();
      statusText.text = $"{GameUIText.ChooseBet} — 적도 배팅 후 주사위/카드로 승부\n{floor}\n{GameUIText.ChooseBetBossHint}";
    }
    else if (RunManager.Instance != null && RunManager.Instance.State.isActive)
    {
      string duelHint = RunManager.Instance.State.currentFloor == 1
        ? GameUIText.DiceDuelHint
        : GameUIText.CardDuelHint;
      statusText.text = $"{GameUIText.ChooseBet}\n{BuildFloorProgressLine()}\n{duelHint}";
    }
    else
    {
      statusText.text = $"{GameUIText.ChooseBet}\n{GameUIText.HigherRollWins}";
    }

    if (statusText != null)
      statusText.richText = true;
  }

  private static string BuildFloorProgressLine()
  {
    if (RunManager.Instance == null || !RunManager.Instance.State.isActive)
      return "";

    var s = RunManager.Instance.State;
    string phase = s.phase == RunPhase.BossCombat
      ? GameUIText.BossFightPhase
      : GameUIText.FightPhase(s.normalFightsCompleted, s.normalFightsBeforeBoss);
    return GameUIText.FloorProgress(s.currentFloor, RunState.TotalFloors, phase);
  }

  private void SetCombatHudVisible(bool visible)
  {
    var preview = FindFirstObjectByType<BossPreviewUI>();
    if (preview != null)
    {
      if (visible)
        preview.ShowCombatHud();
      else
        preview.HidePreview();
    }

    SetHudPanelsVisible(visible);
  }

  /// <summary>
  /// Show/hide the in-combat HUD boxes (player/enemy HP panels + green bars, center status box,
  /// bottom-left combat log box). Hidden on the title screen and other non-combat phases.
  /// Does not touch the boss preview so it can be shown independently.
  /// </summary>
  public void SetHudPanelsVisible(bool visible)
  {
    SetHudElementActive("PlayerHpPanel", visible);
    SetHudElementActive("EnemyHpPanel", visible);
    SetHudElementActive("StatusPanel", visible);
    SetHudElementActive("CombatLogPanel", visible);
    // Restart is moved to ESC pause overlay only.
    SetHudElementActive("RestartButton", false);
  }

  private void SetHudElementActive(string objectName, bool active)
  {
    var t = FindChildRecursive(transform, objectName);
    if (t != null)
      t.gameObject.SetActive(active);
  }

  private void RefreshButtons()
  {
    bool playerTurn = combat.IsPlayerTurn && !_turnPresentationRunning;
    int hp = combat.State.playerHp;
    SetBetButton(betSafeButton, playerTurn && combat.CanAffordBet(safeOption), "Safe");
    SetBetButton(betRiskButton, playerTurn && combat.CanAffordBet(riskOption), "Risk");
    SetBetButton(betAllInButton, playerTurn && combat.CanAffordBet(allInOption), "All-in");
    SetButtonInteractable(restartButton, true);

    bool offerHidden = combat.ShouldOfferHiddenBet();
    if (betHiddenButton != null)
    {
      betHiddenButton.gameObject.SetActive(offerHidden);
      SetBetButton(betHiddenButton, playerTurn && offerHidden && combat.CanAffordBet(hiddenOption), "Hidden");
    }

    if (statusText != null && playerTurn && hp > 0)
    {
      if (offerHidden)
        statusText.text = GameUIText.HiddenBetStatus;
      else if (!combat.CanAffordBet(allInOption) && !combat.CanAffordBet(riskOption) && combat.CanAffordBet(safeOption))
        statusText.text = GameUIText.LowHpSafeOnly;
      else if (!combat.CanAffordBet(safeOption))
        statusText.text = GameUIText.HpInsufficient;
    }
  }

  private static void SetBetButton(Button button, bool interactable, string betLabel)
  {
    if (button == null)
      return;

    button.interactable = interactable;
    if (interactable)
      GameUITheme.StyleBetButton(button, betLabel);
    else
    {
      var img = button.GetComponent<Image>();
      if (img != null)
        img.color = new Color(0.35f, 0.35f, 0.38f, 0.65f);
    }
  }

  private static void SetButtonInteractable(Button button, bool interactable)
  {
    if (button != null)
      button.interactable = interactable;
  }

  private void ApplyButtonLabels()
  {
    if (combat == null)
      return;

    bool duel = CombatPresentationThemeResolver.UsesDuelRules(testBoss);
    int hp = combat.State.playerHp;
    SetButtonLabel(betSafeButton, FormatBetLabel(safeOption, duel, hp));
    SetButtonLabel(betRiskButton, FormatBetLabel(riskOption, duel, hp));
    SetButtonLabel(betAllInButton, FormatBetLabel(allInOption, duel, hp));
    if (betHiddenButton != null && betHiddenButton.gameObject.activeSelf)
      SetButtonLabel(betHiddenButton, FormatHiddenBetLabel(duel, hp));
  }

  private string FormatHiddenBetLabel(bool duel, int playerHp)
  {
    string baseLabel = duel
      ? hiddenOption.FormatDuelButtonLabel(testBoss, combat.PatternController)
      : GameUIText.HiddenButtonLabel(hiddenOption.damageOnSuccess);

    if (playerHp >= hiddenOption.betHp)
      return $"<color=#9BB0D0>{baseLabel}</color>";

    return $"{baseLabel}\n{GameUIText.HpInsufficientButton}";
  }

  private string FormatBetLabel(CombatBetOption option, bool duel, int playerHp)
  {
    string baseLabel = duel
      ? option.FormatDuelButtonLabel(testBoss, combat.PatternController)
      : option.FormatButtonLabel(testBoss, combat.PatternController);

    if (playerHp >= option.betHp)
      return baseLabel;

    return $"{baseLabel}\n{GameUIText.HpInsufficientButton}";
  }

  private static void SetButtonLabel(Button button, string label)
  {
    if (button == null)
      return;

    var tmp = button.GetComponentInChildren<TMP_Text>();
    if (tmp != null)
    {
      tmp.richText = true;
      tmp.text = label;
      tmp.alignment = TextAlignmentOptions.Center;
      // Auto-fit the multi-line bet text so it never overflows the button.
      tmp.textWrappingMode = TextWrappingModes.Normal;
      tmp.lineSpacing = -6f;
      tmp.enableAutoSizing = true;
      tmp.fontSizeMin = 9f;
      tmp.fontSizeMax = 14f;
    }
  }

  private void EnsureHiddenBetButton()
  {
    if (betHiddenButton != null)
      return;

    betHiddenButton = FindButton("BetHidden");
    if (betHiddenButton != null)
    {
      betHiddenButton.onClick.RemoveListener(OnBetHidden);
      betHiddenButton.onClick.AddListener(OnBetHidden);
      betHiddenButton.gameObject.SetActive(false);
      return;
    }

    var canvas = GetComponent<Canvas>() ?? FindFirstObjectByType<Canvas>();
    if (canvas == null)
      return;

    betHiddenButton = GameUITheme.CreateButton(
      canvas.transform,
      "BetHidden",
      GameUIText.HiddenButtonLabel(hiddenOption.damageOnSuccess),
      new Vector2(0f, 188f),
      new Vector2(200f, 72f),
      HexColor("#5B6B8A"),
      OnBetHidden);
    betHiddenButton.gameObject.SetActive(false);
    CombatUILayout.Apply(canvas.transform);
  }

  private void EnsureBossRuleToggle()
  {
    RemoveBossRuleUi();
  }

  private void RemoveBossRuleUi()
  {
    _bossRuleExpanded = false;
    _bossRuleFullText = string.Empty;

    var toggle = bossRuleToggleButton != null
      ? bossRuleToggleButton.transform
      : FindChildRecursive(transform, "BossRuleToggleButton");
    if (toggle != null)
      toggle.gameObject.SetActive(false);

    bossRuleToggleButton = null;

    if (bossRuleText != null)
      bossRuleText.gameObject.SetActive(false);

    var rulePanel = FindChildRecursive(transform, "BossRulePanel");
    if (rulePanel != null)
      rulePanel.gameObject.SetActive(false);
  }

  private static Color HexColor(string hex)
  {
    ColorUtility.TryParseHtmlString(hex, out Color c);
    return c;
  }

  private void WireRestartButton()
  {
    if (restartButton == null)
    {
      Debug.LogWarning(
        "CombatUI: RestartButton not found. Create UI Button named 'RestartButton' under Canvas, " +
        "or assign Restart Button in Inspector / wire On Click to CombatUI.OnRestartCombat.");
      return;
    }

    restartButton.onClick.RemoveListener(OnRestartCombat);
    restartButton.onClick.AddListener(OnRestartCombat);
    restartButton.interactable = true;
    restartButton.gameObject.SetActive(false);
  }
}

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

  [Header("Buttons (optional — auto-find by name if empty)")]
  [SerializeField] private Button betSafeButton;
  [SerializeField] private Button betRiskButton;
  [SerializeField] private Button betAllInButton;
  [SerializeField] private Button restartButton;

  private void Awake()
  {
    ResolveReferences();
    ApplyButtonLabels();
    WireRestartButton();

    combat.OnCombatLog += OnLog;
    combat.OnCombatStateChanged += OnCombatStateChanged;

    if (FindFirstObjectByType<BossPreviewUI>() == null)
      StartNewCombat();
  }

  /// <summary>Starts combat after boss preview (Role A step 4). Role C calls via BossPreviewUI.</summary>
  public void BeginCombat(BossDefinition boss)
  {
    testBoss = boss;
    StartNewCombat();
    PlayerAugmentState.Instance?.ApplyPreCombat(combat);
    SyncRunHpFromCombat();
    RefreshAll();
    LogAugmentSummary();
  }

  private void SyncRunHpFromCombat()
  {
    if (combat == null || RunManager.Instance == null || !RunManager.Instance.State.isActive)
      return;

    RunManager.Instance.State.maxHp = combat.State.playerMaxHp;
    RunManager.Instance.State.currentHp = combat.State.playerHp;
  }

  private void LogAugmentSummary()
  {
    if (PlayerAugmentState.Instance == null)
      return;

    string summary = PlayerAugmentState.Instance.GetSummaryLine();
    if (!string.IsNullOrEmpty(summary))
      OnLog(summary);
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

    // Clear BEFORE reset so "Combat started..." from ResetCombat stays visible
    if (combatLogText != null)
      combatLogText.text = string.Empty;

    combat.ResetCombat(testBoss);
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
    if (!combat.IsCombatActive)
      return;

    float chance = BossGambleResolvers.ModifySuccessChance(option.successChance, testBoss);
    if (combat.ResolveBetTurn(option.betHp, chance, option.damageOnSuccess, option.extraFailDamage))
      RefreshAll();
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

  private void OnCombatStateChanged() => RefreshAll();

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
      playerHpText.text = $"Player HP: {s.playerHp} / {s.playerMaxHp}";
    if (enemyHpText != null)
    {
      string enemyName = testBoss != null ? testBoss.displayName : "Enemy";
      enemyHpText.text = $"{enemyName} HP: {s.enemyHp}";
    }

    if (bossRuleText != null)
      bossRuleText.text = BossGambleResolvers.GetRuleLabel(testBoss);
  }

  private void RefreshStatus()
  {
    if (statusText == null)
      return;

    if (!combat.IsCombatActive)
    {
      if (combat.State.IsPlayerDead)
        statusText.text = "GAME OVER";
      else if (combat.State.IsEnemyDead)
        statusText.text = "VICTORY";
      else
        statusText.text = "FIGHT ENDED";
    }
    else if (testBoss != null)
    {
      statusText.text = $"CHOOSE YOUR BET\n{BossGambleResolvers.GetRuleLabel(testBoss)}";
    }
    else
    {
      statusText.text = "CHOOSE YOUR BET";
    }
  }

  private void RefreshButtons()
  {
    bool active = combat.IsCombatActive;
    SetButtonInteractable(betSafeButton, active);
    SetButtonInteractable(betRiskButton, active);
    SetButtonInteractable(betAllInButton, active);
    // Restart must stay clickable after victory / game over
    SetButtonInteractable(restartButton, true);
  }

  private static void SetButtonInteractable(Button button, bool interactable)
  {
    if (button != null)
      button.interactable = interactable;
  }

  private void ApplyButtonLabels()
  {
    SetButtonLabel(betSafeButton, safeOption.FormatButtonLabel(testBoss));
    SetButtonLabel(betRiskButton, riskOption.FormatButtonLabel(testBoss));
    SetButtonLabel(betAllInButton, allInOption.FormatButtonLabel(testBoss));
  }

  private static void SetButtonLabel(Button button, string label)
  {
    if (button == null)
      return;

    var tmp = button.GetComponentInChildren<TMP_Text>();
    if (tmp != null)
    {
      tmp.text = label;
      tmp.fontSize = 14;
      tmp.alignment = TextAlignmentOptions.Center;
    }
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
  }
}

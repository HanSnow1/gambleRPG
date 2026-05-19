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
  [SerializeField] private CombatBetOption safeOption = new()
  {
    label = "Safe",
    betHp = 10,
    successChance = 0.60f,
    damageOnSuccess = 20,
    extraFailDamage = 6
  };

  [SerializeField] private CombatBetOption riskOption = new()
  {
    label = "Risk",
    betHp = 18,
    successChance = 0.45f,
    damageOnSuccess = 46,
    extraFailDamage = 12
  };

  [SerializeField] private CombatBetOption allInOption = new()
  {
    label = "All-in",
    betHp = 25,
    successChance = 0.30f,
    damageOnSuccess = 72,
    extraFailDamage = 20
  };

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

    StartNewCombat();
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

  private static TMP_Text FindTmp(string objectName) =>
    GameObject.Find(objectName)?.GetComponent<TMP_Text>();

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

  public void OnRestartCombat() => StartNewCombat();

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

  private void ResolveButtons()
  {
    if (betSafeButton == null)
      betSafeButton = GameObject.Find("BetSmall")?.GetComponent<Button>();
    if (betRiskButton == null)
      betRiskButton = GameObject.Find("BetMedium")?.GetComponent<Button>();
    if (betAllInButton == null)
      betAllInButton = GameObject.Find("BetLarge")?.GetComponent<Button>();
    if (restartButton == null)
      restartButton = GameObject.Find("RestartButton")?.GetComponent<Button>();
  }

  private void ApplyButtonLabels()
  {
    SetButtonLabel(betSafeButton, safeOption.ButtonLabel);
    SetButtonLabel(betRiskButton, riskOption.ButtonLabel);
    SetButtonLabel(betAllInButton, allInOption.ButtonLabel);
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

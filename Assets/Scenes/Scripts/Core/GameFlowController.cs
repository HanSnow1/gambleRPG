using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Role C: Title -> Boss Preview (A's UI) -> Combat / Map.
/// </summary>
[DefaultExecutionOrder(-150)]
public class GameFlowController : MonoBehaviour
{
  [Header("References")]
  [SerializeField] private RunManager runManager;
  [SerializeField] private BossPreviewUI bossPreviewUI;

  [Header("Title (auto-created if empty)")]
  [SerializeField] private GameObject titlePanel;

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

    if (bossPreviewUI != null)
    {
      if (diceBoss == null)
        diceBoss = bossPreviewUI.DiceBoss;
      if (cardBoss == null)
        cardBoss = bossPreviewUI.CardBoss;
    }

    if (diceBoss != null && cardBoss != null)
      runManager.SetBossPool(diceBoss, cardBoss);

    EnsureTitlePanel();
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
    runManager.StartNewRun();
  }

  private void HandleRunStarted(RunState state) => ShowBossPreview(state);

  private void HandleStateChanged(RunState state)
  {
    if (state.phase == RunPhase.Title)
      ApplyPhase(RunPhase.Title);
    else if (state.phase == RunPhase.BossPreview)
      ShowBossPreview(state);
    else if (state.phase == RunPhase.Combat || state.phase == RunPhase.BossCombat)
      HideTitleAndPreview();
  }

  private void ShowBossPreview(RunState state)
  {
    if (titlePanel != null)
      titlePanel.SetActive(false);

    if (bossPreviewUI != null && state.assignedBoss != null)
      bossPreviewUI.ShowPreview(state.assignedBoss);
    else
      Debug.LogWarning("GameFlowController: BossPreviewUI or assigned boss is missing.");
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

    if (showTitle && bossPreviewUI != null)
      bossPreviewUI.HidePreview();
  }

  private void EnsureTitlePanel()
  {
    if (titlePanel != null)
      return;

    var canvas = FindFirstObjectByType<Canvas>();
    if (canvas == null)
      return;

    titlePanel = new GameObject("TitlePanel", typeof(RectTransform));
    var rt = titlePanel.GetComponent<RectTransform>();
    rt.SetParent(canvas.transform, false);
    rt.anchorMin = Vector2.zero;
    rt.anchorMax = Vector2.one;
    rt.offsetMin = Vector2.zero;
    rt.offsetMax = Vector2.zero;

    var titleGo = new GameObject("TitleText", typeof(RectTransform), typeof(TextMeshProUGUI));
    titleGo.transform.SetParent(titlePanel.transform, false);
    var titleRt = titleGo.GetComponent<RectTransform>();
    titleRt.anchorMin = titleRt.anchorMax = new Vector2(0.5f, 0.5f);
    titleRt.anchoredPosition = new Vector2(0, 120);
    titleRt.sizeDelta = new Vector2(500, 60);
    var titleTmp = titleGo.GetComponent<TextMeshProUGUI>();
    if (TMP_Settings.defaultFontAsset != null)
      titleTmp.font = TMP_Settings.defaultFontAsset;
    titleTmp.text = "GambleRogue";
    titleTmp.fontSize = 28;
    titleTmp.alignment = TextAlignmentOptions.Center;

    var btnGo = new GameObject("StartRunButton", typeof(RectTransform), typeof(Image), typeof(Button));
    btnGo.transform.SetParent(titlePanel.transform, false);
    var btnRt = btnGo.GetComponent<RectTransform>();
    btnRt.anchorMin = btnRt.anchorMax = new Vector2(0.5f, 0.5f);
    btnRt.anchoredPosition = new Vector2(0, 20);
    btnRt.sizeDelta = new Vector2(220, 44);
    btnGo.GetComponent<Image>().color = new Color(0.2f, 0.45f, 0.85f, 1f);

    var labelGo = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
    labelGo.transform.SetParent(btnGo.transform, false);
    var labelRt = labelGo.GetComponent<RectTransform>();
    labelRt.anchorMin = Vector2.zero;
    labelRt.anchorMax = Vector2.one;
    labelRt.offsetMin = Vector2.zero;
    labelRt.offsetMax = Vector2.zero;
    var labelTmp = labelGo.GetComponent<TextMeshProUGUI>();
    if (TMP_Settings.defaultFontAsset != null)
      labelTmp.font = TMP_Settings.defaultFontAsset;
    labelTmp.text = "Start Run";
    labelTmp.fontSize = 18;
    labelTmp.alignment = TextAlignmentOptions.Center;

    btnGo.GetComponent<Button>().onClick.AddListener(OnStartRunClicked);
  }
}

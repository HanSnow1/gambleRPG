using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>ESC pause overlay for combat: HP, boss traits, and action log.</summary>
public class CombatPauseOverlay : MonoBehaviour
{
  private BettingCombatSystem _combat;
  private GameObject _panel;
  private TMP_Text _leftHp;
  private TMP_Text _rightHp;
  private TMP_Text _bossInfo;
  private TMP_Text _log;
  private TMP_Text _combatLogSource;
  private RunMiniMapUI _miniMap;
  private Button _restartButton;
  private bool _paused;

  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
  private static void Bootstrap()
  {
    if (FindFirstObjectByType<CombatPauseOverlay>() != null)
      return;

    var go = new GameObject("CombatPauseOverlay");
    go.AddComponent<CombatPauseOverlay>();
  }

  private void Awake()
  {
    _combat = FindFirstObjectByType<BettingCombatSystem>();
    _miniMap = FindFirstObjectByType<RunMiniMapUI>();
    Build();
    ResolveCombatLogSource();
    _miniMap?.SetShowOnlyInPause(false);
  }

  private void OnDestroy()
  {
    if (_paused)
      Time.timeScale = 1f;

    _miniMap?.SetShowOnlyInPause(false);
  }

  private void Update()
  {
    var kb = Keyboard.current;
    if (kb != null && kb.escapeKey.wasPressedThisFrame)
      TogglePause();

    if (_paused)
      Refresh();
  }

  private void TogglePause()
  {
    if (_combat == null || !_combat.IsCombatActive)
      return;

    _paused = !_paused;
    Time.timeScale = _paused ? 0f : 1f;
    _panel.SetActive(_paused);
    _miniMap?.SetShowOnlyInPause(_paused);

    if (_paused)
      Refresh();
  }

  private void Refresh()
  {
    if (_combat == null)
      return;

    string enemyName = RunManager.Instance != null && RunManager.Instance.State.isActive && RunManager.Instance.State.currentFloor <= 1
      ? "주사위 보스"
      : "카드보스";
    if (_combat.CurrentBoss == null)
      enemyName = "적";

    _leftHp.text = $"자신 HP\n{_combat.State.playerHp} / {_combat.State.playerMaxHp}";
    _rightHp.text = $"{enemyName} HP\n{_combat.State.enemyHp} / {_combat.State.enemyMaxHp}";

    if (_combat.CurrentBoss != null)
    {
      _bossInfo.text = $"보스 특징\n\n{BossGambleResolvers.GetRuleLabel(_combat.CurrentBoss)}\n\n약점: {_combat.CurrentBoss.weaknessHint}";
    }
    else
    {
      _bossInfo.text = "보스 특징\n\n일반 전투";
    }

    ResolveCombatLogSource();
    _log.text = _combatLogSource != null && !string.IsNullOrEmpty(_combatLogSource.text)
      ? _combatLogSource.text
      : "행동 로그 없음";
  }

  private void ResolveCombatLogSource()
  {
    if (_combatLogSource != null)
      return;

    _combatLogSource = GameObject.Find("CombatLogText")?.GetComponent<TMP_Text>();
    if (_combatLogSource != null)
      return;

    // Fallback includes inactive objects so pause overlay can mirror combat log reliably.
    var allTexts = Resources.FindObjectsOfTypeAll<TMP_Text>();
    foreach (var candidate in allTexts)
    {
      if (candidate == null)
        continue;

      if (candidate.name == "CombatLogText")
      {
        _combatLogSource = candidate;
        return;
      }
    }
  }
  private void OnRestartFromPause()
  {
    _paused = false;
    Time.timeScale = 1f;
    if (_panel != null)
      _panel.SetActive(false);
    _miniMap?.SetShowOnlyInPause(false);

    var flow = FindFirstObjectByType<GameFlowController>();
    if (flow != null)
      flow.RestartFromCombat();
  }

  private void Build()
  {
    var canvas = FindFirstObjectByType<Canvas>();
    if (canvas == null)
      return;

    _panel = new GameObject("EscPausePanel", typeof(RectTransform), typeof(Image));
    _panel.transform.SetParent(canvas.transform, false);
    var rt = _panel.GetComponent<RectTransform>();
    rt.anchorMin = Vector2.zero;
    rt.anchorMax = Vector2.one;
    rt.offsetMin = Vector2.zero;
    rt.offsetMax = Vector2.zero;

    var panelImage = _panel.GetComponent<Image>();
    var bgTex = Resources.Load<Texture2D>("UI/esc_bg");
    if (bgTex != null)
    {
      panelImage.sprite = Sprite.Create(bgTex, new Rect(0f, 0f, bgTex.width, bgTex.height), new Vector2(0.5f, 0.5f), 100f);
      panelImage.type = Image.Type.Simple;
      panelImage.preserveAspect = false;
      panelImage.color = Color.white;
    }
    else
    {
      panelImage.color = new Color(0f, 0f, 0f, 1f);
    }

    // Darkening scrim over the background so text stays readable (combat stays fully hidden).
    var scrim = new GameObject("EscScrim", typeof(RectTransform), typeof(Image));
    scrim.transform.SetParent(_panel.transform, false);
    var scrimRt = scrim.GetComponent<RectTransform>();
    scrimRt.anchorMin = Vector2.zero;
    scrimRt.anchorMax = Vector2.one;
    scrimRt.offsetMin = Vector2.zero;
    scrimRt.offsetMax = Vector2.zero;
    scrim.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.45f);

    _leftHp = CreateText(_panel.transform, "EscLeftHp", new Vector2(0.05f, 0.95f), new Vector2(0.24f, 0.12f), TextAlignmentOptions.TopLeft);
    _rightHp = CreateText(_panel.transform, "EscRightHp", new Vector2(0.95f, 0.95f), new Vector2(0.24f, 0.12f), TextAlignmentOptions.TopRight);
    _bossInfo = CreateText(_panel.transform, "EscBossInfo", new Vector2(0.05f, 0.62f), new Vector2(0.35f, 0.3f), TextAlignmentOptions.TopLeft);
    _log = CreateText(_panel.transform, "EscLog", new Vector2(0.05f, 0.07f), new Vector2(0.46f, 0.45f), TextAlignmentOptions.BottomLeft);
    _restartButton = GameUITheme.CreateButton(_panel.transform, "EscRestartButton", "다시시작", new Vector2(0f, -220f), new Vector2(220f, 44f), GameUITheme.Accent, OnRestartFromPause);
    if (_restartButton != null)
    {
      var bRt = _restartButton.GetComponent<RectTransform>();
      bRt.anchorMin = bRt.anchorMax = new Vector2(1f, 0f);
      bRt.pivot = new Vector2(1f, 0f);
      bRt.anchoredPosition = new Vector2(-36f, 36f);
    }

    _panel.SetActive(false);
  }

  private static TMP_Text CreateText(Transform parent, string name, Vector2 anchorPos, Vector2 size, TextAlignmentOptions alignment)
  {
    var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
    go.transform.SetParent(parent, false);
    var rt = go.GetComponent<RectTransform>();
    rt.anchorMin = rt.anchorMax = anchorPos;
    rt.pivot = new Vector2(anchorPos.x, anchorPos.y);
    rt.sizeDelta = new Vector2(size.x * 1920f, size.y * 1080f);

    var text = go.GetComponent<TextMeshProUGUI>();
    GameUITheme.StyleBodyText(text);
    text.fontSize = 22;
    text.alignment = alignment;
    text.richText = true;
    return text;
  }
}

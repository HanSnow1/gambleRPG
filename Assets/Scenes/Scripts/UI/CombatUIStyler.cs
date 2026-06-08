using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Applies themed layout to combat HUD elements at runtime.</summary>
[DefaultExecutionOrder(-80)]
public class CombatUIStyler : MonoBehaviour
{
  [SerializeField] private CombatUI combatUI;

  private Image _playerHpFill;
  private Image _enemyHpFill;
  private TMP_Text _topBarText;

  private static Sprite _shieldIcon;
  private static Sprite _brokenShieldIcon;
  private static Sprite _swordIcon;

  private void Awake()
  {
    if (combatUI == null)
      combatUI = GetComponent<CombatUI>();

    var scaler = GetComponent<CanvasScaler>();
    GameUITheme.ConfigureCanvasScaler(scaler);
    GameUITheme.ApplyCanvasBackground(transform);

    BuildTopBar();
    StyleCombatElements();
    CombatUILayout.Apply(transform);
  }

  public void RefreshHpBars(int playerHp, int playerMax, int enemyHp, int enemyMax)
  {
    GameUITheme.SetHpBar(_playerHpFill, playerHp, playerMax);
    GameUITheme.SetHpBar(_enemyHpFill, enemyHp, enemyMax);
    RefreshTopBar();
  }

  private void BuildTopBar()
  {
    if (transform.Find("RunTopBar") != null)
      return;

    var bar = new GameObject("RunTopBar", typeof(RectTransform), typeof(Image));
    bar.transform.SetParent(transform, false);

    var rt = bar.GetComponent<RectTransform>();
    rt.anchorMin = new Vector2(0, 1);
    rt.anchorMax = new Vector2(1, 1);
    rt.pivot = new Vector2(0.5f, 1);
    rt.anchoredPosition = Vector2.zero;
    rt.sizeDelta = new Vector2(0, 36);

    bar.GetComponent<Image>().color = new Color(0.08f, 0.1f, 0.18f, 0.92f);

    _topBarText = GameUITheme.CreateText(
      bar.transform,
      "RunTopBarText",
      "GambleRogue",
      13,
      FontStyles.Bold,
      TextAlignmentOptions.MidlineLeft);
    var textRt = _topBarText.GetComponent<RectTransform>();
    textRt.anchorMin = Vector2.zero;
    textRt.anchorMax = Vector2.one;
    textRt.offsetMin = new Vector2(16, 0);
    textRt.offsetMax = new Vector2(-16, 0);
    _topBarText.color = GameUITheme.TextMuted;
  }

  private void RefreshTopBar()
  {
    if (_topBarText == null)
      return;

    string floor = "";
    string loadout = "";

    if (RunManager.Instance != null && RunManager.Instance.State.isActive)
    {
      var s = RunManager.Instance.State;
      floor = $"{s.currentFloor}층 / {RunState.TotalFloors}층";
    }

    if (PlayerAugmentState.Instance != null)
      loadout = PlayerAugmentState.Instance.GetSummaryLine();

    if (PlayerRelicState.Instance != null)
    {
      string relics = PlayerRelicState.Instance.GetSummaryLine();
      loadout = string.IsNullOrEmpty(loadout) ? relics : $"{loadout}  |  {relics}";
    }

    _topBarText.text = string.IsNullOrEmpty(floor)
      ? "GambleRogue"
      : $"{floor}   |   {loadout}";
  }

  private void StyleCombatElements()
  {
    StyleBetButton("BetSmall", "Safe");
    StyleBetButton("BetMedium", "Risk");
    StyleBetButton("BetLarge", "All-in");
    StyleBetButton("BetHidden", "Hidden");
    StyleRestartButton();

    var playerHp = FindTmp("PlayerHpText");
    var enemyHp = FindTmp("EnemyHpText");
    var status = FindTmp("StatusText");
    var bossRule = FindTmp("BossRuleText");
    var log = FindTmp("CombatLogText");

    if (playerHp != null)
    {
      GameUITheme.StyleTitleText(playerHp);
      playerHp.richText = true;
      var panel = GameUITheme.WrapTextInPanel(playerHp, "PlayerHpPanel", new Vector2(20, 28));
      if (panel != null)
      {
        MakeCombatTextPanelTransparent(panel);
        GameUITheme.CreateHpBarUnder(panel.transform, "PlayerHpBar", out _playerHpFill);
      }
    }

    if (enemyHp != null)
    {
      GameUITheme.StyleTitleText(enemyHp);
      enemyHp.richText = true;
      var panel = GameUITheme.WrapTextInPanel(enemyHp, "EnemyHpPanel", new Vector2(20, 28));
      if (panel != null)
      {
        MakeCombatTextPanelTransparent(panel);
        GameUITheme.CreateHpBarUnder(panel.transform, "EnemyHpBar", out _enemyHpFill);
      }
    }

    if (status != null)
    {
      var panel = GameUITheme.WrapTextInPanel(status, "StatusPanel", new Vector2(16, 12));
      MakeCombatTextPanelTransparent(panel);
      GameUITheme.StyleBodyText(status);
      status.fontSize = 15;
    }

    if (bossRule != null)
    {
      GameUITheme.WrapTextInPanel(bossRule, "BossRulePanel", new Vector2(16, 12));
      GameUITheme.StyleMutedText(bossRule);
      bossRule.fontSize = 13;
      bossRule.fontStyle = FontStyles.Normal;
      bossRule.gameObject.SetActive(false);
    }

    if (log != null)
    {
      var panel = GameUITheme.WrapTextInPanel(log, "CombatLogPanel", new Vector2(12, 8));
      MakeCombatTextPanelTransparent(panel);
      GameUITheme.StyleBodyText(log);
      log.fontSize = 12;
      log.alignment = TextAlignmentOptions.TopLeft;
      log.color = new Color(0.85f, 0.9f, 1f, 0.95f);
    }
  }

  private static void MakeCombatTextPanelTransparent(Image panel)
  {
    if (panel == null)
      return;

    panel.color = new Color(0f, 0f, 0f, 0f);
    panel.raycastTarget = false;

    var outline = panel.GetComponent<Outline>();
    if (outline != null)
      outline.enabled = false;
  }

  private void StyleBetButton(string objectName, string betLabel)
  {
    var t = FindChildRecursive(transform, objectName);
    if (t == null)
      return;

    var btn = t.GetComponent<Button>();
    if (btn != null)
    {
      var rt = btn.GetComponent<RectTransform>();
      if (rt != null)
        rt.sizeDelta = new Vector2(Mathf.Max(rt.sizeDelta.x, 140), Mathf.Max(rt.sizeDelta.y, 72));
      GameUITheme.StyleBetButton(btn, betLabel);
      EnsureBetIcon(btn.transform, betLabel);
    }
  }

  private static void EnsureBetIcon(Transform buttonRoot, string betLabel)
  {
    if (buttonRoot == null)
      return;

    var oldTextIcon = buttonRoot.Find("BetVisualIcon");
    if (oldTextIcon != null)
      oldTextIcon.gameObject.SetActive(false);

    var iconTransform = buttonRoot.Find("BetVisualIconImage");
    Image icon;
    if (iconTransform == null)
    {
      var iconGo = new GameObject("BetVisualIconImage", typeof(RectTransform), typeof(Image));
      iconGo.transform.SetParent(buttonRoot, false);
      icon = iconGo.GetComponent<Image>();
    }
    else
    {
      icon = iconTransform.GetComponent<Image>();
    }

    if (icon == null)
      return;

    var iconRt = icon.GetComponent<RectTransform>();
    iconRt.anchorMin = iconRt.anchorMax = new Vector2(0.5f, 1f);
    iconRt.pivot = new Vector2(0.5f, 0.5f);
    iconRt.anchoredPosition = new Vector2(0f, 20f);
    iconRt.sizeDelta = new Vector2(48f, 48f);

    icon.raycastTarget = false;
    icon.preserveAspect = true;

    switch (betLabel)
    {
      case "Safe":
        icon.sprite = GetShieldIcon();
        icon.color = Color.white;
        icon.enabled = true;
        break;
      case "Risk":
        icon.sprite = GetBrokenShieldIcon();
        icon.color = Color.white;
        icon.enabled = true;
        break;
      case "All-in":
        icon.sprite = GetSwordIcon();
        icon.color = Color.white;
        icon.enabled = true;
        break;
      default:
        // No icon (e.g. Hidden): hide the Image so it does not render a white box.
        icon.sprite = null;
        icon.enabled = false;
        break;
    }

    var label = buttonRoot.GetComponentInChildren<TMP_Text>();
    if (label != null)
      ConfigureBetLabelRect(label, hasIcon: betLabel != "Hidden");
  }

  // Keeps the multi-line bet text inside the button so it never overlaps other UI.
  private static void ConfigureBetLabelRect(TMP_Text label, bool hasIcon)
  {
    var labelRt = label.GetComponent<RectTransform>();
    labelRt.anchorMin = Vector2.zero;
    labelRt.anchorMax = Vector2.one;
    labelRt.pivot = new Vector2(0.5f, 0.5f);
    labelRt.offsetMin = new Vector2(8f, 6f);
    // Leave headroom at the top for the floating icon so text doesn't collide with it.
    labelRt.offsetMax = new Vector2(-8f, hasIcon ? -10f : -6f);

    label.alignment = TextAlignmentOptions.Center;
    label.textWrappingMode = TextWrappingModes.Normal;
    label.lineSpacing = -6f;
    label.enableAutoSizing = true;
    label.fontSizeMin = 9f;
    label.fontSizeMax = 14f;
  }

  private static Sprite GetShieldIcon()
  {
    if (_shieldIcon == null)
      _shieldIcon = CreateIconSprite(tex =>
      {
        DrawBadge(tex, GameUITheme.SafeBet);
        DrawShieldOutline(tex);
      });
    return _shieldIcon;
  }

  private static Sprite GetBrokenShieldIcon()
  {
    if (_brokenShieldIcon == null)
      _brokenShieldIcon = CreateIconSprite(tex =>
      {
        DrawBadge(tex, GameUITheme.RiskBet);
        DrawShieldOutline(tex);
        DrawThickLine(tex, 35, 16, 27, 32, Color.white, 3);
        DrawThickLine(tex, 27, 32, 36, 48, Color.white, 3);
      });
    return _brokenShieldIcon;
  }

  private static Sprite GetSwordIcon()
  {
    if (_swordIcon == null)
      _swordIcon = CreateIconSprite(tex =>
      {
        DrawBadge(tex, GameUITheme.AllInBet);
        DrawSword(tex);
      });
    return _swordIcon;
  }

  private static Sprite CreateIconSprite(System.Action<Texture2D> draw)
  {
    var tex = new Texture2D(128, 128, TextureFormat.RGBA32, false);
    tex.filterMode = FilterMode.Bilinear;
    var clear = new Color(0f, 0f, 0f, 0f);
    for (int y = 0; y < tex.height; y++)
    {
      for (int x = 0; x < tex.width; x++)
        tex.SetPixel(x, y, clear);
    }

    draw(tex);
    tex.Apply();
    return Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
  }

  private static void DrawBadge(Texture2D tex, Color color)
  {
    for (int y = ToTex(tex, 5f); y <= ToTex(tex, 59f); y++)
    {
      float logicalY = FromTex(tex, y);
      float topSlant = Mathf.Lerp(3f, 8f, logicalY / 64f);
      int minX = ToTex(tex, logicalY < 17f ? 6f + (17f - logicalY) * 0.12f : 5f);
      int maxX = ToTex(tex, 59f - topSlant * 0.22f);
      if (logicalY > 49f)
        maxX -= ToTex(tex, (logicalY - 49f) * 0.28f);

      for (int x = minX; x <= maxX; x++)
      {
        if (x < 0 || x >= tex.width)
          continue;
        tex.SetPixel(x, y, color);
      }
    }
  }

  private static void DrawShieldOutline(Texture2D tex)
  {
    DrawThickLine(tex, 18, 39, 19, 25, Color.white, 4);
    DrawThickLine(tex, 19, 25, 32, 25, Color.white, 4);
    DrawThickLine(tex, 32, 25, 47, 36, Color.white, 4);
    DrawThickLine(tex, 47, 36, 37, 50, Color.white, 4);
    DrawThickLine(tex, 37, 50, 24, 56, Color.white, 4);
    DrawThickLine(tex, 24, 56, 18, 39, Color.white, 4);
  }

  private static void DrawSword(Texture2D tex)
  {
    DrawThickLine(tex, 20, 14, 47, 51, Color.white, 4);
    DrawThickLine(tex, 47, 51, 50, 58, Color.white, 3);
    DrawThickLine(tex, 15, 25, 29, 15, Color.white, 4);
    DrawThickLine(tex, 16, 10, 29, 24, Color.white, 3);
  }

  private static void DrawThickLine(Texture2D tex, int x0, int y0, int x1, int y1, Color color, int thickness)
  {
    x0 = ToTex(tex, x0);
    y0 = ToTex(tex, y0);
    x1 = ToTex(tex, x1);
    y1 = ToTex(tex, y1);
    thickness = Mathf.Max(1, ToTex(tex, thickness));

    int dx = Mathf.Abs(x1 - x0);
    int sx = x0 < x1 ? 1 : -1;
    int dy = -Mathf.Abs(y1 - y0);
    int sy = y0 < y1 ? 1 : -1;
    int err = dx + dy;

    while (true)
    {
      DrawDot(tex, x0, y0, color, thickness);
      if (x0 == x1 && y0 == y1)
        break;

      int e2 = 2 * err;
      if (e2 >= dy)
      {
        err += dy;
        x0 += sx;
      }
      if (e2 <= dx)
      {
        err += dx;
        y0 += sy;
      }
    }
  }

  private static void DrawDot(Texture2D tex, int cx, int cy, Color color, int radius)
  {
    for (int y = cy - radius; y <= cy + radius; y++)
    {
      for (int x = cx - radius; x <= cx + radius; x++)
      {
        if (x < 0 || x >= tex.width || y < 0 || y >= tex.height)
          continue;
        if ((x - cx) * (x - cx) + (y - cy) * (y - cy) > radius * radius)
          continue;
        // Clip the white overlay to the badge silhouette so it never pokes outside.
        if (tex.GetPixel(x, y).a <= 0.02f)
          continue;
        tex.SetPixel(x, y, color);
      }
    }
  }

  private static int ToTex(Texture2D tex, float logical) =>
    Mathf.RoundToInt(logical * tex.width / 64f);

  private static float FromTex(Texture2D tex, int pixel) =>
    pixel * 64f / tex.width;

  private void StyleRestartButton()
  {
    var t = FindChildRecursive(transform, "RestartButton");
    if (t == null)
      return;

    var btn = t.GetComponent<Button>();
    var img = t.GetComponent<Image>();
    if (img != null)
      img.color = GameUITheme.Border;

    if (btn != null)
    {
      var tmp = btn.GetComponentInChildren<TMP_Text>();
      if (tmp != null)
      {
        tmp.text = GameUIText.Restart;
        GameUITheme.StyleMutedText(tmp);
        tmp.fontStyle = FontStyles.Normal;
      }
    }
  }

  private TMP_Text FindTmp(string name) =>
    FindChildRecursive(transform, name)?.GetComponent<TMP_Text>();

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
}

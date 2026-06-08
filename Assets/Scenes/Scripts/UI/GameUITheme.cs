using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Shared casino-roguelike UI palette and builders.</summary>
public static class GameUITheme
{
  public static readonly Color BgDeep = Hex("#0D0F1A");
  public static readonly Color BgPanel = Hex("#161B2E");
  public static readonly Color BgPanelLight = Hex("#1E2640");
  public static readonly Color Border = Hex("#2E3A5C");
  public static readonly Color Accent = Hex("#4A8CFF");
  public static readonly Color AccentHover = Hex("#6AA3FF");
  public static readonly Color TextPrimary = Hex("#EEF2FF");
  public static readonly Color TextMuted = Hex("#8A9BB8");
  public static readonly Color SafeBet = Hex("#3CB371");
  public static readonly Color RiskBet = Hex("#E8A838");
  public static readonly Color AllInBet = Hex("#E74C5C");
  public static readonly Color Heal = Hex("#7DFFAA");
  public static readonly Color HpHigh = Hex("#5DDF8A");
  public static readonly Color HpMid = Hex("#E8C547");
  public static readonly Color HpLow = Hex("#FF6B6B");

  private static readonly Color TavernButton = Hex("#7A4826");
  private static readonly Color TavernButtonPressed = Hex("#4A2A16");

  public static Color Hex(string hex)
  {
    if (ColorUtility.TryParseHtmlString(hex, out Color c))
      return c;
    return Color.white;
  }

  // Slightly blurred full-screen background loaded from Resources/UI/title_bg.
  private static Sprite _backgroundSprite;
  private static bool _backgroundLoaded;

  public static Sprite GetBackgroundSprite()
  {
    if (_backgroundLoaded)
      return _backgroundSprite;

    _backgroundLoaded = true;
    var tex = Resources.Load<Texture2D>("UI/title_bg");
    if (tex != null)
    {
      _backgroundSprite = Sprite.Create(
        tex,
        new Rect(0f, 0f, tex.width, tex.height),
        new Vector2(0.5f, 0.5f),
        100f);
    }

    return _backgroundSprite;
  }

  public static void ApplyCanvasBackground(Transform canvas)
  {
    if (canvas == null || canvas.Find("UIBackground") != null)
      return;

    var bg = new GameObject("UIBackground", typeof(RectTransform), typeof(Image));
    bg.transform.SetParent(canvas, false);
    bg.transform.SetAsFirstSibling();

    var rt = bg.GetComponent<RectTransform>();
    rt.anchorMin = Vector2.zero;
    rt.anchorMax = Vector2.one;
    rt.offsetMin = Vector2.zero;
    rt.offsetMax = Vector2.zero;

    var img = bg.GetComponent<Image>();
    var sprite = GetBackgroundSprite();
    if (sprite != null)
    {
      img.sprite = sprite;
      img.type = Image.Type.Simple;
      img.preserveAspect = false;
      img.color = Color.white;
    }
    else
    {
      img.color = BgDeep;
    }
  }

  public static void ConfigureCanvasScaler(CanvasScaler scaler)
  {
    if (scaler == null)
      return;

    scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
    scaler.referenceResolution = new Vector2(1280, 720);
    scaler.matchWidthOrHeight = 0.5f;
  }

  public static void StyleOverlayPanel(Image image)
  {
    if (image == null)
      return;
    image.color = new Color(BgPanel.r, BgPanel.g, BgPanel.b, 0.96f);
    ApplyTavernOutline(image, Border, new Vector2(2f, -2f));
  }

  public static Image WrapTextInPanel(TMP_Text text, string panelName, Vector2 padding)
  {
    if (text == null || text.transform.parent?.name == panelName)
      return null;

    var panel = new GameObject(panelName, typeof(RectTransform), typeof(Image));
    panel.transform.SetParent(text.transform.parent, false);
    panel.transform.SetSiblingIndex(text.transform.GetSiblingIndex());

    var textRt = text.GetComponent<RectTransform>();
    var panelRt = panel.GetComponent<RectTransform>();
    panelRt.anchorMin = textRt.anchorMin;
    panelRt.anchorMax = textRt.anchorMax;
    panelRt.pivot = textRt.pivot;
    panelRt.anchoredPosition = textRt.anchoredPosition;
    panelRt.sizeDelta = textRt.sizeDelta + padding;

    var img = panel.GetComponent<Image>();
    img.color = new Color(BgPanelLight.r, BgPanelLight.g, BgPanelLight.b, 0.88f);
    ApplyTavernOutline(img, Border, new Vector2(1.5f, -1.5f));

    text.transform.SetParent(panel.transform, false);
    var innerRt = text.GetComponent<RectTransform>();
    innerRt.anchorMin = Vector2.zero;
    innerRt.anchorMax = Vector2.one;
    innerRt.offsetMin = new Vector2(8, 6);
    innerRt.offsetMax = new Vector2(-8, -6);

    StyleBodyText(text);
    return img;
  }

  public static Image CreateHpBarUnder(Transform parent, string name, out Image fill)
  {
    var barRoot = new GameObject(name, typeof(RectTransform), typeof(Image));
    barRoot.transform.SetParent(parent, false);

    var rootRt = barRoot.GetComponent<RectTransform>();
    rootRt.anchorMin = new Vector2(0, 0);
    rootRt.anchorMax = new Vector2(1, 0);
    rootRt.pivot = new Vector2(0.5f, 1f);
    rootRt.anchoredPosition = new Vector2(0, -4);
    rootRt.sizeDelta = new Vector2(0, 8);

    barRoot.GetComponent<Image>().color = new Color(BgDeep.r, BgDeep.g, BgDeep.b, 0.72f);

    var fillGo = new GameObject("Fill", typeof(RectTransform), typeof(Image));
    fillGo.transform.SetParent(barRoot.transform, false);
    var fillRt = fillGo.GetComponent<RectTransform>();
    fillRt.anchorMin = Vector2.zero;
    fillRt.anchorMax = Vector2.one;
    fillRt.offsetMin = new Vector2(2, 2);
    fillRt.offsetMax = new Vector2(-2, -2);

    fill = fillGo.GetComponent<Image>();
    fill.type = Image.Type.Filled;
    fill.fillMethod = Image.FillMethod.Horizontal;
    fill.fillOrigin = (int)Image.OriginHorizontal.Left;
    fill.color = HpHigh;
    return barRoot.GetComponent<Image>();
  }

  public static void SetHpBar(Image fill, int current, int max)
  {
    if (fill == null)
      return;

    float ratio = max > 0 ? Mathf.Clamp01(current / (float)max) : 0f;
    fill.fillAmount = ratio;
    fill.color = ratio > 0.55f ? HpHigh : ratio > 0.25f ? HpMid : HpLow;
  }

  public static string FormatHpRich(int current, int max, string label)
  {
    float ratio = max > 0 ? current / (float)max : 0f;
    string color = ratio > 0.55f ? "#5DDF8A" : ratio > 0.25f ? "#E8C547" : "#FF6B6B";
    return $"<color={color}><b>{label}</b></color>  {current} / {max}";
  }

  public static void StyleBetButton(Button button, string betLabel)
  {
    if (button == null)
      return;

    var img = button.GetComponent<Image>();
    Color baseColor = betLabel switch
    {
      "Safe" => SafeBet,
      "Risk" => RiskBet,
      "All-in" => AllInBet,
      "Hidden" => Hex("#6B4A30"),
      _ => Accent
    };

    if (img != null)
    {
      Color fill = baseColor;
      fill.a = 0.95f;
      ApplyRoundedFrameStyle(img, fill);
    }

    var colors = button.colors;
    colors.normalColor = Color.white;
    colors.highlightedColor = new Color(1.12f, 1.08f, 0.98f);
    colors.pressedColor = new Color(0.72f, 0.58f, 0.42f);
    colors.disabledColor = new Color(0.35f, 0.27f, 0.2f, 0.7f);
    button.colors = colors;

    var tmp = button.GetComponentInChildren<TMP_Text>();
    if (tmp != null)
    {
      tmp.color = TextPrimary;
      tmp.fontSize = 13;
      tmp.fontStyle = FontStyles.Bold;
    }
  }

  public static Button CreateButton(
    Transform parent,
    string name,
    string label,
    Vector2 anchoredPos,
    Vector2 size,
    Color bgColor,
    UnityEngine.Events.UnityAction onClick = null)
  {
    var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
    go.transform.SetParent(parent, false);

    var rt = go.GetComponent<RectTransform>();
    rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
    rt.anchoredPosition = anchoredPos;
    rt.sizeDelta = size;

    var img = go.GetComponent<Image>();
    // GambleRogue-style: translucent dark fill (slightly tinted) + gold rounded border.
    Color fill = Color.Lerp(BgDeep, bgColor, 0.32f);
    fill.a = 0.8f;
    ApplyRoundedFrameStyle(img, fill);

    var btn = go.GetComponent<Button>();
    var colors = btn.colors;
    colors.normalColor = Color.white;
    colors.highlightedColor = new Color(1.12f, 1.08f, 0.98f);
    colors.pressedColor = TavernButtonPressed;
    colors.disabledColor = new Color(0.35f, 0.27f, 0.2f, 0.6f);
    btn.colors = colors;

    if (onClick != null)
      btn.onClick.AddListener(onClick);

    var labelTmp = CreateText(go.transform, "Text", label, 16, FontStyles.Bold);
    var labelRt = labelTmp.GetComponent<RectTransform>();
    labelRt.anchorMin = Vector2.zero;
    labelRt.anchorMax = Vector2.one;
    labelRt.offsetMin = Vector2.zero;
    labelRt.offsetMax = Vector2.zero;
    labelTmp.alignment = TextAlignmentOptions.Center;
    labelTmp.color = TextPrimary;

    return btn;
  }

  public static TMP_Text CreateText(
    Transform parent,
    string name,
    string text,
    int fontSize,
    FontStyles style = FontStyles.Normal,
    TextAlignmentOptions alignment = TextAlignmentOptions.Center)
  {
    var go = new GameObject(name, typeof(RectTransform));
    go.transform.SetParent(parent, false);

    var tmp = go.AddComponent<TextMeshProUGUI>();
    ApplyFont(tmp);
    tmp.text = text;
    tmp.fontSize = fontSize;
    tmp.fontStyle = style;
    tmp.alignment = alignment;
    tmp.color = TextPrimary;
    tmp.textWrappingMode = TextWrappingModes.Normal;
    tmp.richText = true;
    return tmp;
  }

  public static void StyleTitleText(TMP_Text tmp)
  {
    if (tmp == null)
      return;
    ApplyFont(tmp);
    tmp.color = TextPrimary;
    tmp.fontStyle = FontStyles.Bold;
    tmp.richText = true;
  }

  public static void StyleBodyText(TMP_Text tmp)
  {
    if (tmp == null)
      return;
    ApplyFont(tmp);
    tmp.color = TextPrimary;
    tmp.richText = true;
  }

  public static void StyleMutedText(TMP_Text tmp)
  {
    if (tmp == null)
      return;
    ApplyFont(tmp);
    tmp.color = TextMuted;
    tmp.fontStyle = FontStyles.Italic;
  }

  public static Color GetAugmentTierColor(AugmentTier tier) =>
    tier switch
    {
      AugmentTier.Silver => Hex("#B9C0C9"),
      AugmentTier.Gold => Hex("#D8A73C"),
      AugmentTier.Prismatic => Hex("#A855F7"),
      _ => Hex("#4A5568")
    };

  private static void ApplyTavernOutline(Graphic graphic, Color color, Vector2 distance)
  {
    if (graphic == null)
      return;

    var outline = graphic.GetComponent<Outline>() ?? graphic.gameObject.AddComponent<Outline>();
    outline.effectColor = new Color(color.r, color.g, color.b, 0.88f);
    outline.effectDistance = distance;
    outline.useGraphicAlpha = true;
  }

  // Shared "GambleRogue" style: rounded translucent dark fill with a gold border ring.
  public static void ApplyRoundedFrameStyle(Image img, Color fillColor, float goldAlpha = 1f)
  {
    if (img == null)
      return;

    img.sprite = GetButtonFillSprite();
    img.type = Image.Type.Sliced;
    img.color = fillColor;

    var oldOutline = img.GetComponent<Outline>();
    if (oldOutline != null)
      Object.Destroy(oldOutline);

    var existing = img.transform.Find("GoldFrame");
    GameObject frameGo = existing != null
      ? existing.gameObject
      : new GameObject("GoldFrame", typeof(RectTransform), typeof(Image));
    if (existing == null)
      frameGo.transform.SetParent(img.transform, false);

    var frt = frameGo.GetComponent<RectTransform>();
    frt.anchorMin = Vector2.zero;
    frt.anchorMax = Vector2.one;
    frt.offsetMin = Vector2.zero;
    frt.offsetMax = Vector2.zero;

    var fimg = frameGo.GetComponent<Image>();
    fimg.sprite = GetButtonGoldRingSprite();
    fimg.type = Image.Type.Sliced;
    fimg.raycastTarget = false;
    fimg.color = new Color(0.957f, 0.788f, 0.365f, goldAlpha);

    // Behind the label, above the fill.
    frameGo.transform.SetAsFirstSibling();
  }

  private static Sprite _buttonFillSprite;
  private static Sprite _buttonGoldRingSprite;

  private static Sprite GetButtonFillSprite() => _buttonFillSprite ??= BuildRoundedSprite(96, 22f, -1f);
  private static Sprite GetButtonGoldRingSprite() => _buttonGoldRingSprite ??= BuildRoundedSprite(96, 22f, 6f);

  // Rounded rectangle sprite (white, alpha-shaped). borderThickness < 0 => filled; else a ring of that thickness.
  private static Sprite BuildRoundedSprite(int size, float radius, float borderThickness)
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
          a = Mathf.Clamp01(0.5f - dist);
        else
          a = Mathf.Clamp01(0.5f - Mathf.Abs(dist + borderThickness * 0.5f) + borderThickness * 0.5f)
              * Mathf.Clamp01(0.5f - dist);
        px[y * size + x] = new Color(1f, 1f, 1f, a);
      }
    }
    tex.SetPixels(px);
    tex.Apply();
    int b = Mathf.CeilToInt(radius) + 2;
    return Sprite.Create(tex, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(b, b, b, b));
  }

  private static void ApplyFont(TMP_Text tmp)
  {
    if (KoreanFontSetup.IsReady)
      KoreanFontSetup.Apply(tmp);
    else if (TMP_Settings.defaultFontAsset != null)
      tmp.font = TMP_Settings.defaultFontAsset;
  }
}

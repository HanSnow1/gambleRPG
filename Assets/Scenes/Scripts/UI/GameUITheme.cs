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

  public static Color Hex(string hex)
  {
    if (ColorUtility.TryParseHtmlString(hex, out Color c))
      return c;
    return Color.white;
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
    bg.GetComponent<Image>().color = BgDeep;
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

    barRoot.GetComponent<Image>().color = new Color(0, 0, 0, 0.45f);

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
      "Hidden" => Hex("#5B6B8A"),
      _ => Accent
    };

    if (img != null)
      img.color = baseColor;

    var colors = button.colors;
    colors.normalColor = Color.white;
    colors.highlightedColor = new Color(1.1f, 1.1f, 1.1f);
    colors.pressedColor = new Color(0.85f, 0.85f, 0.85f);
    colors.disabledColor = new Color(0.45f, 0.45f, 0.45f, 0.7f);
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
    img.color = bgColor;

    var btn = go.GetComponent<Button>();
    var colors = btn.colors;
    colors.normalColor = Color.white;
    colors.highlightedColor = new Color(1.08f, 1.08f, 1.08f);
    colors.pressedColor = new Color(0.88f, 0.88f, 0.88f);
    colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.6f);
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
      AugmentTier.Silver => Hex("#6B7A8F"),
      AugmentTier.Gold => Hex("#C9A227"),
      AugmentTier.Prismatic => Hex("#9B59D4"),
      _ => Hex("#4A5568")
    };

  private static void ApplyFont(TMP_Text tmp)
  {
    if (KoreanFontSetup.IsReady)
      KoreanFontSetup.Apply(tmp);
    else if (TMP_Settings.defaultFontAsset != null)
      tmp.font = TMP_Settings.defaultFontAsset;
  }
}

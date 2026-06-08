using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(-60)]
public class AugmentPickUI : MonoBehaviour
{
  [SerializeField] private AugmentCatalog catalog;
  [SerializeField] private GameObject pickPanel;
  [SerializeField] private TMP_Text titleText;

  private readonly List<Button> _cardButtons = new();
  private readonly List<TMP_Text> _cardNameTexts = new();
  private readonly List<TMP_Text> _cardBodyTexts = new();
  private readonly List<Image> _cardBackgrounds = new();
  private readonly List<Image> _cardBorders = new();

  private List<AugmentDefinition> _currentChoices = new();
  private Action<AugmentDefinition> _onPicked;

  private void Awake()
  {
    if (catalog == null)
      catalog = Resources.Load<AugmentCatalog>("AugmentCatalog");

    EnsurePanelExists();
  }

  public void ShowPick(Action<AugmentDefinition> onPicked)
  {
    _onPicked = onPicked;

    int tierBias = PlayerAugmentState.Instance != null
      ? PlayerAugmentState.Instance.ConsumeCrownUpgradeTierBias()
      : 0;

    _currentChoices = AugmentSelector.PickChoices(
      catalog != null ? catalog.allAugments : Array.Empty<AugmentDefinition>(),
      PlayerAugmentState.Instance,
      3,
      tierBias);

    if (tierBias > 0)
      Debug.Log($"Crown Upgrade: draft minimum tier raised by {tierBias} step(s).");

    if (_currentChoices.Count == 0)
    {
      Debug.LogWarning("AugmentPickUI: No augments available in catalog.");
      onPicked?.Invoke(null);
      return;
    }

    if (titleText != null)
      titleText.text = GameUIText.ChooseAugment;

    for (int i = 0; i < _cardButtons.Count; i++)
    {
      bool active = i < _currentChoices.Count;
      _cardButtons[i].gameObject.SetActive(active);
      if (!active)
        continue;

      var def = _currentChoices[i];
      int previewLevel = PlayerAugmentState.Instance != null
        ? Mathf.Min(PlayerAugmentState.Instance.GetLevel(def) + 1, def.maxLevel)
        : 1;

      string tierLabel = GameUIText.TierLabel(def.tier);

      if (_cardNameTexts[i] != null)
      {
        _cardNameTexts[i].text =
          $"<color=#{ColorUtility.ToHtmlStringRGB(GameUITheme.GetAugmentTierColor(def.tier))}>" +
          $"{def.displayName}</color>\n[{tierLabel}]  Lv{previewLevel}/{def.maxLevel}";
      }

      if (_cardBodyTexts[i] != null)
      {
        _cardBodyTexts[i].text = def.GetDescriptionForLevel(previewLevel);
        _cardBodyTexts[i].color = GameUITheme.TextPrimary;
      }

      if (_cardBorders[i] != null)
      {
        // Each tier uses its own ornate frame artwork (gold / silver / platinum).
        var tierFrame = GetTierFrameSprite(def.tier);
        if (tierFrame != null)
          _cardBorders[i].sprite = tierFrame;
        _cardBorders[i].color = Color.white;
      }

      if (_cardBackgrounds[i] != null)
      {
        // Subtle tier hue on the dark inner backdrop.
        Color tierColor = GameUITheme.GetAugmentTierColor(def.tier);
        _cardBackgrounds[i].color = Color.Lerp(new Color(0.13f, 0.125f, 0.2f, 0.98f), tierColor, 0.1f);
      }
    }

    if (pickPanel != null)
    {
      pickPanel.transform.SetAsLastSibling();
      pickPanel.SetActive(true);
    }
  }

  public void Hide()
  {
    if (pickPanel != null)
      pickPanel.SetActive(false);
  }

  private void OnCardClicked(int index)
  {
    if (index < 0 || index >= _currentChoices.Count)
      return;

    var picked = _currentChoices[index];
    Hide();
    _onPicked?.Invoke(picked);
    _onPicked = null;
  }

  private void EnsurePanelExists()
  {
    if (pickPanel != null)
      return;

    var canvas = GetComponent<Canvas>() ?? FindFirstObjectByType<Canvas>();
    if (canvas == null)
    {
      Debug.LogError("AugmentPickUI: No Canvas found.");
      return;
    }

    pickPanel = BuildRuntimePanel(canvas.transform);
  }

  private GameObject BuildRuntimePanel(Transform canvasRoot)
  {
    var panel = new GameObject("AugmentPickPanel", typeof(RectTransform), typeof(Image));
    panel.transform.SetParent(canvasRoot, false);

    var panelRect = panel.GetComponent<RectTransform>();
    panelRect.anchorMin = Vector2.zero;
    panelRect.anchorMax = Vector2.one;
    panelRect.offsetMin = Vector2.zero;
    panelRect.offsetMax = Vector2.zero;

    GameUITheme.StyleOverlayPanel(panel.GetComponent<Image>());

    // Dark backdrop behind the header so the background art doesn't clutter the text.
    var headerBackdrop = new GameObject("HeaderBackdrop", typeof(RectTransform), typeof(Image));
    headerBackdrop.transform.SetParent(panel.transform, false);
    var hbRt = headerBackdrop.GetComponent<RectTransform>();
    hbRt.anchorMin = hbRt.anchorMax = new Vector2(0.5f, 0.5f);
    hbRt.anchoredPosition = new Vector2(0f, 280f);
    hbRt.sizeDelta = new Vector2(900f, 140f);
    var hbImg = headerBackdrop.GetComponent<Image>();
    hbImg.sprite = GetRoundedPanelSprite();
    hbImg.type = Image.Type.Sliced;
    hbImg.color = new Color(0.04f, 0.05f, 0.09f, 0.62f);
    hbImg.raycastTarget = false;

    titleText = GameUITheme.CreateText(
      panel.transform,
      "AugmentPickTitle",
      GameUIText.ChooseAugment,
      32,
      FontStyles.Bold);
    var titleRt = titleText.GetComponent<RectTransform>();
    titleRt.anchorMin = titleRt.anchorMax = new Vector2(0.5f, 0.5f);
    titleRt.anchoredPosition = new Vector2(0, 298);
    titleRt.sizeDelta = new Vector2(760, 50);
    titleText.color = GameUITheme.Accent;
    AddTextReadability(titleText, 2.6f);

    var subtitle = GameUITheme.CreateText(
      panel.transform,
      "AugmentPickSubtitle",
      GameUIText.AugmentPickSubtitle,
      15,
      FontStyles.Italic);
    var subRt = subtitle.GetComponent<RectTransform>();
    subRt.anchorMin = subRt.anchorMax = new Vector2(0.5f, 0.5f);
    subRt.anchoredPosition = new Vector2(0, 258);
    subRt.sizeDelta = new Vector2(760, 32);
    subtitle.color = new Color(0.86f, 0.9f, 1f, 1f);
    AddTextReadability(subtitle, 1.6f);

    float[] xPositions = { -300f, 0f, 300f };
    for (int i = 0; i < 3; i++)
      BuildCard(panel.transform, i, xPositions[i]);

    panel.SetActive(false);
    return panel;
  }

  private void BuildCard(Transform parent, int index, float xPos)
  {
    var card = new GameObject($"AugmentCard{index}", typeof(RectTransform), typeof(Image), typeof(Button));
    card.transform.SetParent(parent, false);

    var rect = card.GetComponent<RectTransform>();
    rect.anchorMin = new Vector2(0.5f, 0.5f);
    rect.anchorMax = new Vector2(0.5f, 0.5f);
    rect.anchoredPosition = new Vector2(xPos, -28f);
    // Match the ornate frame texture aspect ratio (475 x 847).
    rect.sizeDelta = new Vector2(286f, 510f);

    // The card's own image is invisible; the InnerPanel below provides the backdrop
    // so it can be sized to fit within the ornate frame's arch.
    var bg = card.GetComponent<Image>();
    bg.color = new Color(0f, 0f, 0f, 0f);

    // Dark inner backdrop that sits inside the ornate frame (text reads on this).
    var innerPanel = new GameObject("InnerPanel", typeof(RectTransform), typeof(Image));
    innerPanel.transform.SetParent(card.transform, false);
    var innerRt = innerPanel.GetComponent<RectTransform>();
    innerRt.anchorMin = new Vector2(0.5f, 0.5f);
    innerRt.anchorMax = new Vector2(0.5f, 0.5f);
    // Kept inside the frame's wide middle so it never shows a square block below the bottom orb.
    innerRt.anchoredPosition = new Vector2(0f, 17f);
    innerRt.sizeDelta = new Vector2(184f, 254f);
    var innerImg = innerPanel.GetComponent<Image>();
    innerImg.sprite = GetRoundedPanelSprite();
    innerImg.type = Image.Type.Sliced;
    innerImg.color = new Color(0.13f, 0.125f, 0.2f, 0.98f);
    innerImg.raycastTarget = false;

    // Ornate gold frame artwork on top of the backdrop.
    var frame = new GameObject("Frame", typeof(RectTransform), typeof(Image));
    frame.transform.SetParent(card.transform, false);
    var frameRt = frame.GetComponent<RectTransform>();
    frameRt.anchorMin = Vector2.zero;
    frameRt.anchorMax = Vector2.one;
    frameRt.offsetMin = Vector2.zero;
    frameRt.offsetMax = Vector2.zero;
    var frameImg = frame.GetComponent<Image>();
    frameImg.sprite = LoadSprite("UI/augment_card_frame");
    frameImg.type = Image.Type.Simple;
    frameImg.color = Color.white;
    frameImg.raycastTarget = false;
    _cardBorders.Add(frameImg);
    _cardBackgrounds.Add(innerImg);

    var nameTmp = GameUITheme.CreateText(card.transform, "Name", "증강", 19, FontStyles.Bold);
    var nameRt = nameTmp.GetComponent<RectTransform>();
    nameRt.anchoredPosition = new Vector2(0, 116);
    nameRt.sizeDelta = new Vector2(172f, 48f);
    AddTextReadability(nameTmp, 2.4f);

    var bodyTmp = GameUITheme.CreateText(
      card.transform,
      "Body",
      "설명",
      14,
      alignment: TextAlignmentOptions.Center);
    var bodyRt = bodyTmp.GetComponent<RectTransform>();
    bodyRt.anchoredPosition = new Vector2(0, 12f);
    bodyRt.sizeDelta = new Vector2(168f, 134f);
    bodyTmp.color = GameUITheme.TextPrimary;
    AddTextReadability(bodyTmp, 1.6f);

    var button = card.GetComponent<Button>();
    var colors = button.colors;
    colors.normalColor = Color.white;
    colors.highlightedColor = new Color(1.05f, 1.05f, 1.1f);
    colors.pressedColor = new Color(0.9f, 0.9f, 0.95f);
    button.colors = colors;

    int captured = index;
    button.onClick.AddListener(() => OnCardClicked(captured));

    _cardButtons.Add(button);
    _cardNameTexts.Add(nameTmp);
    _cardBodyTexts.Add(bodyTmp);
  }

  // Dark outline + drop shadow so card text stays legible over the ornate frame art.
  private static void AddTextReadability(TMP_Text tmp, float shadowDist)
  {
    if (tmp == null)
      return;

    var outline = tmp.GetComponent<Outline>() ?? tmp.gameObject.AddComponent<Outline>();
    outline.effectColor = new Color(0f, 0f, 0f, 0.92f);
    outline.effectDistance = new Vector2(1.4f, -1.4f);

    var shadow = tmp.GetComponent<Shadow>() ?? tmp.gameObject.AddComponent<Shadow>();
    shadow.effectColor = new Color(0f, 0f, 0f, 0.6f);
    shadow.effectDistance = new Vector2(shadowDist, -shadowDist);
  }

  private static void AddCardDecorations(Transform card)
  {
    CreateDecorImage(card, "TierGlow", Vector2.zero, new Vector2(252f, 332f), new Color(1f, 1f, 1f, 0.08f), GetGlowSprite());
    CreateDecorImage(card, "InnerPanel", new Vector2(0f, -14f), new Vector2(196f, 186f), new Color(0f, 0f, 0f, 0.34f), GetRoundedPanelSprite());

    CreateDecorImage(card, "LeftWing", new Vector2(-118f, 8f), new Vector2(70f, 286f), Color.white, GetFeatherSprite());
    CreateDecorImage(card, "RightWing", new Vector2(118f, 8f), new Vector2(70f, 286f), Color.white, GetFeatherSprite());

    CreateDecorImage(card, "TopLeftShard", new Vector2(-58f, 150f), new Vector2(40f, 96f), Color.white, GetFeatherSprite());
    CreateDecorImage(card, "TopRightShard", new Vector2(58f, 150f), new Vector2(40f, 96f), Color.white, GetFeatherSprite());
    CreateDecorImage(card, "TopGem", new Vector2(0f, 168f), new Vector2(40f, 40f), Color.white, GetGemSprite());

    CreateDecorImage(card, "NamePlate", new Vector2(0f, 116f), new Vector2(196f, 46f), Color.white, GetRoundedPanelSprite());

    CreateDecorImage(card, "CornerTL", new Vector2(-118f, 156f), new Vector2(24f, 24f), Color.white, GetGemSprite());
    CreateDecorImage(card, "CornerTR", new Vector2(118f, 156f), new Vector2(24f, 24f), Color.white, GetGemSprite());
    CreateDecorImage(card, "CornerBL", new Vector2(-118f, -148f), new Vector2(20f, 20f), Color.white, GetGemSprite());
    CreateDecorImage(card, "CornerBR", new Vector2(118f, -148f), new Vector2(20f, 20f), Color.white, GetGemSprite());

    CreateDecorImage(card, "BottomOrb", new Vector2(0f, -168f), new Vector2(70f, 70f), Color.white, GetOrbSprite());
    CreateDecorImage(card, "BottomGem", new Vector2(0f, -168f), new Vector2(30f, 30f), Color.white, GetGemSprite());
    CreateDecorImage(card, "BottomLeftClaw", new Vector2(-74f, -162f), new Vector2(52f, 40f), Color.white, GetFeatherSprite());
    CreateDecorImage(card, "BottomRightClaw", new Vector2(74f, -162f), new Vector2(52f, 40f), Color.white, GetFeatherSprite());
  }

  private static Image CreateDecorImage(Transform parent, string name, Vector2 pos, Vector2 size, Color color, Sprite sprite = null)
  {
    var go = new GameObject(name, typeof(RectTransform), typeof(Image));
    go.transform.SetParent(parent, false);
    var rt = go.GetComponent<RectTransform>();
    rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
    rt.anchoredPosition = pos;
    rt.sizeDelta = size;

    var image = go.GetComponent<Image>();
    image.color = color;
    image.sprite = sprite;
    image.raycastTarget = false;
    return image;
  }

  private static void ApplyTierDecoration(Transform card, AugmentTier tier)
  {
    Color tierColor = GameUITheme.GetAugmentTierColor(tier);
    Color bright = GetTierBrightColor(tier);
    Color dark = Color.Lerp(GameUITheme.BgPanel, tierColor, 0.3f);

    Color rail = Color.Lerp(dark, bright, 0.78f);

    SetDecorColor(card, "TierGlow", new Color(bright.r, bright.g, bright.b, 0.18f));
    SetDecorColor(card, "InnerPanel", new Color(0.03f, 0.04f, 0.09f, 0.8f));
    SetDecorColor(card, "LeftWing", rail);
    SetDecorColor(card, "RightWing", rail);
    SetDecorColor(card, "TopLeftShard", bright);
    SetDecorColor(card, "TopRightShard", bright);
    SetDecorColor(card, "TopGem", bright);
    SetDecorColor(card, "BottomOrb", Color.Lerp(GameUITheme.BgDeep, bright, 0.6f));
    SetDecorColor(card, "BottomGem", Color.Lerp(Color.white, bright, 0.5f));
    SetDecorColor(card, "BottomLeftClaw", bright);
    SetDecorColor(card, "BottomRightClaw", bright);
    SetDecorColor(card, "NamePlate", new Color(dark.r * 0.5f, dark.g * 0.5f, dark.b * 0.5f, 0.62f));
    SetDecorColor(card, "CornerTL", bright);
    SetDecorColor(card, "CornerTR", bright);
    SetDecorColor(card, "CornerBL", Color.Lerp(bright, Color.white, 0.3f));
    SetDecorColor(card, "CornerBR", Color.Lerp(bright, Color.white, 0.3f));

    // Wings flare outward; shards angle like horns; bottom claws splay down.
    RotateDecor(card, "LeftWing", 8f);
    RotateDecor(card, "RightWing", -8f);
    FlipDecorX(card, "RightWing");
    RotateDecor(card, "TopLeftShard", tier == AugmentTier.Gold ? -22f : -32f);
    RotateDecor(card, "TopRightShard", tier == AugmentTier.Gold ? 22f : 32f);
    RotateDecor(card, "BottomLeftClaw", -118f);
    RotateDecor(card, "BottomRightClaw", 118f);
  }

  private static void FlipDecorX(Transform card, string name)
  {
    var t = card.Find(name);
    if (t != null)
      t.localScale = new Vector3(-Mathf.Abs(t.localScale.x), t.localScale.y, t.localScale.z);
  }

  private static Sprite _frameGold;
  private static Sprite _frameSilver;
  private static Sprite _framePrismatic;

  // Each tier has its own ornate frame artwork.
  private static Sprite GetTierFrameSprite(AugmentTier tier)
  {
    switch (tier)
    {
      case AugmentTier.Silver:
        return _frameSilver ??= LoadSprite("UI/augment_card_frame_silver");
      case AugmentTier.Prismatic:
        return _framePrismatic ??= LoadSprite("UI/augment_card_frame_prismatic");
      default:
        return _frameGold ??= LoadSprite("UI/augment_card_frame");
    }
  }

  private static Color GetTierBrightColor(AugmentTier tier) =>
    tier switch
    {
      AugmentTier.Gold => GameUITheme.Hex("#FFC857"),
      AugmentTier.Silver => GameUITheme.Hex("#DCE8FF"),
      AugmentTier.Prismatic => GameUITheme.Hex("#B8FFE9"),
      _ => GameUITheme.GetAugmentTierColor(tier)
    };

  private static void SetDecorColor(Transform card, string name, Color color)
  {
    var image = card.Find(name)?.GetComponent<Image>();
    if (image != null)
      image.color = color;
  }

  private static void RotateDecor(Transform card, string name, float z)
  {
    var t = card.Find(name);
    if (t != null)
      t.localEulerAngles = new Vector3(0f, 0f, z);
  }

  private static Sprite _gemSprite;
  private static Sprite _featherSprite;
  private static Sprite _orbSprite;
  private static Sprite _glowSprite;
  private static Sprite _roundedPanelSprite;
  private static Sprite _frameSprite;

  private static Sprite GetGemSprite() => _gemSprite ??= CreateShapeSprite(DrawGem);
  private static Sprite GetFeatherSprite() => _featherSprite ??= CreateShapeSprite(DrawFeather);
  private static Sprite GetOrbSprite() => _orbSprite ??= CreateShapeSprite(DrawOrb);
  private static Sprite GetGlowSprite() => _glowSprite ??= CreateShapeSprite(DrawGlow);
  private static Sprite GetRoundedPanelSprite() => _roundedPanelSprite ??= CreateShapeSprite(DrawRoundedPanel);
  private static Sprite GetFrameSprite() => _frameSprite ??= CreateFrameSprite();

  private static Sprite CreateFrameSprite()
  {
    const int size = 128;
    var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };
    var px = new Color[size * size];
    for (int i = 0; i < px.Length; i++)
      px[i] = new Color(0f, 0f, 0f, 0f);

    const int thickness = 30;
    for (int y = 0; y < size; y++)
    {
      for (int x = 0; x < size; x++)
      {
        int edge = Mathf.Min(Mathf.Min(x, size - 1 - x), Mathf.Min(y, size - 1 - y));
        if (edge >= thickness)
          continue;

        // Beveled metallic profile from the outer edge inward.
        Color c;
        if (edge <= 1)
          c = new Color(0.12f, 0.12f, 0.16f, 1f);          // outer dark outline
        else if (edge <= 6)
          c = new Color(1f, 1f, 1f, 1f);                   // bright outer rim
        else if (edge <= thickness - 8)
        {
          float t = (edge - 6f) / (thickness - 14f);
          float shade = Mathf.Lerp(0.95f, 0.45f, t);       // metallic bevel gradient
          c = new Color(shade, shade, shade, 1f);
        }
        else if (edge <= thickness - 4)
          c = new Color(0.18f, 0.18f, 0.24f, 1f);          // inner dark line
        else
        {
          float a = Mathf.SmoothStep(1f, 0f, (edge - (thickness - 4f)) / 4f);
          c = new Color(0.3f, 0.3f, 0.36f, a);             // fade into recess
        }

        px[y * size + x] = c;
      }
    }

    tex.SetPixels(px);
    tex.Apply();
    return Sprite.Create(tex, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(thickness, thickness, thickness, thickness));
  }

  private static Sprite CreateShapeSprite(Action<Color[], int> draw)
  {
    const int size = 128;
    var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };
    var px = new Color[size * size];
    for (int i = 0; i < px.Length; i++)
      px[i] = new Color(0f, 0f, 0f, 0f);

    draw(px, size);
    tex.SetPixels(px);
    tex.Apply();
    return Sprite.Create(tex, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 100f);
  }

  private static void DrawGem(Color[] px, int size)
  {
    float cx = size * 0.5f;
    float cy = size * 0.5f;
    float halfW = size * 0.30f;
    float halfH = size * 0.46f;
    for (int y = 0; y < size; y++)
    {
      for (int x = 0; x < size; x++)
      {
        float dxn = Mathf.Abs(x - cx) / halfW;
        float dyn = Mathf.Abs(y - cy) / halfH;
        float d = dxn + dyn;
        if (d <= 1f)
        {
          float edge = Mathf.SmoothStep(1f, 0.82f, d);
          float shine = x - cx < 0 && y - cy > 0 ? 1f : 0.82f;
          px[y * size + x] = new Color(shine, shine, shine, edge);
        }
      }
    }
  }

  private static void DrawFeather(Color[] px, int size)
  {
    float cx = size * 0.5f;
    for (int y = 0; y < size; y++)
    {
      float t = y / (float)(size - 1);
      // Pointed at top, tapered at bottom, widest around lower-middle.
      float profile = Mathf.Sin(Mathf.Pow(t, 0.8f) * Mathf.PI);
      float halfW = profile * size * 0.22f * (0.4f + 0.6f * (1f - t));
      if (halfW < 0.5f)
        continue;

      int minX = Mathf.RoundToInt(cx - halfW);
      int maxX = Mathf.RoundToInt(cx + halfW);
      for (int x = minX; x <= maxX; x++)
      {
        if (x < 0 || x >= size)
          continue;
        float edge = 1f - Mathf.Abs(x - cx) / Mathf.Max(1f, halfW);
        float a = Mathf.SmoothStep(0f, 0.35f, edge);
        px[y * size + x] = new Color(1f, 1f, 1f, a);
      }
    }
  }

  private static void DrawOrb(Color[] px, int size)
  {
    float c = size * 0.5f;
    float r = size * 0.46f;
    for (int y = 0; y < size; y++)
    {
      for (int x = 0; x < size; x++)
      {
        float d = Mathf.Sqrt((x - c) * (x - c) + (y - c) * (y - c));
        if (d <= r)
        {
          float edge = Mathf.SmoothStep(r, r * 0.7f, d);
          px[y * size + x] = new Color(1f, 1f, 1f, 0.35f + 0.65f * edge);
        }
      }
    }
  }

  private static void DrawGlow(Color[] px, int size)
  {
    float c = size * 0.5f;
    float r = size * 0.5f;
    for (int y = 0; y < size; y++)
    {
      for (int x = 0; x < size; x++)
      {
        float d = Mathf.Sqrt((x - c) * (x - c) + (y - c) * (y - c)) / r;
        float a = Mathf.Clamp01(1f - d);
        px[y * size + x] = new Color(1f, 1f, 1f, a * a * 0.9f);
      }
    }
  }

  private static void DrawRoundedPanel(Color[] px, int size)
  {
    int radius = Mathf.RoundToInt(size * 0.16f);
    int margin = 4;
    for (int y = 0; y < size; y++)
    {
      for (int x = 0; x < size; x++)
      {
        if (x < margin || x >= size - margin || y < margin || y >= size - margin)
          continue;

        if (InRoundedRect(x, y, margin, size - margin, radius))
          px[y * size + x] = Color.white;
      }
    }
  }

  private static bool InRoundedRect(int x, int y, int min, int max, int radius)
  {
    int innerMin = min + radius;
    int innerMax = max - radius;
    float cx = Mathf.Clamp(x, innerMin, innerMax);
    float cy = Mathf.Clamp(y, innerMin, innerMax);
    float dx = x - cx;
    float dy = y - cy;
    return dx * dx + dy * dy <= radius * radius;
  }

  private static Sprite LoadSprite(string resourcePath)
  {
    var tex = Resources.Load<Texture2D>(resourcePath);
    if (tex == null)
      return null;

    return Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
  }
}

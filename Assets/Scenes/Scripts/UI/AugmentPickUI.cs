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
    _currentChoices = AugmentSelector.PickChoices(
      catalog != null ? catalog.allAugments : Array.Empty<AugmentDefinition>(),
      PlayerAugmentState.Instance,
      3);

    if (_currentChoices.Count == 0)
    {
      Debug.LogWarning("AugmentPickUI: No augments available in catalog.");
      onPicked?.Invoke(null);
      return;
    }

    if (titleText != null)
      titleText.text = "CHOOSE YOUR AUGMENT";

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

      if (_cardNameTexts[i] != null)
      {
        _cardNameTexts[i].text =
          $"{def.displayName}  [{def.tier}]  Lv{previewLevel}/{def.maxLevel}";
      }

      if (_cardBodyTexts[i] != null)
        _cardBodyTexts[i].text = def.GetDescriptionForLevel(previewLevel);

      if (_cardBackgrounds[i] != null)
        _cardBackgrounds[i].color = GetTierColor(def.tier);
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

  private static Color GetTierColor(AugmentTier tier) =>
    tier switch
    {
      AugmentTier.Silver => new Color(0.45f, 0.48f, 0.55f, 0.95f),
      AugmentTier.Gold => new Color(0.55f, 0.42f, 0.15f, 0.95f),
      AugmentTier.Prismatic => new Color(0.42f, 0.22f, 0.62f, 0.95f),
      _ => new Color(0.3f, 0.3f, 0.35f, 0.95f)
    };

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

    panel.GetComponent<Image>().color = new Color(0.06f, 0.07f, 0.11f, 0.94f);

    titleText = CreateTmp(
      panel.transform,
      "AugmentPickTitle",
      "CHOOSE YOUR AUGMENT",
      30,
      new Vector2(0, 250),
      new Vector2(760, 50),
      FontStyles.Bold);

    CreateTmp(
      panel.transform,
      "AugmentPickSubtitle",
      "Pick 1 of 3 — then face the boss",
      16,
      new Vector2(0, 210),
      new Vector2(760, 32),
      FontStyles.Italic);

    float[] xPositions = { -250f, 0f, 250f };
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
    rect.anchoredPosition = new Vector2(xPos, -20f);
    rect.sizeDelta = new Vector2(230f, 300f);

    var bg = card.GetComponent<Image>();
    bg.color = new Color(0.35f, 0.35f, 0.4f, 1f);
    _cardBackgrounds.Add(bg);

    var nameTmp = CreateTmp(
      card.transform,
      "Name",
      "Augment",
      18,
      new Vector2(0, 115),
      new Vector2(210f, 44f),
      FontStyles.Bold);

    var bodyTmp = CreateTmp(
      card.transform,
      "Body",
      "Description",
      13,
      new Vector2(0, -20f),
      new Vector2(210f, 180f),
      alignment: TextAlignmentOptions.TopLeft);

    var pickLabel = CreateTmp(
      card.transform,
      "PickLabel",
      "SELECT",
      14,
      new Vector2(0, -125f),
      new Vector2(120f, 28f),
      FontStyles.Bold);

    pickLabel.color = new Color(0.9f, 0.95f, 1f, 0.9f);

    var button = card.GetComponent<Button>();
    int captured = index;
    button.onClick.AddListener(() => OnCardClicked(captured));

    _cardButtons.Add(button);
    _cardNameTexts.Add(nameTmp);
    _cardBodyTexts.Add(bodyTmp);
  }

  private static TMP_Text CreateTmp(
    Transform parent,
    string name,
    string text,
    int fontSize,
    Vector2 anchoredPos,
    Vector2 size,
    FontStyles fontStyle = FontStyles.Normal,
    TextAlignmentOptions alignment = TextAlignmentOptions.Center)
  {
    var go = new GameObject(name, typeof(RectTransform));
    go.transform.SetParent(parent, false);

    var rect = go.GetComponent<RectTransform>();
    rect.anchorMin = new Vector2(0.5f, 0.5f);
    rect.anchorMax = new Vector2(0.5f, 0.5f);
    rect.anchoredPosition = anchoredPos;
    rect.sizeDelta = size;

    var tmp = go.AddComponent<TextMeshProUGUI>();
    if (TMP_Settings.defaultFontAsset != null)
      tmp.font = TMP_Settings.defaultFontAsset;
    tmp.text = text;
    tmp.fontSize = fontSize;
    tmp.fontStyle = fontStyle;
    tmp.alignment = alignment;
    tmp.color = Color.white;
    tmp.textWrappingMode = TextWrappingModes.Normal;
    return tmp;
  }
}

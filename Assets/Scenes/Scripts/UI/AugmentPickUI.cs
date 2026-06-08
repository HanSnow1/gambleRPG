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
        _cardBodyTexts[i].text = def.GetDescriptionForLevel(previewLevel);

      if (_cardBackgrounds[i] != null)
        _cardBackgrounds[i].color = new Color(
          GameUITheme.GetAugmentTierColor(def.tier).r,
          GameUITheme.GetAugmentTierColor(def.tier).g,
          GameUITheme.GetAugmentTierColor(def.tier).b,
          0.35f);
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

    titleText = GameUITheme.CreateText(
      panel.transform,
      "AugmentPickTitle",
      GameUIText.ChooseAugment,
      32,
      FontStyles.Bold);
    var titleRt = titleText.GetComponent<RectTransform>();
    titleRt.anchorMin = titleRt.anchorMax = new Vector2(0.5f, 0.5f);
    titleRt.anchoredPosition = new Vector2(0, 250);
    titleRt.sizeDelta = new Vector2(760, 50);
    titleText.color = GameUITheme.Accent;

    var subtitle = GameUITheme.CreateText(
      panel.transform,
      "AugmentPickSubtitle",
      GameUIText.AugmentPickSubtitle,
      15,
      FontStyles.Italic);
    var subRt = subtitle.GetComponent<RectTransform>();
    subRt.anchorMin = subRt.anchorMax = new Vector2(0.5f, 0.5f);
    subRt.anchoredPosition = new Vector2(0, 210);
    subRt.sizeDelta = new Vector2(760, 32);
    subtitle.color = GameUITheme.TextMuted;

    float[] xPositions = { -270f, 0f, 270f };
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
    rect.anchoredPosition = new Vector2(xPos, -30f);
    rect.sizeDelta = new Vector2(250f, 320f);

    var bg = card.GetComponent<Image>();
    bg.color = GameUITheme.BgPanelLight;

    var border = new GameObject("Border", typeof(RectTransform), typeof(Image));
    border.transform.SetParent(card.transform, false);
    border.transform.SetAsFirstSibling();
    var borderRt = border.GetComponent<RectTransform>();
    borderRt.anchorMin = Vector2.zero;
    borderRt.anchorMax = Vector2.one;
    borderRt.offsetMin = new Vector2(-2, -2);
    borderRt.offsetMax = new Vector2(2, 2);
    border.GetComponent<Image>().color = GameUITheme.Border;
    _cardBackgrounds.Add(bg);

    var nameTmp = GameUITheme.CreateText(card.transform, "Name", "증강", 17, FontStyles.Bold);
    var nameRt = nameTmp.GetComponent<RectTransform>();
    nameRt.anchoredPosition = new Vector2(0, 120);
    nameRt.sizeDelta = new Vector2(220f, 50f);

    var bodyTmp = GameUITheme.CreateText(
      card.transform,
      "Body",
      "설명",
      13,
      alignment: TextAlignmentOptions.TopLeft);
    var bodyRt = bodyTmp.GetComponent<RectTransform>();
    bodyRt.anchoredPosition = new Vector2(0, -10f);
    bodyRt.sizeDelta = new Vector2(220f, 190f);
    bodyTmp.color = GameUITheme.TextMuted;

    var pickLabel = GameUITheme.CreateText(card.transform, "PickLabel", GameUIText.SelectAugment, 14, FontStyles.Bold);
    var pickRt = pickLabel.GetComponent<RectTransform>();
    pickRt.anchoredPosition = new Vector2(0, -130f);
    pickRt.sizeDelta = new Vector2(140f, 28f);
    pickLabel.color = GameUITheme.Accent;

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
}

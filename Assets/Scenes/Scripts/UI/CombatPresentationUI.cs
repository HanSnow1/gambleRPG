using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Staged dice/card duel presentation before HP changes.</summary>
public class CombatPresentationUI : MonoBehaviour
{
  [SerializeField] private float betRevealSeconds = 1.15f;
  [SerializeField] private float singleRollSeconds = 1.35f;
  [SerializeField] private float compareHoldSeconds = 1.05f;
  [SerializeField] private float damageHoldSeconds = 1.15f;
  [SerializeField] private float rollTickSeconds = 0.08f;
  [SerializeField] private float augmentFxSeconds = 1.05f;

  private GameObject _overlayRoot;
  private TMP_Text _phaseText;
  private TMP_Text _playerBetText;
  private TMP_Text _enemyBetText;
  private TMP_Text _playerLabelText;
  private TMP_Text _enemyLabelText;
  private TMP_Text _playerValueText;
  private TMP_Text _enemyValueText;
  private TMP_Text _resultText;
  private TMP_Text _damageText;
  private Image _playerDiceBg;
  private Image _enemyDiceBg;
  private Image _playerCardBg;
  private Image _enemyCardBg;
  private GameObject _augmentFxPanel;
  private TMP_Text _augmentFxTitle;
  private TMP_Text _augmentFxSubtitle;
  private Image _augmentFxGlow;

  public static CombatPresentationUI Ensure(Canvas canvas)
  {
    if (canvas == null)
      return null;

    var existing = canvas.GetComponentInChildren<CombatPresentationUI>(true);
    if (existing != null)
      return existing;

    var go = new GameObject("CombatPresentationUI", typeof(RectTransform), typeof(CombatPresentationUI));
    go.transform.SetParent(canvas.transform, false);
    var ui = go.GetComponent<CombatPresentationUI>();
    ui.BuildUi();
    return ui;
  }

  public IEnumerator PlayTurn(CombatPresentationTheme theme, CombatTurnResult result, string enemyName)
  {
    if (_overlayRoot == null)
      BuildUi();

    transform.SetAsLastSibling();
    _overlayRoot.SetActive(true);
    _overlayRoot.transform.SetAsLastSibling();

    if (theme == CombatPresentationTheme.Dice)
      yield return PlayDiceDuel(result, enemyName);
    else if (theme == CombatPresentationTheme.Card)
      yield return PlayCardDuel(result, enemyName);
    else
      yield return PlayDiceDuel(result, enemyName);

    _overlayRoot.SetActive(false);
  }

  public IEnumerator PlayPostApplyFx(IReadOnlyList<AugmentFxEvent> events)
  {
    if (events == null || events.Count == 0)
      yield break;

    if (_overlayRoot == null)
      BuildUi();

    transform.SetAsLastSibling();
    _overlayRoot.SetActive(true);
    yield return PlayAugmentFxSequence(events);
    _overlayRoot.SetActive(false);
  }

  private IEnumerator PlayDiceDuel(CombatTurnResult result, string enemyName)
  {
    SetDiceMode();
    ResetHighlights();
    _playerBetText.text = GameUIText.BetLineYou(result.option.label, result.betHpCost);
    _enemyBetText.text = GameUIText.BetLineEnemy(enemyName, result.enemyOption.label, result.enemyOption.betHp);
    _playerValueText.text = "?";
    _enemyValueText.text = "?";
    _resultText.text = "";
    _damageText.text = "";
    _phaseText.text = GameUIText.PhaseYouBet;

    yield return new WaitForSeconds(betRevealSeconds);
    _phaseText.text = GameUIText.PhaseEnemyBet(enemyName);
    yield return new WaitForSeconds(betRevealSeconds);

    _phaseText.text = GameUIText.PhaseYourDice;
    yield return AnimateSingleRoll(_playerValueText, _playerDiceBg, result.playerDiceRoll, result.playerRollCursed);
    yield return PlayAugmentFxSequence(result.rollFxEvents);

    _phaseText.text = GameUIText.PhaseEnemyDice(enemyName);
    yield return AnimateSingleRoll(_enemyValueText, _enemyDiceBg, result.enemyDiceRoll, result.enemyRollCursed);

    _phaseText.text = GameUIText.PhaseCompareDice;
    string playerLine = $"{result.playerCompareValue}" + (result.playerRollCursed ? " ☠" : "");
    string enemyLine = $"{result.enemyCompareValue}" + (result.enemyRollCursed ? " ☠" : "");
    _playerValueText.text = playerLine;
    _enemyValueText.text = enemyLine;

    if (result.duelWinner == DuelWinner.Player)
    {
      _playerDiceBg.color = new Color(0.2f, 0.65f, 0.35f, 0.95f);
      _resultText.text = GameUIText.YouWinRoll;
    }
    else if (result.duelWinner == DuelWinner.Enemy)
    {
      _enemyDiceBg.color = new Color(0.75f, 0.25f, 0.25f, 0.95f);
      _resultText.text = GameUIText.EnemyWinRoll(enemyName);
    }
    else
    {
      _resultText.text = GameUIText.TieChip;
    }

    _resultText.richText = true;
    yield return new WaitForSeconds(compareHoldSeconds);

    _phaseText.text = GameUIText.PhaseDamage;
    _damageText.text = BuildDuelDamageLine(result, enemyName);
    _damageText.richText = true;
    yield return new WaitForSeconds(damageHoldSeconds * 0.55f);
    yield return PlayAugmentFxSequence(result.damageFxEvents);
    yield return new WaitForSeconds(damageHoldSeconds * 0.45f);
  }

  private IEnumerator PlayCardDuel(CombatTurnResult result, string enemyName)
  {
    SetCardMode();
    ResetHighlights();
    _playerBetText.text = GameUIText.BetLineYou(result.option.label, result.betHpCost);
    _enemyBetText.text = GameUIText.BetLineEnemy(enemyName, result.enemyOption.label, result.enemyOption.betHp);
    _playerValueText.text = "???";
    _enemyValueText.text = "???";
    _resultText.text = "";
    _damageText.text = "";

    _phaseText.text = GameUIText.PhaseYouBet;
    yield return new WaitForSeconds(betRevealSeconds);
    _phaseText.text = GameUIText.PhaseEnemyBet(enemyName);
    yield return new WaitForSeconds(betRevealSeconds);

    _phaseText.text = GameUIText.PhaseDrawYourCard;
    yield return AnimateCardFlip(_playerValueText, _playerCardBg, result.playerCard);
    yield return PlayAugmentFxSequence(result.rollFxEvents);
    _phaseText.text = GameUIText.PhaseEnemyCard(enemyName);
    yield return AnimateCardFlip(_enemyValueText, _enemyCardBg, result.enemyCard);

    _phaseText.text = GameUIText.PhaseCompareCards(result.playerCardValue, result.enemyCardValue);
    if (result.duelWinner == DuelWinner.Player)
    {
      _playerCardBg.color = new Color(0.2f, 0.65f, 0.35f, 0.95f);
      _resultText.text = GameUIText.YourCardWins;
    }
    else if (result.duelWinner == DuelWinner.Enemy)
    {
      _enemyCardBg.color = new Color(0.75f, 0.25f, 0.25f, 0.95f);
      _resultText.text = GameUIText.EnemyCardWins;
    }
    else
      _resultText.text = GameUIText.TieChip;

    _resultText.richText = true;
    yield return new WaitForSeconds(compareHoldSeconds);

    _phaseText.text = GameUIText.PhaseDamage;
    _damageText.text = BuildDuelDamageLine(result, enemyName);
    _damageText.richText = true;
    yield return new WaitForSeconds(damageHoldSeconds * 0.55f);
    yield return PlayAugmentFxSequence(result.damageFxEvents);
    yield return new WaitForSeconds(damageHoldSeconds * 0.45f);
  }

  private IEnumerator PlayAugmentFxSequence(IReadOnlyList<AugmentFxEvent> events)
  {
    if (events == null || events.Count == 0 || _augmentFxPanel == null)
      yield break;

    foreach (var fx in events)
    {
      if (string.IsNullOrEmpty(fx.title))
        continue;

      _augmentFxPanel.SetActive(true);
      _augmentFxTitle.text = $"✦  {fx.title}  ✦";
      _augmentFxSubtitle.text = fx.subtitle ?? "";
      _augmentFxGlow.color = new Color(fx.accent.r, fx.accent.g, fx.accent.b, 0.55f);
      _augmentFxTitle.color = fx.accent;

      if (fx.kind == AugmentFxKind.ExtraPip)
        PulseTarget(_playerDiceBg, fx.accent);
      else if (fx.kind == AugmentFxKind.RevealedCup)
        PulseTarget(_playerCardBg, fx.accent);

      yield return new WaitForSeconds(augmentFxSeconds);
      _augmentFxPanel.SetActive(false);
    }
  }

  private void PulseTarget(Image target, Color accent)
  {
    if (target == null || !target.gameObject.activeInHierarchy)
      return;

    target.color = accent;
  }

  private IEnumerator AnimateSingleRoll(TMP_Text valueText, Image bg, int finalValue, bool cursed)
  {
    float elapsed = 0f;
    while (elapsed < singleRollSeconds)
    {
      valueText.text = Random.Range(1, 7).ToString();
      bg.color = GameUITheme.BgPanelLight;
      elapsed += rollTickSeconds;
      yield return new WaitForSeconds(rollTickSeconds);
    }

    valueText.text = finalValue.ToString() + (cursed ? "\n☠" : "");
    bg.color = cursed ? new Color(0.45f, 0.15f, 0.2f, 0.95f) : GameUITheme.Accent;
  }

  private IEnumerator AnimateCardFlip(TMP_Text valueText, Image bg, string finalCard)
  {
    float elapsed = 0f;
    while (elapsed < singleRollSeconds * 0.7f)
    {
      valueText.text = "???";
      bg.color = GameUITheme.Border;
      elapsed += rollTickSeconds;
      yield return new WaitForSeconds(rollTickSeconds);
    }

    valueText.text = finalCard ?? "?";
    bg.color = GameUITheme.BgPanelLight;
    yield return new WaitForSeconds(singleRollSeconds * 0.3f);
  }

  private static string BuildDuelDamageLine(CombatTurnResult result, string enemyName)
  {
    if (result.duelWinner == DuelWinner.Player)
      return GameUIText.DamageLinePlayerWin(result.betHpCost, result.damageToEnemy);

    if (result.duelWinner == DuelWinner.Enemy)
      return GameUIText.DamageLineEnemyWin(enemyName, result.enemyOption.betHp, result.damageToPlayer);

    return GameUIText.DamageLineTie(result.chipToPlayer, result.chipToEnemy, enemyName);
  }

  private void SetDiceMode()
  {
    _playerDiceBg.gameObject.SetActive(true);
    _enemyDiceBg.gameObject.SetActive(true);
    _playerCardBg.gameObject.SetActive(false);
    _enemyCardBg.gameObject.SetActive(false);
    _playerValueText.fontSize = 64;
    _enemyValueText.fontSize = 64;
  }

  private void SetCardMode()
  {
    _playerDiceBg.gameObject.SetActive(false);
    _enemyDiceBg.gameObject.SetActive(false);
    _playerCardBg.gameObject.SetActive(true);
    _enemyCardBg.gameObject.SetActive(true);
    _playerValueText.fontSize = 40;
    _enemyValueText.fontSize = 40;
  }

  private void ResetHighlights()
  {
    _playerDiceBg.color = GameUITheme.BgPanelLight;
    _enemyDiceBg.color = GameUITheme.BgPanelLight;
    _playerCardBg.color = GameUITheme.Border;
    _enemyCardBg.color = GameUITheme.Border;
  }

  private void BuildUi()
  {
    var canvas = GetComponentInParent<Canvas>() ?? FindFirstObjectByType<Canvas>();
    if (canvas != null && transform.parent != canvas.transform)
      transform.SetParent(canvas.transform, false);

    var rootRt = gameObject.GetComponent<RectTransform>() ?? gameObject.AddComponent<RectTransform>();
    rootRt.anchorMin = Vector2.zero;
    rootRt.anchorMax = Vector2.one;
    rootRt.offsetMin = Vector2.zero;
    rootRt.offsetMax = Vector2.zero;

    _overlayRoot = new GameObject("PresentationOverlay", typeof(RectTransform), typeof(Image));
    _overlayRoot.transform.SetParent(transform, false);
    var overlayRt = _overlayRoot.GetComponent<RectTransform>();
    overlayRt.anchorMin = Vector2.zero;
    overlayRt.anchorMax = Vector2.one;
    overlayRt.offsetMin = Vector2.zero;
    overlayRt.offsetMax = Vector2.zero;
    _overlayRoot.GetComponent<Image>().color = new Color(0.02f, 0.03f, 0.08f, 0.88f);

    var box = new GameObject("PresentationBox", typeof(RectTransform), typeof(Image));
    box.transform.SetParent(_overlayRoot.transform, false);
    var boxRt = box.GetComponent<RectTransform>();
    boxRt.anchorMin = boxRt.anchorMax = new Vector2(0.5f, 0.5f);
    boxRt.anchoredPosition = new Vector2(0, 24);
    boxRt.sizeDelta = new Vector2(760, 420);
    box.GetComponent<Image>().color = GameUITheme.BgPanel;

    _phaseText = CreateText(box.transform, "PhaseText", "...", 22, FontStyles.Bold, new Vector2(0, 175), new Vector2(700, 40));
    _phaseText.color = GameUITheme.Accent;

    _playerBetText = CreateText(box.transform, "PlayerBetText", "", 16, FontStyles.Normal, new Vector2(-180, 130), new Vector2(320, 28));
    _enemyBetText = CreateText(box.transform, "EnemyBetText", "", 16, FontStyles.Normal, new Vector2(180, 130), new Vector2(320, 28));
    _playerBetText.alignment = TextAlignmentOptions.MidlineLeft;
    _enemyBetText.alignment = TextAlignmentOptions.MidlineRight;
    _playerBetText.richText = _enemyBetText.richText = true;

    _playerLabelText = CreateText(box.transform, "PlayerLabel", GameUIText.You, 15, FontStyles.Bold, new Vector2(-190, 78), new Vector2(160, 24));
    _enemyLabelText = CreateText(box.transform, "EnemyLabel", GameUIText.Enemy, 15, FontStyles.Bold, new Vector2(190, 78), new Vector2(160, 24));
    _playerLabelText.color = GameUITheme.TextMuted;
    _enemyLabelText.color = GameUITheme.TextMuted;

    _playerDiceBg = CreateDiceBg(box.transform, "PlayerDiceBg", new Vector2(-190, -20));
    _enemyDiceBg = CreateDiceBg(box.transform, "EnemyDiceBg", new Vector2(190, -20));
    _playerCardBg = CreateCardBg(box.transform, "PlayerCardBg", new Vector2(-190, -20));
    _enemyCardBg = CreateCardBg(box.transform, "EnemyCardBg", new Vector2(190, -20));

    _playerValueText = CreateText(box.transform, "PlayerValue", "?", 64, FontStyles.Bold, new Vector2(-190, -20), new Vector2(160, 160));
    _enemyValueText = CreateText(box.transform, "EnemyValue", "?", 64, FontStyles.Bold, new Vector2(190, -20), new Vector2(160, 160));

    _resultText = CreateText(box.transform, "ResultText", "", 30, FontStyles.Bold, new Vector2(0, -120), new Vector2(700, 44));
    _damageText = CreateText(box.transform, "DamageText", "", 18, FontStyles.Normal, new Vector2(0, -168), new Vector2(700, 52));

    _augmentFxPanel = new GameObject("AugmentFxPanel", typeof(RectTransform), typeof(Image));
    _augmentFxPanel.transform.SetParent(box.transform, false);
    var fxRt = _augmentFxPanel.GetComponent<RectTransform>();
    fxRt.anchorMin = fxRt.anchorMax = new Vector2(0.5f, 0.5f);
    fxRt.anchoredPosition = new Vector2(0, -30);
    fxRt.sizeDelta = new Vector2(520, 110);
    _augmentFxPanel.GetComponent<Image>().color = new Color(0.08f, 0.12f, 0.22f, 0.94f);

    var glowGo = new GameObject("AugmentFxGlow", typeof(RectTransform), typeof(Image));
    glowGo.transform.SetParent(_augmentFxPanel.transform, false);
    _augmentFxGlow = glowGo.GetComponent<Image>();
    var glowRt = _augmentFxGlow.GetComponent<RectTransform>();
    glowRt.anchorMin = Vector2.zero;
    glowRt.anchorMax = Vector2.one;
    glowRt.offsetMin = new Vector2(-4, -4);
    glowRt.offsetMax = new Vector2(4, 4);
    _augmentFxGlow.transform.SetAsFirstSibling();
    _augmentFxGlow.color = new Color(GameUITheme.Accent.r, GameUITheme.Accent.g, GameUITheme.Accent.b, 0.45f);

    _augmentFxTitle = CreateText(_augmentFxPanel.transform, "AugmentFxTitle", "", 24, FontStyles.Bold, new Vector2(0, 18), new Vector2(480, 36));
    _augmentFxSubtitle = CreateText(_augmentFxPanel.transform, "AugmentFxSubtitle", "", 14, FontStyles.Italic, new Vector2(0, -22), new Vector2(480, 48));
    _augmentFxSubtitle.color = GameUITheme.TextPrimary;
    _augmentFxPanel.SetActive(false);

    _overlayRoot.SetActive(false);
  }

  private static Image CreateDiceBg(Transform parent, string name, Vector2 pos)
  {
    var go = new GameObject(name, typeof(RectTransform), typeof(Image));
    go.transform.SetParent(parent, false);
    var rt = go.GetComponent<RectTransform>();
    rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
    rt.anchoredPosition = pos;
    rt.sizeDelta = new Vector2(170, 170);
    go.GetComponent<Image>().color = GameUITheme.BgPanelLight;
    return go.GetComponent<Image>();
  }

  private static Image CreateCardBg(Transform parent, string name, Vector2 pos)
  {
    var go = new GameObject(name, typeof(RectTransform), typeof(Image));
    go.transform.SetParent(parent, false);
    var rt = go.GetComponent<RectTransform>();
    rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
    rt.anchoredPosition = pos;
    rt.sizeDelta = new Vector2(150, 200);
    go.GetComponent<Image>().color = GameUITheme.Border;
    return go.GetComponent<Image>();
  }

  private static TMP_Text CreateText(
    Transform parent,
    string name,
    string text,
    int fontSize,
    FontStyles style,
    Vector2 pos,
    Vector2 size)
  {
    var tmp = GameUITheme.CreateText(parent, name, text, fontSize, style);
    var rt = tmp.GetComponent<RectTransform>();
    rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
    rt.anchoredPosition = pos;
    rt.sizeDelta = size;
    tmp.alignment = TextAlignmentOptions.Center;
    tmp.richText = true;
    return tmp;
  }
}

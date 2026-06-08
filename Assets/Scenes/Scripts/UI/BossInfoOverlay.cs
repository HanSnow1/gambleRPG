using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BossInfoOverlay : MonoBehaviour
{
  private GameObject panel;
  private TMP_Text diceText;
  private TMP_Text cardText;

  public void Show(BossDefinition diceBoss, BossDefinition cardBoss)
  {
    EnsurePanel();
    if (panel == null)
      return;

    diceText.text = FormatBoss("다이스 보스", diceBoss);
    cardText.text = FormatBoss("카드 보스", cardBoss);
    panel.transform.SetAsLastSibling();
    panel.SetActive(true);
  }

  public void Hide()
  {
    if (panel != null)
      panel.SetActive(false);
  }

  private void EnsurePanel()
  {
    if (panel != null)
      return;

    var canvas = GetComponent<Canvas>() ?? FindFirstObjectByType<Canvas>();
    if (canvas == null)
      return;

    panel = new GameObject("BossInfoOverlay", typeof(RectTransform), typeof(Image));
    panel.transform.SetParent(canvas.transform, false);
    var rt = panel.GetComponent<RectTransform>();
    rt.anchorMin = Vector2.zero;
    rt.anchorMax = Vector2.one;
    rt.offsetMin = Vector2.zero;
    rt.offsetMax = Vector2.zero;

    var img = panel.GetComponent<Image>();
    img.color = new Color(GameUITheme.BgPanel.r, GameUITheme.BgPanel.g, GameUITheme.BgPanel.b, 0.92f);

    var title = GameUITheme.CreateText(panel.transform, "BossInfoTitle", "보스 정보", 34, FontStyles.Bold);
    var titleRt = title.GetComponent<RectTransform>();
    titleRt.anchorMin = titleRt.anchorMax = new Vector2(0.5f, 1f);
    titleRt.pivot = new Vector2(0.5f, 1f);
    titleRt.anchoredPosition = new Vector2(0f, -36f);
    titleRt.sizeDelta = new Vector2(640f, 52f);
    title.color = GameUITheme.Accent;

    diceText = CreateBox(panel.transform, "DiceInfo", new Vector2(-250f, -10f));
    cardText = CreateBox(panel.transform, "CardInfo", new Vector2(250f, -10f));

    GameUITheme.CreateButton(panel.transform, "BossInfoCloseButton", "닫기", new Vector2(0f, -280f), new Vector2(220f, 44f), GameUITheme.Border, Hide);
    panel.SetActive(false);
  }

  private static TMP_Text CreateBox(Transform parent, string name, Vector2 pos)
  {
    var box = new GameObject(name, typeof(RectTransform), typeof(Image));
    box.transform.SetParent(parent, false);
    var boxRt = box.GetComponent<RectTransform>();
    boxRt.anchorMin = boxRt.anchorMax = new Vector2(0.5f, 0.5f);
    boxRt.anchoredPosition = pos;
    boxRt.sizeDelta = new Vector2(430f, 430f);
    GameUITheme.ApplyRoundedFrameStyle(box.GetComponent<Image>(), new Color(GameUITheme.BgPanelLight.r, GameUITheme.BgPanelLight.g, GameUITheme.BgPanelLight.b, 0.9f));

    var text = GameUITheme.CreateText(box.transform, "Body", "", 16, alignment: TextAlignmentOptions.TopLeft);
    var txtRt = text.GetComponent<RectTransform>();
    txtRt.anchorMin = Vector2.zero;
    txtRt.anchorMax = Vector2.one;
    txtRt.offsetMin = new Vector2(22f, 22f);
    txtRt.offsetMax = new Vector2(-22f, -22f);
    return text;
  }

  private static string FormatBoss(string title, BossDefinition boss)
  {
    if (boss == null)
      return $"{title}\n\n정보 없음";

    return
      $"<b>{title}</b>\n" +
      $"{boss.displayName}\n\n" +
      $"HP: {boss.maxHp}\n" +
      $"{BossGambleResolvers.GetRuleLabel(boss)}\n\n" +
      $"약점: {boss.weaknessHint}";
  }
}

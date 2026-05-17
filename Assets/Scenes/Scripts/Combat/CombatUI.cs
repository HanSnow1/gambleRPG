using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CombatUI : MonoBehaviour
{
  [SerializeField] private BettingCombatSystem combat;
  [SerializeField] private TMP_Text playerHpText;
  [SerializeField] private TMP_Text enemyHpText;
  [SerializeField] private TMP_Text combatLogText;

  private void Awake()
  {
    if (combat == null)
      combat = FindFirstObjectByType<BettingCombatSystem>();

    combat.OnCombatLog += OnLog;
    combat.ResetCombat();
    Refresh();
  }

  public void OnBetSmall() => DoBet(10, 0.60f, 20, 6);
  public void OnBetMedium() => DoBet(18, 0.45f, 46, 12);
  public void OnBetLarge() => DoBet(25, 0.30f, 72, 20);

  private void DoBet(int bet, float chance, int dmg, int failExtra)
  {
    if (combat.ResolveBetTurn(bet, chance, dmg, failExtra))
      Refresh();
  }

  private void OnLog(string msg)
  {
    combatLogText.text = msg + "\n" + combatLogText.text;
    Refresh();
  }

  private void Refresh()
  {
    var s = combat.State;
    playerHpText.text = $"Player HP: {s.playerHp} / {s.playerMaxHp}";
    enemyHpText.text = $"Enemy HP: {s.enemyHp}";
  }
}
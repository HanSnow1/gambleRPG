using System;
using UnityEngine;

public class BettingCombatSystem : MonoBehaviour
{
  [Serializable]
  public class CombatState
  {
    public int playerHp = 100;
    public int playerMaxHp = 100;
    public int enemyHp = 80;
    public bool IsPlayerDead => playerHp <= 0;
    public bool IsEnemyDead => enemyHp <= 0;
  }

  [SerializeField] private int startingEnemyHp = 80;

  public BossDefinition CurrentBoss { get; private set; }
  public CombatState State { get; } = new CombatState();
  public bool IsCombatActive { get; private set; }

  public event Action<string> OnCombatLog;
  public event Action OnCombatStateChanged;

  public void ResetCombat(BossDefinition boss = null)
  {
    CurrentBoss = boss;
    State.playerHp = State.playerMaxHp;
    State.enemyHp = boss != null ? boss.maxHp : startingEnemyHp;
    IsCombatActive = true;

    if (boss != null)
      Log($"Combat started vs {boss.displayName} ({boss.gambleType})");
    else
      Log("Combat started!");

    NotifyStateChanged();
  }

  public bool ResolveBetTurn(int betHp, float successChance, int damageOnSuccess, int extraFailDamage)
  {
    if (!IsCombatActive)
    {
      Log("Combat already ended.");
      return false;
    }

    betHp = Mathf.Clamp(betHp, 1, State.playerHp);
    State.playerHp -= betHp;

    bool success = UnityEngine.Random.value <= successChance;
    if (success)
    {
      State.enemyHp = Mathf.Max(0, State.enemyHp - damageOnSuccess);
      Log($"Success! Bet {betHp}, damage {damageOnSuccess} (chance {successChance:P0})");
    }
    else
    {
      State.playerHp = Mathf.Max(0, State.playerHp - extraFailDamage);
      Log($"Fail! Bet {betHp} + extra {extraFailDamage} (chance {successChance:P0})");
    }

    if (State.IsEnemyDead)
      Log("Enemy defeated!");
    if (State.IsPlayerDead)
      Log("Game Over - HP 0");

    if (State.IsPlayerDead || State.IsEnemyDead)
      IsCombatActive = false;

    NotifyStateChanged();
    return true;
  }

  public void LogMessage(string msg) => OnCombatLog?.Invoke(msg);

  private void Log(string msg) => LogMessage(msg);

  private void NotifyStateChanged() => OnCombatStateChanged?.Invoke();
}

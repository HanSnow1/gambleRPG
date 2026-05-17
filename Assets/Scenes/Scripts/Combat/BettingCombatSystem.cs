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

  public CombatState State { get; } = new CombatState();
  public event Action<string> OnCombatLog;

  public void ResetCombat()
  {
    State.playerHp = State.playerMaxHp;
    State.enemyHp = 80;
    Log("Combat started!");
  }

  public bool ResolveBetTurn(int betHp, float successChance, int damageOnSuccess, int extraFailDamage)
  {
    if (State.IsPlayerDead || State.IsEnemyDead)
    {
      Log("Combat already ended.");
      return false;
    }

    betHp = Mathf.Clamp(betHp, 1, State.playerHp);
    State.playerHp -= betHp;

    bool success = UnityEngine.Random.value <= successChance;
    if (success)
    {
      State.enemyHp -= damageOnSuccess;
      Log($"Success! Bet {betHp}, damage {damageOnSuccess} (chance {successChance:P0})");
    }
    else
    {
      State.playerHp -= extraFailDamage;
      Log($"Fail! Bet {betHp} + extra {extraFailDamage} (chance {successChance:P0})");
    }

    if (State.IsEnemyDead)
      Log("Enemy defeated!");
    if (State.IsPlayerDead)
      Log("Game Over - HP 0");

    return true;
  }

  private void Log(string msg) => OnCombatLog?.Invoke(msg);
}
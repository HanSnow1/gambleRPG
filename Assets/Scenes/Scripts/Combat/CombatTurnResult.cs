using System.Collections.Generic;

public enum DuelWinner
{
  None,
  Player,
  Enemy,
  Tie
}

public struct CombatTurnResult
{
  public bool isDuelRound;
  public DuelWinner duelWinner;
  public bool isPlayerAttacker;
  public CombatBetOption option;
  public CombatBetOption enemyOption;
  public float successChance;
  public bool success;
  public int betHpCost;
  public int damageToEnemy;
  public int damageToPlayer;
  public int chipToPlayer;
  public int chipToEnemy;
  public List<string> logLines;
  public string patternRollLog;

  public int playerDiceRoll;
  public int enemyDiceRoll;
  public int playerCompareValue;
  public int enemyCompareValue;
  public bool playerRollCursed;
  public bool enemyRollCursed;
  public int diceNeed;
  public string playerCard;
  public string enemyCard;
  public int playerCardValue;
  public int enemyCardValue;
  public List<AugmentFxEvent> augmentFxEvents;
  public List<AugmentFxEvent> rollFxEvents;
  public List<AugmentFxEvent> damageFxEvents;
}

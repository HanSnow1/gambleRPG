using UnityEngine;

public enum CombatPresentationTheme
{
  Dice,
  Card,
  Generic
}

public static class CombatPresentationThemeResolver
{
  public static CombatPresentationTheme Resolve(BossDefinition boss)
  {
    if (boss != null)
    {
      return boss.gambleType switch
      {
        BossGambleType.Dice => CombatPresentationTheme.Dice,
        BossGambleType.Card => CombatPresentationTheme.Card,
        BossGambleType.Shell => CombatPresentationTheme.Card,
        _ => CombatPresentationTheme.Generic
      };
    }

    if (RunManager.Instance != null && RunManager.Instance.State.isActive)
      return RunManager.Instance.State.currentFloor == 1
        ? CombatPresentationTheme.Dice
        : CombatPresentationTheme.Card;

    return CombatPresentationTheme.Dice;
  }

  public static bool UsesDuelRules(BossDefinition boss) =>
    Resolve(boss) != CombatPresentationTheme.Generic;

  public static int RollD6() => Random.Range(1, 7);

  public static int RollCardValue(bool favorHigh)
  {
    if (favorHigh)
      return Random.Range(9, 15);

    return Random.Range(2, 8);
  }

  public static string FormatCard(int value)
  {
    string suit = value switch
    {
      >= 14 => "A",
      13 => "K",
      12 => "Q",
      11 => "J",
      _ => value.ToString()
    };

    string color = value % 2 == 0 ? "♥" : "♠";
    return $"{suit}{color}";
  }
}

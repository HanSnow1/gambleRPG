using System.Collections.Generic;
using UnityEngine;

/// <summary>Mutable boss-pattern state for minigame boards (dice faces, queen mark, joker).</summary>
public class BossPatternRuntimeState
{
  public BossPatternDefinition Pattern { get; private set; }
  public BossDefinition Boss { get; private set; }

  public bool PhaseEnraged { get; private set; }
  public int PlayerTurnCount { get; private set; }

  // Dice Tyrant
  public readonly HashSet<int> CursedFaces = new();
  public int LastDiceRoll { get; private set; }

  // Queen Of Cards
  public int MarkedBetIndex { get; private set; } = 1;
  public bool JokerConsumed { get; private set; }
  public bool SecondJokerActive { get; private set; }
  public bool SecondJokerConsumed { get; private set; }

  public void Initialize(BossDefinition boss, BossPatternDefinition pattern)
  {
    Boss = boss;
    Pattern = pattern;
    PhaseEnraged = false;
    PlayerTurnCount = 0;
    CursedFaces.Clear();
    LastDiceRoll = 0;
    MarkedBetIndex = 1;
    JokerConsumed = false;
    SecondJokerActive = false;
    SecondJokerConsumed = false;

    if (pattern == null || boss == null)
      return;

    SetupMinigameState();
  }

  private void SetupMinigameState()
  {
    switch (Pattern.minigameType)
    {
      case BossMinigameType.DiceFaces when Pattern.patternId == BossPatternId.A:
        CursedFaces.Add(1);
        CursedFaces.Add(2);
        break;

      case BossMinigameType.DiceFaces when Pattern.patternId == BossPatternId.B:
        if (Random.value < 0.5f)
          CursedFaces.Add(6);
        else
          CursedFaces.Add(1);
        break;
    }
  }

  public void OnPlayerTurnStarted()
  {
    PlayerTurnCount++;
    if (Pattern?.minigameType == BossMinigameType.CardTable &&
        Pattern.patternId == BossPatternId.A &&
        PlayerTurnCount > 1 &&
        (PlayerTurnCount - 1) % 3 == 0)
    {
      MarkedBetIndex = (MarkedBetIndex + 1) % 3;
    }
  }

  public void SetLastDiceRoll(int roll) => LastDiceRoll = roll;

  public void ConsumeJoker() => JokerConsumed = true;

  public void ConsumeSecondJoker() => SecondJokerConsumed = true;

  public bool TryEnrageAtHalfHp(int enemyHp, int enemyMaxHp)
  {
    if (PhaseEnraged || enemyMaxHp <= 0 || Pattern == null)
      return false;

    if (enemyHp > enemyMaxHp / 2)
      return false;

    PhaseEnraged = true;
    ApplyEnrageEffects();
    return true;
  }

  private void ApplyEnrageEffects()
  {
    if (Pattern.minigameType == BossMinigameType.DiceFaces &&
        Pattern.patternId == BossPatternId.B &&
        !CursedFaces.Contains(1))
      CursedFaces.Add(1);

    if (Pattern.minigameType == BossMinigameType.CardTable &&
        Pattern.patternId == BossPatternId.B)
      SecondJokerActive = true;
  }

  public static int BetLabelToIndex(string label)
  {
    if (label == "Safe")
      return 0;
    if (label == "Risk")
      return 1;
    return 2;
  }

  public static string BetIndexToLabel(int index) =>
    index switch
    {
      0 => "Safe",
      1 => "Risk",
      _ => "All-in"
    };

  public bool IsBetMarked(string label) =>
    Pattern?.minigameType == BossMinigameType.CardTable &&
    Pattern.patternId == BossPatternId.A &&
    BetLabelToIndex(label) == MarkedBetIndex;
}

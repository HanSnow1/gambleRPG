using UnityEngine;

public class QueenCardBoard : BossGambleBoard
{
  public override float GetPlayerSuccessModifier(CombatBetOption option) => 0f;

  public override float ResolvePlayerRoll(CombatBetOption option, out string logLine)
  {
    logLine = null;
    if (State?.Pattern == null)
      return 0f;

    if (State.Pattern.patternId == BossPatternId.A && State.IsBetMarked(option.label))
      logLine = GameUIText.QueensMarkOn(option.label);

    if (State.Pattern.patternId == BossPatternId.B && !State.JokerConsumed)
      logLine = GameUIText.CardsShuffledJoker;

    Refresh();
    return 0f;
  }

  public override void OnPlayerTurnStarted()
  {
    base.OnPlayerTurnStarted();
    Refresh();
  }

  public override void OnPlayerMiss(CombatBetOption option)
  {
    if (State?.Pattern?.patternId != BossPatternId.B || State.JokerConsumed)
      return;

    State.ConsumeJoker();
    Refresh();
  }

  public override void OnPhaseEnraged(string logSink)
  {
    Refresh();
  }

  public override string GetBoardStatusLine()
  {
    if (State?.Pattern == null)
      return null;

    if (State.Pattern.patternId == BossPatternId.A)
      return GameUIText.MarkLine(BossPatternRuntimeState.BetIndexToLabel(State.MarkedBetIndex));

    if (State.SecondJokerActive && !State.JokerConsumed)
      return GameUIText.JokerDoubleEnraged;

    return State.JokerConsumed ? GameUIText.JokerSpent : GameUIText.JokerInDeck;
  }

  public override string GetBetRiskIcon(string betLabel)
  {
    if (State?.Pattern?.patternId != BossPatternId.A)
      return "";

    return State.IsBetMarked(betLabel) ? " ♛" : "";
  }

  protected override void Refresh()
  {
    if (State?.Pattern == null)
    {
      SetTexts("", "", "");
      return;
    }

    string title = GameUIText.QueensTableTitle(State.Pattern.displayName);
    string body;
    string footer = State.Pattern.counterHint;

    if (State.Pattern.patternId == BossPatternId.A)
    {
      body = BuildMarkLine() + "\n\n" +
             GameUIText.MarkedBetHint +
             State.Pattern.description;
    }
    else
    {
      body = BuildDeckLine() + "\n\n" +
             (State.SecondJokerActive ? GameUIText.EnragedSecondJoker : "") +
             State.Pattern.description;
    }

    SetTexts(title, body, footer);
  }

  private string BuildMarkLine()
  {
    string safe = GameUIText.MarkedBet("Safe", State.MarkedBetIndex == 0);
    string risk = GameUIText.MarkedBet("Risk", State.MarkedBetIndex == 1);
    string allIn = GameUIText.MarkedBet("All-in", State.MarkedBetIndex == 2);
    return $"{safe}  {risk}  {allIn}";
  }

  private string BuildDeckLine()
  {
    if (State.JokerConsumed && !State.SecondJokerActive)
      return GameUIText.DeckJokerDrawn;

    if (State.JokerConsumed && State.SecondJokerActive)
      return GameUIText.DeckSecondJokerHidden;

    return GameUIText.DeckHidden;
  }
}

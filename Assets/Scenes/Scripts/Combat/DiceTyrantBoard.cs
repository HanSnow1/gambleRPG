using UnityEngine;

public class DiceTyrantBoard : BossGambleBoard
{
  private static int RequiredFaceForBet(string label) =>
    label switch
    {
      "Safe" => 4,
      "Risk" => 5,
      "All-in" => 6,
      _ => 4
    };

  public override float GetPlayerSuccessModifier(CombatBetOption option)
  {
    if (State?.Pattern == null)
      return 0f;

    return State.Pattern.GetSuccessModForBet(option.label);
  }

  public override float ResolvePlayerRoll(CombatBetOption option, out string logLine)
  {
    logLine = null;
    if (State == null)
      return 0f;

    int roll = Random.Range(1, 7);
    State.SetLastDiceRoll(roll);

    float mod = GetRollModifier(option, roll) * 100f;
    bool cursed = State.CursedFaces.Contains(roll);
    int need = RequiredFaceForBet(option.label);
    logLine = GameUIText.DiceRollLog(roll, cursed, need, option.label);

    Refresh();
    return mod;
  }

  private float GetRollModifier(CombatBetOption option, int roll)
  {
    if (State.CursedFaces.Contains(roll))
      return -0.20f;

    int need = RequiredFaceForBet(option.label);
    if (roll >= need)
      return 0.12f;

    return -0.12f;
  }

  public override string GetBoardStatusLine()
  {
    if (State?.Pattern == null)
      return null;

    string faces = BuildFaceLine();
    string roll = State.LastDiceRoll > 0
      ? GameUIText.LastRoll(State.LastDiceRoll)
      : GameUIText.AwaitingRoll;
    return $"{faces} | {roll}";
  }

  public override string GetBetRiskIcon(string betLabel)
  {
    if (State?.Pattern == null)
      return "";

    if (State.Pattern.patternId == BossPatternId.A && betLabel == "Safe")
      return " ☠";
    if (State.Pattern.patternId == BossPatternId.B && betLabel == "All-in" && State.CursedFaces.Contains(6))
      return " ☠";
    if (State.Pattern.patternId == BossPatternId.B && betLabel == "Safe" && State.CursedFaces.Contains(1))
      return " ☠";

    return "";
  }

  public override void OnPhaseEnraged(string logSink)
  {
    if (State == null || State.Pattern?.patternId != BossPatternId.B)
      return;

    Refresh();
  }

  protected override void Refresh()
  {
    if (State?.Pattern == null)
    {
      SetTexts("", "", "");
      return;
    }

    string title = GameUIText.DiceTyrantTitle(State.Pattern.displayName);
    string body = BuildFaceLine() + "\n\n" +
                  $"{GameUIText.DiceBetNeeds}\n" +
                  (State.PhaseEnraged ? GameUIText.EnragedExtraCurse + "\n" : "") +
                  State.Pattern.description;
    string footer = State.LastDiceRoll > 0
      ? GameUIText.LastRoll(State.LastDiceRoll)
      : GameUIText.PickBetDice;

    SetTexts(title, body, footer);
  }

  private string BuildFaceLine()
  {
    var parts = new System.Text.StringBuilder();
    for (int i = 1; i <= 6; i++)
    {
      if (parts.Length > 0)
        parts.Append(' ');
      parts.Append(State.CursedFaces.Contains(i) ? $"{i}☠" : $"{i}");
    }

    return parts.ToString();
  }
}

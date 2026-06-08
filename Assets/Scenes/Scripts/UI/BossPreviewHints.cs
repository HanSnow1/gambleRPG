using UnityEngine;

public static class BossPreviewHints
{
  public static string BuildPatternHints(BossDefinition boss)
  {
    if (boss == null)
      return "";

    string hintA = boss.patternA != null
      ? boss.patternA.GetPreviewHintOrDescription()
      : boss.riskPatternHintA;
    string hintB = boss.patternB != null
      ? boss.patternB.GetPreviewHintOrDescription()
      : boss.riskPatternHintB;

    return $"{GameUIText.PatternA(hintA)}\n\n{GameUIText.PatternB(hintB)}";
  }
}

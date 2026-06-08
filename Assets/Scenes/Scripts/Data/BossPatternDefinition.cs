using UnityEngine;

public enum BossPatternId
{
  A = 0,
  B = 1
}

public enum BossBetStyle
{
  None = 0,
  PreferSafe = 1,
  PreferRisk = 2,
  PreferAllIn = 3
}

public enum BossMinigameType
{
  None = 0,
  DiceFaces = 1,
  CardTable = 2
}

[CreateAssetMenu(menuName = "GambleRogue/Boss Pattern", fileName = "Pattern_")]
public class BossPatternDefinition : ScriptableObject
{
  [Header("Identity")]
  public BossPatternId patternId = BossPatternId.A;
  public string displayName = "Unknown Pattern";
  [TextArea(2, 5)] public string description = "";
  [TextArea(2, 4)] public string previewHint = "";
  [TextArea(2, 3)] public string counterHint = "";
  [TextArea(2, 4)] public string introNarration = "";
  public BossMinigameType minigameType = BossMinigameType.None;

  [Header("Player success (percent points, e.g. -15 = -15%)")]
  public float playerSafeSuccessMod;
  public float playerRiskSuccessMod;
  public float playerAllInSuccessMod;

  [Header("Enemy")]
  public float enemySuccessBonus;
  public BossBetStyle enemyBetStyle = BossBetStyle.None;

  [Header("Player chip on miss")]
  public int playerExtraChipOnMiss;
  [Tooltip("Extra chip when player misses with Risk.")]
  public int playerExtraChipOnRiskMissOnly;
  [Tooltip("One-time extra chip on the player's first miss this fight.")]
  public int jokerPenaltyOnFirstMiss;

  public string GetPreviewHintOrDescription() =>
    !string.IsNullOrWhiteSpace(previewHint) ? previewHint : description;

  public float GetSuccessModForBet(string label)
  {
    if (label == "Safe")
      return playerSafeSuccessMod;
    if (label == "Risk")
      return playerRiskSuccessMod;
    if (label == "All-in")
      return playerAllInSuccessMod;
    return 0f;
  }
}

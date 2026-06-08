using System.Collections.Generic;

public static class AugmentCombatFx
{
  public static void AddFromLogs(List<AugmentFxEvent> fx, string logBlock)
  {
    if (fx == null || string.IsNullOrEmpty(logBlock))
      return;

    foreach (string line in logBlock.Split('\n'))
    {
      string trimmed = line.Trim();
      if (string.IsNullOrEmpty(trimmed))
        continue;

      fx.Add(AugmentFxEvent.FromLogLine(trimmed));
    }
  }

  public static BossGambleType? ResolveGambleContext(BossDefinition boss)
  {
    if (boss != null)
      return boss.gambleType;

    if (RunManager.Instance != null && RunManager.Instance.State.isActive)
      return RunManager.Instance.State.currentFloor == 1
        ? BossGambleType.Dice
        : BossGambleType.Card;

    return null;
  }

  public static int GetDicePipBonus(BossDefinition boss, List<AugmentFxEvent> fx)
  {
    if (PlayerAugmentState.Instance == null)
      return 0;

    if (ResolveGambleContext(boss) != BossGambleType.Dice)
      return 0;

    float bonus = PlayerAugmentState.Instance.GetGambleBonusForContext(
      boss, AugmentEffectType.DiceBossExtraPip, BossGambleType.Dice);
    int pip = bonus > 0f ? 1 : 0;
    if (pip > 0)
      fx?.Add(AugmentFxEvent.ExtraPip(pip));

    return pip;
  }

  public static int GetCardRevealBonus(BossDefinition boss, List<AugmentFxEvent> fx)
  {
    if (PlayerAugmentState.Instance == null)
      return 0;

    var ctx = ResolveGambleContext(boss);
    if (ctx != BossGambleType.Card && ctx != BossGambleType.Shell)
      return 0;

    float bonus = PlayerAugmentState.Instance.GetGambleBonusForContext(
      boss, AugmentEffectType.ShellGameRevealCup, BossGambleType.Shell);
    int reveal = bonus > 0f ? 1 : 0;
    if (reveal > 0)
      fx?.Add(AugmentFxEvent.RevealedCup(reveal));

    return reveal;
  }
}

using UnityEngine;

public enum AugmentFxKind
{
  ExtraPip,
  RevealedCup,
  BossSlayer,
  LoadedDice,
  DoubleDown,
  HotStreak,
  CursedFace,
  QueensMark,
  JokerDrawn,
  RelicBoost,
  CheatDeath,
  Generic
}

public struct AugmentFxEvent
{
  public AugmentFxKind kind;
  public string title;
  public string subtitle;
  public Color accent;

  public static AugmentFxEvent ExtraPip(int bonus) => new()
  {
    kind = AugmentFxKind.ExtraPip,
    title = GameUIText.FxExtraPip,
    subtitle = GameUIText.FxExtraPipSub(bonus),
    accent = GameUITheme.Accent
  };

  public static AugmentFxEvent RevealedCup(int bonus) => new()
  {
    kind = AugmentFxKind.RevealedCup,
    title = GameUIText.FxRevealedCup,
    subtitle = GameUIText.FxRevealedCupSub(bonus),
    accent = GameUITheme.RiskBet
  };

  public static AugmentFxEvent FromLogLine(string line)
  {
    if (string.IsNullOrEmpty(line))
      return default;

    if (line.Contains("보스 학살자") || line.Contains("Boss Slayer"))
      return new AugmentFxEvent
      {
        kind = AugmentFxKind.BossSlayer,
        title = GameUIText.FxBossSlayer,
        subtitle = line,
        accent = GameUITheme.RiskBet
      };

    if (line.Contains("장전 주사위") || line.Contains("Loaded Dice"))
      return new AugmentFxEvent
      {
        kind = AugmentFxKind.LoadedDice,
        title = GameUIText.FxLoadedDice,
        subtitle = line,
        accent = GameUITheme.AllInBet
      };

    if (line.Contains("더블 다운") || line.Contains("Double Down"))
      return new AugmentFxEvent
      {
        kind = AugmentFxKind.DoubleDown,
        title = GameUIText.FxDoubleDown,
        subtitle = line,
        accent = GameUITheme.Heal
      };

    if (line.Contains("연속 적중") || line.Contains("Hot Streak"))
      return new AugmentFxEvent
      {
        kind = AugmentFxKind.HotStreak,
        title = GameUIText.FxHotStreak,
        subtitle = line,
        accent = GameUITheme.Heal
      };

    if (line.Contains("여왕의 표식") || line.Contains("Queen's Mark"))
      return new AugmentFxEvent
      {
        kind = AugmentFxKind.QueensMark,
        title = GameUIText.FxQueensMark,
        subtitle = line,
        accent = GameUITheme.HpMid
      };

    if (line.Contains("조커") || line.Contains("Joker"))
      return new AugmentFxEvent
      {
        kind = AugmentFxKind.JokerDrawn,
        title = GameUIText.FxJoker,
        subtitle = line,
        accent = GameUITheme.HpLow
      };

    if (line.Contains("☠") || line.Contains("저주") || line.ToLowerInvariant().Contains("cursed"))
      return new AugmentFxEvent
      {
        kind = AugmentFxKind.CursedFace,
        title = GameUIText.FxCursedFace,
        subtitle = line,
        accent = GameUITheme.HpLow
      };

    return new AugmentFxEvent
    {
      kind = AugmentFxKind.Generic,
      title = GameUIText.FxAugment,
      subtitle = line,
      accent = GameUITheme.Accent
    };
  }
}

using UnityEngine;

/// <summary>
/// Runtime boss pattern — picks A/B, drives minigame board, applies combat modifiers.
/// </summary>
public class BossPatternController
{
  private BossPatternDefinition _active;
  private BossPatternRuntimeState _runtime;
  private BossGambleBoard _board;
  private BettingCombatSystem _combat;
  private bool _jokerConsumed;
  private bool _secondJokerConsumed;

  public BossPatternDefinition Active => _active;
  public BossPatternRuntimeState Runtime => _runtime;
  public BossGambleBoard Board => _board;
  public bool HasActivePattern => _active != null;

  public void Initialize(BossDefinition boss, BettingCombatSystem combat)
  {
    _combat = combat;
    _jokerConsumed = false;
    _secondJokerConsumed = false;
    _active = null;
    _runtime = null;
    _board?.Hide();

    _board?.Hide();

    if (boss == null || combat == null)
      return;

    _active = PickPattern(boss);
    if (_active == null)
      return;

    _runtime = new BossPatternRuntimeState();
    _runtime.Initialize(boss, _active);
    _board = BossGambleBoardFactory.EnsureBoard(combat, boss, _runtime);

    LogPatternIntro();
  }

  private void LogPatternIntro()
  {
    if (_combat == null || _active == null)
      return;

    _combat.LogMessage(
      GameUIText.BossPatternIntro(_active.patternId, _active.displayName, _active.description));

    if (!string.IsNullOrWhiteSpace(_active.introNarration))
      _combat.LogMessage(_active.introNarration);

    if (!string.IsNullOrWhiteSpace(_active.counterHint))
      _combat.LogMessage(GameUIText.BossTip(_active.counterHint));
  }

  private static BossPatternDefinition PickPattern(BossDefinition boss)
  {
    bool hasA = boss.patternA != null;
    bool hasB = boss.patternB != null;

    if (hasA && hasB)
      return Random.value < 0.5f ? boss.patternA : boss.patternB;
    if (hasA)
      return boss.patternA;
    if (hasB)
      return boss.patternB;

    return null;
  }

  public void OnPlayerTurnStarted()
  {
    if (_runtime == null)
      return;

    int markBefore = _runtime.MarkedBetIndex;
    _runtime.OnPlayerTurnStarted();
    _board?.OnPlayerTurnStarted();

    if (_active?.patternId == BossPatternId.A &&
        _active.minigameType == BossMinigameType.CardTable &&
        _runtime.MarkedBetIndex != markBefore)
    {
      _combat?.LogMessage(
        $"Queen's Mark moves to {BossPatternRuntimeState.BetIndexToLabel(_runtime.MarkedBetIndex)}");
    }
  }

  public void CheckPhaseEnrage(int enemyHp, int enemyMaxHp)
  {
    if (_runtime == null || !_runtime.TryEnrageAtHalfHp(enemyHp, enemyMaxHp))
      return;

    _combat?.LogMessage("Boss pattern ENRAGED at 50% HP!");
    _board?.OnPhaseEnraged(null);

    if (_active?.patternId == BossPatternId.B && _active.minigameType == BossMinigameType.DiceFaces)
      _combat?.LogMessage("An extra face is cursed!");
    if (_active?.patternId == BossPatternId.B && _active.minigameType == BossMinigameType.CardTable)
      _combat?.LogMessage("A second Joker is shuffled into the deck!");
  }

  public float ResolvePlayerSuccess(CombatBetOption option, float baseChance, out string rollLog)
  {
    rollLog = null;
    float chance = ModifyPlayerSuccess(option, baseChance);

    if (_board != null)
    {
      float rollMod = _board.ResolvePlayerRoll(option, out rollLog) / 100f;
      chance = Mathf.Clamp01(chance + rollMod);
    }

    return chance;
  }

  public float ModifyPlayerSuccess(CombatBetOption option, float chance)
  {
    if (_active == null)
      return chance;

    float mod = GetPlayerSuccessMod(option);
    if (_board != null)
      mod += _board.GetPlayerSuccessModifier(option);

    if (Mathf.Approximately(mod, 0f))
      return chance;

    return Mathf.Clamp01(chance + mod / 100f);
  }

  public float ModifyEnemySuccess(float chance)
  {
    if (_active == null || Mathf.Approximately(_active.enemySuccessBonus, 0f))
      return chance;

    float bonus = _active.enemySuccessBonus;
    if (_runtime != null && _runtime.PhaseEnraged)
      bonus += 5f;

    return Mathf.Clamp01(chance + bonus / 100f);
  }

  public int ModifyPlayerChipOnMiss(CombatBetOption option, int baseChip)
  {
    if (_active == null)
      return baseChip;

    int chip = baseChip + _active.playerExtraChipOnMiss;

    if (option.label == "Risk" && _active.playerExtraChipOnRiskMissOnly > 0)
      chip += _active.playerExtraChipOnRiskMissOnly;

    if (_runtime != null && _runtime.IsBetMarked(option.label))
      chip += 1;

    if (_active.jokerPenaltyOnFirstMiss > 0)
    {
      if (!_jokerConsumed)
      {
        chip += _active.jokerPenaltyOnFirstMiss;
        _jokerConsumed = true;
        _runtime?.ConsumeJoker();
        _board?.OnPlayerMiss(option);
        _combat?.LogMessage($"Joker drawn! +{_active.jokerPenaltyOnFirstMiss} chip damage.");
      }
      else if (_runtime != null && _runtime.SecondJokerActive && !_secondJokerConsumed)
      {
        chip += _active.jokerPenaltyOnFirstMiss;
        _secondJokerConsumed = true;
        _runtime.ConsumeSecondJoker();
        _board?.OnPlayerMiss(option);
        _combat?.LogMessage($"Second Joker! +{_active.jokerPenaltyOnFirstMiss} chip damage.");
      }
    }

    return chip;
  }

  public BossBetStyle GetEnemyBetStyle()
  {
    if (_active == null)
      return BossBetStyle.None;

    if (_runtime != null && _runtime.PhaseEnraged && _active.enemyBetStyle == BossBetStyle.PreferAllIn)
      return BossBetStyle.PreferAllIn;

    return _active.enemyBetStyle;
  }

  public float GetPlayerSuccessMod(CombatBetOption option)
  {
    if (_active == null)
      return 0f;

    return _active.GetSuccessModForBet(option.label);
  }

  public string GetStatusLabel()
  {
    if (_active == null)
      return null;

    string board = _board?.GetBoardStatusLine();
    string core = GameUIText.PatternStatus(_active.patternId, _active.displayName);
    return string.IsNullOrEmpty(board) ? core : $"{core}\n{board}";
  }

  public string GetCounterHint() =>
    _active != null ? _active.counterHint : null;

  public string GetBetRiskSuffix(string label) =>
    _board?.GetBetRiskIcon(label) ?? "";
}

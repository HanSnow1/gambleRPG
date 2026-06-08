using System;
using System.Collections.Generic;
using UnityEngine;

public enum CombatTurnPhase
{
  Player,
  Enemy
}

public class BettingCombatSystem : MonoBehaviour
{
  [Serializable]
  public class CombatState
  {
    public int playerHp = 100;
    public int playerMaxHp = 100;
    public int enemyHp = 80;
    public int enemyMaxHp = 80;
    public bool IsPlayerDead => playerHp <= 0;
    public bool IsEnemyDead => enemyHp <= 0;
  }

  [SerializeField] private int startingEnemyHp = CombatBalance.DefaultEnemyHp;

  private int _playerSuccessStreak;
  private readonly BossPatternController _patternController = new();
  private readonly List<AugmentFxEvent> _postApplyFx = new();

  public BossDefinition CurrentBoss { get; private set; }
  public BossPatternController PatternController => _patternController;
  public CombatState State { get; } = new CombatState();
  public CombatTurnPhase CurrentPhase { get; private set; } = CombatTurnPhase.Player;
  public bool IsCombatActive { get; private set; }
  public bool IsPlayerTurn => IsCombatActive && CurrentPhase == CombatTurnPhase.Player;

  public event Action<string> OnCombatLog;
  public event Action OnCombatStateChanged;

  public void ResetCombat(BossDefinition boss = null, int enemyHp = -1)
  {
    CurrentBoss = boss;
    int resolvedEnemyHp = enemyHp >= 0 ? enemyHp : (boss != null ? boss.maxHp : startingEnemyHp);
    State.enemyHp = resolvedEnemyHp;
    State.enemyMaxHp = resolvedEnemyHp;
    State.playerHp = State.playerMaxHp;
    CurrentPhase = CombatTurnPhase.Player;
    IsCombatActive = true;
    _playerSuccessStreak = 0;
    _patternController.Initialize(boss, this);

    if (boss != null)
    {
      Log(GameUIText.CombatStartBoss(boss.displayName, boss.gambleType));
      LogBossAugmentHints(boss);
    }
    else
      Log(GameUIText.CombatStartEnemy(State.enemyHp));

    NotifyStateChanged();
  }

  public bool CanAffordBet(CombatBetOption option) =>
    IsCombatActive && State.playerHp >= option.betHp;

  public bool ShouldOfferHiddenBet() =>
    IsCombatActive &&
    IsPlayerTurn &&
    State.playerHp >= CombatBalance.Hidden.betHp &&
    !CanAffordBet(CombatBalance.Safe);

  public IReadOnlyList<AugmentFxEvent> ConsumePostApplyFx()
  {
    var copy = new List<AugmentFxEvent>(_postApplyFx);
    _postApplyFx.Clear();
    return copy;
  }

  public bool TryComputeDuel(
    CombatBetOption playerOption,
    CombatBetOption enemyOption,
    CombatPresentationTheme theme,
    out CombatTurnResult result)
  {
    result = default;
    if (!IsPlayerTurn)
    {
      Log(GameUIText.NotYourTurn);
      return false;
    }

    if (!CanAffordBet(playerOption))
    {
      Log(GameUIText.HpInsufficientBet(playerOption.label, playerOption.betHp, State.playerHp));
      return false;
    }

    _patternController.OnPlayerTurnStarted();

    return theme == CombatPresentationTheme.Card
      ? TryComputeCardDuel(playerOption, enemyOption, out result)
      : TryComputeDiceDuel(playerOption, enemyOption, out result);
  }

  public void ApplyDuelResult(CombatTurnResult result)
  {
    if (!IsPlayerTurn || !result.isDuelRound)
      return;

    foreach (string line in result.logLines)
      Log(line);

    if (result.duelWinner == DuelWinner.Player)
    {
      State.playerHp -= result.betHpCost;
      State.enemyHp = Mathf.Max(0, State.enemyHp - result.damageToEnemy);
      _playerSuccessStreak++;
      TryApplyConsecutiveHitHeal();
    }
    else if (result.duelWinner == DuelWinner.Enemy)
    {
      State.enemyHp -= Mathf.Clamp(result.enemyOption.betHp, 1, State.enemyHp);
      State.playerHp = Mathf.Max(0, State.playerHp - result.damageToPlayer);
      _playerSuccessStreak = 0;
    }
    else
    {
      _playerSuccessStreak = 0;
      State.playerHp = Mathf.Max(0, State.playerHp - result.chipToPlayer);
      State.enemyHp = Mathf.Max(0, State.enemyHp - result.chipToEnemy);
    }

    _patternController.CheckPhaseEnrage(State.enemyHp, State.enemyMaxHp);
    _patternController.Board?.RefreshBoard();

    if (TryFinalizeTurn(isPlayerAttacking: result.duelWinner == DuelWinner.Player))
      NotifyStateChanged();
  }

  private bool TryComputeDiceDuel(
    CombatBetOption playerOption,
    CombatBetOption enemyOption,
    out CombatTurnResult result)
  {
    var logs = new List<string>();
    var rollFx = new List<AugmentFxEvent>();
    var damageFx = new List<AugmentFxEvent>();
    int playerBet = playerOption.betHp;
    int enemyBet = Mathf.Clamp(enemyOption.betHp, 1, State.enemyHp);

    RollDicePair(rollFx, out int playerRaw, out int enemyRaw, out bool playerCursed, out bool enemyCursed,
      out int playerValue, out int enemyValue, out DuelWinner winner);

    result = new CombatTurnResult
    {
      isDuelRound = true,
      isPlayerAttacker = true,
      option = playerOption,
      enemyOption = enemyOption,
      betHpCost = playerBet,
      duelWinner = winner,
      playerDiceRoll = playerRaw,
      enemyDiceRoll = enemyRaw,
      playerCompareValue = playerValue,
      enemyCompareValue = enemyValue,
      playerRollCursed = playerCursed,
      enemyRollCursed = enemyCursed,
      logLines = logs,
      rollFxEvents = rollFx,
      damageFxEvents = damageFx,
      augmentFxEvents = new List<AugmentFxEvent>()
    };

    _patternController.Runtime?.SetLastDiceRoll(playerRaw);

    logs.Add(GameUIText.DuelLine(playerOption.label, playerBet, enemyOption.label, enemyBet));
    logs.Add(GameUIText.RollsLine(playerRaw, playerCursed, playerValue, enemyRaw, enemyValue));

    FillDuelOutcome(ref result, playerOption, enemyOption, playerBet, enemyBet, logs, damageFx);
    return true;
  }

  private bool TryComputeCardDuel(
    CombatBetOption playerOption,
    CombatBetOption enemyOption,
    out CombatTurnResult result)
  {
    var logs = new List<string>();
    var rollFx = new List<AugmentFxEvent>();
    var damageFx = new List<AugmentFxEvent>();
    int playerBet = playerOption.betHp;
    int enemyBet = Mathf.Clamp(enemyOption.betHp, 1, State.enemyHp);

    int revealBonus = AugmentCombatFx.GetCardRevealBonus(CurrentBoss, rollFx);
    int playerValue = CombatPresentationThemeResolver.RollCardValue(true) + revealBonus;
    int enemyValue = CombatPresentationThemeResolver.RollCardValue(true);

    int safety = 0;
    while (playerValue == enemyValue && safety < 4)
    {
      playerValue = CombatPresentationThemeResolver.RollCardValue(true);
      enemyValue = CombatPresentationThemeResolver.RollCardValue(true);
      safety++;
    }

    DuelWinner winner = playerValue > enemyValue
      ? DuelWinner.Player
      : enemyValue > playerValue
        ? DuelWinner.Enemy
        : DuelWinner.Tie;

    result = new CombatTurnResult
    {
      isDuelRound = true,
      isPlayerAttacker = true,
      option = playerOption,
      enemyOption = enemyOption,
      betHpCost = playerBet,
      duelWinner = winner,
      playerCardValue = playerValue,
      enemyCardValue = enemyValue,
      playerCard = CombatPresentationThemeResolver.FormatCard(playerValue),
      enemyCard = CombatPresentationThemeResolver.FormatCard(enemyValue),
      logLines = logs,
      rollFxEvents = rollFx,
      damageFxEvents = damageFx,
      augmentFxEvents = new List<AugmentFxEvent>()
    };

    logs.Add(GameUIText.DuelLine(playerOption.label, playerBet, enemyOption.label, enemyBet));
    logs.Add(GameUIText.CardsLine(result.playerCard, playerValue, result.enemyCard, enemyValue));

    FillDuelOutcome(ref result, playerOption, enemyOption, playerBet, enemyBet, logs, damageFx);
    return true;
  }

  private void RollDicePair(
    List<AugmentFxEvent> rollFx,
    out int playerRaw,
    out int enemyRaw,
    out bool playerCursed,
    out bool enemyCursed,
    out int playerValue,
    out int enemyValue,
    out DuelWinner winner)
  {
    int safety = 0;
    do
    {
      playerRaw = CombatPresentationThemeResolver.RollD6();
      enemyRaw = CombatPresentationThemeResolver.RollD6();
      playerCursed = IsCursedRoll(playerRaw);
      enemyCursed = IsCursedRoll(enemyRaw);
      playerValue = playerRaw + AugmentCombatFx.GetDicePipBonus(CurrentBoss, rollFx);
      enemyValue = enemyRaw;
      if (playerCursed)
      {
        playerValue = Mathf.Max(1, playerValue - 2);
        rollFx.Add(AugmentFxEvent.FromLogLine(GameUIText.CursedFace(playerRaw)));
      }
      if (enemyCursed)
        enemyValue = Mathf.Max(1, enemyValue - 2);

      winner = playerValue > enemyValue
        ? DuelWinner.Player
        : enemyValue > playerValue
          ? DuelWinner.Enemy
          : DuelWinner.Tie;
      safety++;
    } while (winner == DuelWinner.Tie && safety < 4);
  }

  private bool IsCursedRoll(int roll)
  {
    var runtime = _patternController.Runtime;
    return runtime != null && runtime.CursedFaces.Contains(roll);
  }

  private void FillDuelOutcome(
    ref CombatTurnResult result,
    CombatBetOption playerOption,
    CombatBetOption enemyOption,
    int playerBet,
    int enemyBet,
    List<string> logs,
    List<AugmentFxEvent> damageFx)
  {
    if (result.duelWinner == DuelWinner.Player)
    {
      int damage = AugmentCombatModifiers.ModifyDamageOnSuccess(
        playerOption.damageOnSuccess, CurrentBoss, out string augmentLog);
      result.damageToEnemy = damage;
      result.success = true;
      logs.Add(GameUIText.YouWinDamage(playerBet, damage));
      if (!string.IsNullOrEmpty(augmentLog))
      {
        logs.Add(augmentLog);
        AugmentCombatFx.AddFromLogs(damageFx, augmentLog);
      }
    }
    else if (result.duelWinner == DuelWinner.Enemy)
    {
      int baseDamage = enemyOption.damageOnSuccess;
      if (CurrentBoss != null && CurrentBoss.baseAttack > 0)
      {
        float scale = CurrentBoss.baseAttack / (float)CombatBalance.Risk.damageOnSuccess;
        baseDamage = Mathf.Max(1, Mathf.RoundToInt(baseDamage * scale));
      }

      int damage = AugmentCombatModifiers.ModifyIncomingEnemyDamage(baseDamage, out string augmentLog);
      int chipBase = playerOption.chipDamageOnFail;
      int chipWithMods = _patternController.ModifyPlayerChipOnMiss(playerOption, chipBase);
      if (chipWithMods > chipBase)
      {
        int extra = chipWithMods - chipBase;
        damage += extra;
        string penaltyLine = chipWithMods > chipBase + 1
          ? GameUIText.JokerDrawn(extra)
          : GameUIText.QueensMarkDamage(extra);
        logs.Add(penaltyLine);
        damageFx.Add(AugmentFxEvent.FromLogLine(penaltyLine));
      }

      result.damageToPlayer = damage;
      result.success = false;
      logs.Add(GameUIText.EnemyWinDamage(enemyBet, damage));
      if (!string.IsNullOrEmpty(augmentLog))
      {
        logs.Add(augmentLog);
        AugmentCombatFx.AddFromLogs(damageFx, augmentLog);
      }
    }
    else
    {
      int playerChip = AugmentCombatModifiers.ModifyChipDamageToPlayer(
        playerOption.chipDamageOnFail, out string pLog);
      playerChip = _patternController.ModifyPlayerChipOnMiss(playerOption, playerChip);
      result.chipToPlayer = playerChip;
      result.chipToEnemy = Mathf.Max(1, enemyOption.chipDamageOnFail);
      result.success = false;
      logs.Add(GameUIText.TieChipDamage(playerChip, result.chipToEnemy));
      if (!string.IsNullOrEmpty(pLog))
      {
        logs.Add(pLog);
        AugmentCombatFx.AddFromLogs(damageFx, pLog);
      }
    }
  }

  private bool TryFinalizeTurn(bool isPlayerAttacking)
  {
    if (!TryHandleCheatDeath())
      State.playerHp = Mathf.Max(0, State.playerHp);

    if (State.IsEnemyDead)
    {
      Log(GameUIText.EnemyDefeated);
      IsCombatActive = false;
      NotifyStateChanged();
      return false;
    }

    if (State.IsPlayerDead)
    {
      Log(GameUIText.GameOverHpZero);
      IsCombatActive = false;
      NotifyStateChanged();
      return false;
    }

    return true;
  }

  private void TryApplyConsecutiveHitHeal()
  {
    if (_playerSuccessStreak < 2 || PlayerAugmentState.Instance == null)
      return;

    int heal = PlayerAugmentState.Instance.GetConsecutiveHitHealAmount();
    if (heal <= 0)
      return;

    int before = State.playerHp;
    State.playerHp = Mathf.Min(State.playerHp + heal, State.playerMaxHp);
    if (State.playerHp > before)
    {
      string line = GameUIText.HotStreakHeal(State.playerHp - before, _playerSuccessStreak);
      Log(line);
      _postApplyFx.Add(AugmentFxEvent.FromLogLine(line));
    }
  }

  private bool TryHandleCheatDeath()
  {
    if (State.playerHp > 0)
      return false;

    if (PlayerAugmentState.Instance == null)
      return false;

    if (PlayerAugmentState.Instance.TryPreventDeath(this, out string msg))
    {
      if (!string.IsNullOrEmpty(msg))
      {
        Log(msg);
        _postApplyFx.Add(new AugmentFxEvent
        {
          kind = AugmentFxKind.CheatDeath,
          title = GameUIText.FxCheatDeath,
          subtitle = msg,
          accent = GameUITheme.Heal
        });
      }
      return true;
    }

    return false;
  }

  public void EndCombat()
  {
    IsCombatActive = false;
    NotifyStateChanged();
  }

  public void LogMessage(string msg) => OnCombatLog?.Invoke(msg);

  private void LogBossAugmentHints(BossDefinition boss)
  {
    if (PlayerAugmentState.Instance == null || boss == null)
      return;

    float pip = PlayerAugmentState.Instance.GetBossGambleSuccessBonus(
      boss, AugmentEffectType.DiceBossExtraPip);
    if (pip > 0f)
      Log(GameUIText.AugmentExtraPip(pip));

    float reveal = PlayerAugmentState.Instance.GetBossGambleSuccessBonus(
      boss, AugmentEffectType.ShellGameRevealCup);
    if (reveal > 0f)
      Log(GameUIText.AugmentRevealedCup(reveal));

    float bossDmg = PlayerAugmentState.Instance.GetBossOnlyDamageBonusPercent(boss);
    if (bossDmg > 0f)
      Log(GameUIText.AugmentBossSlayer(bossDmg));
  }

  private void Log(string msg) => LogMessage(msg);

  private void NotifyStateChanged() => OnCombatStateChanged?.Invoke();
}

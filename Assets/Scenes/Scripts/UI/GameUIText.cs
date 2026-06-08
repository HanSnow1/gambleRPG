/// <summary>플레이어에게 보이는 UI 문구 (한국어).</summary>
public static class GameUIText
{
  public static string BetLabel(string internalLabel) =>
    internalLabel switch
    {
      "Safe" => "안전",
      "Risk" => "리스크",
      "All-in" => "올인",
      "Hidden" => "히든",
      _ => internalLabel
    };

  public static string Player => "플레이어";
  public static string Enemy => "적";
  public static string You => "나";
  public static string GameOver => "게임 오버";
  public static string Victory => "승리";
  public static string BossDefeated => "보스 처치";
  public static string FightEnded => "전투 종료";
  public static string DuelInProgress => "<color=#8A9BB8>대결 진행 중</color>\n배팅 → 굴림 → 비교";
  public static string ChooseBet => "<color=#7DFFAA>배팅을 선택하세요</color>";
  public static string ChooseBetBossHint => "<color=#8A9BB8>▶ 보스 정보</color>에서 규칙 확인";
  public static string DiceDuelHint => "더 높은 주사위가 승리!";
  public static string CardDuelHint => "더 높은 카드가 승리!";
  public static string HigherRollWins => "높은 숫자가 대결에서 이깁니다!";
  public static string HiddenBetStatus => "<color=#8A9BB8>히든 배팅</color> — HP 1 걸고 승리 시 5 데미지";
  public static string LowHpSafeOnly => "<color=#E8C547>HP 부족</color> — 안전 배팅만 가능";
  public static string HpInsufficient => "<color=#FF6B6B>HP 부족</color> — 사용 가능한 배팅 없음";
  public static string HpInsufficientButton => "<color=#FF6B6B><size=12>HP 부족</size></color>";
  public static string BossInfoShow => "▶  보스 정보";
  public static string BossInfoHide => "▼  보스 정보 닫기";
  public static string Restart => "다시 시작";
  public static string NormalFight => "일반 전투 — 보스 규칙 없음";
  public static string Continue => "계속";
  public static string StageClear => "스테이지 클리어";
  public static string StartRun => "런 시작";
  public static string NewRun => "새 런";
  public static string ToTitle => "타이틀";
  public static string VictoryTitle => "승리!";
  public static string VictoryBody => "모든 층을 클리어했습니다!";
  public static string GameOverBody => "HP가 0이 되었습니다.";
  public static string ChooseAugment => "증강을 선택하세요";
  public static string AugmentPickSubtitle => "3개 중 1개 — 같은 카드는 레벨업";
  public static string BossPreview => "보스 프리뷰";
  public static string StartFight => "▶  전투 시작";
  public static string DiceBossBtn => "주사위 보스";
  public static string CardBossBtn => "카드 보스";

  public static string FloorProgress(int floor, int total, string phase) =>
    $"{floor}층 / {total}층 — {phase}";

  public static string FightPhase(int completed, int total) => $"전투 {completed + 1}/{total}";
  public static string BossFightPhase => "보스전";

  public static string DuelButtonLabel(string internalLabel, int betHp, int damage, string riskSuffix = "") =>
    $"{BetLabel(internalLabel)}{riskSuffix}\n배팅 {betHp} HP\n승리 → {damage} 데미지";

  public static string LegacyButtonLabel(string internalLabel, int betHp, float chance, int hit, int miss, string riskSuffix = "") =>
    $"{BetLabel(internalLabel)}{riskSuffix}\n배팅 {betHp} | {chance:P0}\n적중 {hit} / 빗나감 {miss}";

  public static string HiddenButtonLabel(int damage) =>
    $"히든\n배팅 1 HP\n승리 → {damage} 데미지";

  public static string BossRule(BossGambleType type)
  {
    return type switch
    {
      BossGambleType.Dice => $"보스 규칙: 주사위 (+{CombatBalance.DiceBossSuccessBonus:P0} 성공)",
      BossGambleType.Card => $"보스 규칙: 카드 (+{CombatBalance.CardBossSuccessBonus:P0} 성공)",
      BossGambleType.Shell => $"보스 규칙: 컵 게임 (+{CombatBalance.CardBossSuccessBonus:P0} 성공)",
      _ => "보스 규칙: 알 수 없음"
    };
  }

  public static string TitleGuide =>
    "<color=#8A9BB8>안전</color>  <color=#E8A838>리스크</color>  <color=#E74C5C>올인</color>  —  2개 층  —  증강 & 유물\n" +
    "1층: <b>주사위 폭군</b>   →   2층: <b>카드의 여왕</b>";

  public static string TitleSubtitle => "HP 배팅 로그라이크";

  public static string TierLabel(AugmentTier tier) =>
    tier switch
    {
      AugmentTier.Silver => "실버",
      AugmentTier.Gold => "골드",
      AugmentTier.Prismatic => "플래티넘",
      _ => "?"
    };

  public static string FinalBossTitle(int floor) => $"최종 보스 — {floor}층";
  public static string FloorBossTitle(int floor) => $"{floor}층 보스";

  public static string HpFullNoHeal => "HP가 가득 차 회복 없음";
  public static string HpRecovered(int amount, float percent) =>
    $"<color=#7DFFAA>+{amount} HP 회복</color>  (최대 HP의 {percent:P0})";

  public static string StageClearBody(int floor, int completed, int total, int hpBefore, int hpAfter, int maxHp, string healLine, string nextLine) =>
    $"{floor}층 — 전투 {completed} / {total} 클리어\n\n" +
    $"HP  {hpBefore}  →  {hpAfter}  /  {maxHp}\n" +
    $"{healLine}\n\n" +
    nextLine;

  public static string NextBossLine(int floor) => $"계속 → 증강 선택 후 {floor}층 보스";
  public static string NextAugmentLine => "계속 → 다음 전투를 위한 증강 선택";
  public static string FloorClearTitle(int floor) => $"{floor}층 클리어";

  public static string FloorClearBody(int clearedFloor, int hpBefore, int hpAfter, int maxHp, string healLine, int nextFloor, string nextBossName) =>
    $"{clearedFloor}층 보스 처치!\n\n" +
    $"HP  {hpBefore}  →  {hpAfter}  /  {maxHp}\n" +
    $"{healLine}\n\n" +
    $"{nextFloor}층이 기다립니다 — {nextBossName}\n" +
    "계속해서 다음 층으로 이동합니다.";

  // 전투 연출
  public static string PhaseYouBet => "① 배팅 선택 완료";
  public static string PhaseEnemyBet(string name) => $"② {name} 배팅 선택";
  public static string PhaseYourDice => "③ 내 주사위!";
  public static string PhaseEnemyDice(string name) => $"④ {name} 주사위!";
  public static string PhaseCompareDice => "⑤ 주사위 비교!";
  public static string PhaseDrawYourCard => "③ 내 카드 뽑기";
  public static string PhaseEnemyCard(string name) => $"④ {name} 카드 뽑기";
  public static string PhaseCompareCards(int player, int enemy) => $"⑤ 비교 — {player} vs {enemy}";
  public static string PhaseDamage => "⑥ 승자 배팅으로 데미지";
  public static string YouWinRoll => "<color=#7DFFAA><b>주사위 승리!</b></color>";
  public static string EnemyWinRoll(string name) => $"<color=#FF6B6B><b>{name} 주사위 승리!</b></color>";
  public static string TieChip => "<color=#E8C547><b>무승부 — 양쪽 칩 데미지!</b></color>";
  public static string YourCardWins => "<color=#7DFFAA><b>카드 승리!</b></color>";
  public static string EnemyCardWins => "<color=#FF6B6B><b>적 카드 승리!</b></color>";
  public static string YourCardLabel => "내 카드";
  public static string EnemyCardLabel => "적 카드";

  public static string BetLineYou(string label, int betHp) =>
    $"나: <b>{BetLabel(label)}</b>  (배팅 {betHp} HP)";
  public static string BetLineEnemy(string name, string label, int betHp) =>
    $"{name}: <b>{BetLabel(label)}</b>  (배팅 {betHp} HP)";

  public static string DamageLinePlayerWin(int betCost, int damage) =>
    $"<color=#FF6B6B>나</color>  -{betCost} HP   →   <color=#7DFFAA>적</color>  -{damage} HP";
  public static string DamageLineEnemyWin(string name, int enemyBet, int damage) =>
    $"<color=#FF6B6B>{name}</color>  -{enemyBet} HP   →   <color=#FF6B6B>나</color>  -{damage} HP";
  public static string DamageLineTie(int playerChip, int enemyChip, string enemyName) =>
    $"<color=#E8C547>무승부</color>  나 -{playerChip} HP  |  {enemyName} -{enemyChip} HP";

  // 전투 로그
  public static string CombatStartBoss(string name, BossGambleType type) =>
    $"{name}({type})와 전투 시작 — 양쪽 배팅 후 굴림·비교로 승부.";
  public static string CombatStartEnemy(int hp) =>
    $"적(HP {hp})과 전투 시작 — 배팅을 선택해 대결을 시작하세요.";
  public static string NotYourTurn => "지금은 내 턴이 아닙니다.";
  public static string HpInsufficientBet(string label, int need, int have) =>
    $"HP 부족 — {BetLabel(label)}은(는) {need} HP 필요 (현재 {have} HP).";
  public static string DuelLine(string playerLabel, int playerBet, string enemyLabel, int enemyBet) =>
    $"대결 — 나: {BetLabel(playerLabel)} ({playerBet} HP) vs 적: {BetLabel(enemyLabel)} ({enemyBet} HP)";
  public static string RollsLine(int playerRaw, bool playerCursed, int playerValue, int enemyRaw, int enemyValue) =>
    $"굴림 — 나: {playerRaw}{(playerCursed ? "☠" : "")} ({playerValue}) vs 적: {enemyRaw} ({enemyValue})";
  public static string CardsLine(string playerCard, int playerValue, string enemyCard, int enemyValue) =>
    $"카드 — 나: {playerCard} ({playerValue}) vs 적: {enemyCard} ({enemyValue})";
  public static string CursedFace(int face) => $"저주 면 {face} — 굴림 -2!";
  public static string YouWinDamage(int bet, int damage) => $"승리! 배팅 {bet} HP → {damage} 데미지!";
  public static string JokerDrawn(int extra) => $"조커 등장! +{extra} 데미지.";
  public static string QueensMarkDamage(int extra) => $"여왕의 표식 — +{extra} 데미지.";
  public static string EnemyWinDamage(int enemyBet, int damage) => $"적 승리! 배팅 {enemyBet} HP → 나에게 {damage} 데미지.";
  public static string TieChipDamage(int playerChip, int enemyChip) =>
    $"무승부! 칩 데미지 — 나 {playerChip}, 적 {enemyChip}.";
  public static string EnemyDefeated => "적 처치!";
  public static string GameOverHpZero => "게임 오버 — HP 0";
  public static string HotStreakHeal(int amount, int streak) =>
    $"증강 발동! 연속 적중: +{amount} HP (연속 {streak}회)";
  public static string AugmentExtraPip(float bonus) => $"증강: 추가 눈금 활성 (+{bonus:P0} 주사위 보스 성공)";
  public static string AugmentRevealedCup(float bonus) => $"증강: 공개 컵 활성 (+{bonus:P0} 성공 — 컵 1개 확인)";
  public static string AugmentBossSlayer(float bonus) => $"증강: 보스 학살자 활성 (+{bonus:0}% 보스 데미지)";
  public static string BossSlayerLog(float pct, int before, int damage) =>
    $"증강 발동! 보스 학살자: 보스 대상 +{pct:0}% ({before} → {damage})";
  public static string LoadedDiceLog(int level, float bonusPct, int before, int damage) =>
    $"증강 발동! 장전 주사위 Lv{level}: +{bonusPct:P0} 데미지 ({before} → {damage})";
  public static string DoubleDownDealLog(int level, int before, int damage) =>
    $"증강 발동! 더블 다운 Lv{level}: 가하는 데미지 ×2 ({before} → {damage})";
  public static string DoubleDownChipLog(int level, int before, int chip) =>
    $"증강 발동! 더블 다운 Lv{level}: 칩 데미지 ×2 ({before} → {chip})";
  public static string DoubleDownTakenLog(int level, int before, int damage) =>
    $"증강 발동! 더블 다운 Lv{level}: 받는 데미지 ×2 ({before} → {damage})";
  public static string AugmentsNone => "증강: 없음";
  public static string RelicsNone => "유물: 없음";
  public static string AugmentsSummary(string list) => $"증강: {list}";
  public static string RelicsSummary(string list) => $"유물: {list}";
  public static string BossTip(string counter) => $"팁: {counter}";

  // 증강 FX
  public static string FxExtraPip => "추가 눈금";
  public static string FxRevealedCup => "공개 컵";
  public static string FxBossSlayer => "보스 학살자";
  public static string FxLoadedDice => "장전 주사위";
  public static string FxDoubleDown => "더블 다운";
  public static string FxHotStreak => "연속 적중";
  public static string FxQueensMark => "여왕의 표식";
  public static string FxJoker => "조커!";
  public static string FxCursedFace => "저주 면";
  public static string FxAugment => "증강";
  public static string FxExtraPipSub(int bonus) => $"굴림 +{bonus}!";
  public static string FxRevealedCupSub(int bonus) => $"카드 +{bonus}!";
  public static string FxCheatDeath => "죽음 회피";

  // 보스 프리뷰
  public static string PreviewTitle => "보스 프리뷰";
  public static string PreviewFloor(int floor, int total) => $"{floor}층 / {total}층 — 프리뷰";
  public static string BossHpLine(string name, int hp) => $"{name}  |  HP {hp}";
  public static string FinalBossIntro => "런의 마지막 보스.\n\n";
  public static string FloorBossIntro(int floor) => $"{floor}층 보스전.\n\n";
  public static string Weakness(string hint) => $"약점: {hint}";
  public static string FloorBossPreview(int floor, string name, int total, int completed) =>
    $"{floor}층 보스: {name}\n" +
    $"전투 {total}회 클리어 후 이 보스와 대결.\n" +
    $"진행: {completed}/{total}\n\n";
  public static string PatternA(string hint) => $"패턴 A: {hint}";
  public static string PatternB(string hint) => $"패턴 B: {hint}";
  public static string SelectAugment => "▶  선택";

  // 보스 보드
  public static string DiceTyrantTitle(string pattern) => $"주사위 폭군 — {pattern}";
  public static string DiceBetNeeds => "안전 4+ | 리스크 5+ | 올인 6+";
  public static string EnragedExtraCurse => "격노: 추가 저주 활성!";
  public static string LastRoll(int roll) => $"마지막 굴림: {roll}";
  public static string AwaitingRoll => "굴림 대기";
  public static string PickBetDice => "배팅 선택 — 공격마다 주사위 굴림.";
  public static string DiceRollLog(int roll, bool cursed, int need, string label) =>
    $"주사위: {roll}{(cursed ? " ☠" : "")} ({BetLabel(label)}에 {need}+ 필요)";
  public static string QueensTableTitle(string pattern) => $"여왕의 테이블 — {pattern}";
  public static string QueensMarkOn(string label) => $"여왕의 표식 — {BetLabel(label)}!";
  public static string CardsShuffledJoker => "카드 섞음 — 첫 실패 시 조커 대기.";
  public static string MarkLine(string label) => $"표식: {BetLabel(label)}";
  public static string JokerDoubleEnraged => "조커 ×2 (격노)";
  public static string JokerSpent => "조커 사용됨";
  public static string JokerInDeck => "덱에 조커 있음";
  public static string MarkedBetHint => "표식 배팅: 실패 시 추가 칩 데미지.\n표식은 3턴마다 이동.\n\n";
  public static string EnragedSecondJoker => "격노: 두 번째 조커 섞음!\n";
  public static string DeckJokerDrawn => "덱: 조커 뽑음 — 일반 칩만.";
  public static string DeckSecondJokerHidden => "덱: 2번째 조커 아직 숨김!";
  public static string DeckHidden => "덱: ??? ??? ??? ??? [조커]";
  public static string MarkedBet(string label, bool marked) =>
    marked ? $"[♛ {BetLabel(label)}]" : $"[ {BetLabel(label)} ]";

  public static string VictoryFullBody =>
    "당신은 모든 행운을 극복했습니다";

  public static string BossPatternIntro(BossPatternId id, string name, string description) =>
    $"보스 패턴: [{id}] {name}\n{description}";
  public static string PatternStatus(BossPatternId id, string name) =>
    $"패턴 {id}: {name}";
  public static string CheatDeathHeal(string name, float healPct, int remaining) =>
    $"죽음 회피: {name} (최대 HP의 {healPct:0}% 회복)  [남은 {remaining}회]";
  public static string CheatDeathSurvive(string name, int remaining) =>
    $"죽음 회피: {name} (HP 1로 생존)  [남은 {remaining}회]";
  public static string AugmentFloorMaxHp(string name, int level, float pct) =>
    $"증강: {name} Lv{level} (이번 층 최대 HP +{pct:0}%)";
  public static string RelicMaxHp(string name, float pct, int bonus) =>
    $"유물: {name} (최대 HP +{pct:0}% → +{bonus})";
}

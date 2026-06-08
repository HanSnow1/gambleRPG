# GambleRogue — 발표 시연 스크립트 (5분)

## 0. 오프닝 (30초)

> "GambleRogue는 **HP를 걸고 싸우는 로그라이크**입니다.  
> 매 턴 Safe / Risk / All-in 중 하나를 고르고, 성공하면 피해를 주고 실패하면 칩 피해만 받습니다.  
> 증강과 유물을 모아 **2층 보스**를 처치하면 승리합니다."

**화면**: 타이틀 → Start Run

---

## 1. 증강 & 유물 (45초)

> "런 시작 시 **증강 3택1**과 **시작 유물**을 받습니다.  
> 증강은 Silver / Gold / Prismatic 등급이 있고, 같은 카드를 다시 뽑으면 레벨업됩니다."

**추천 시연 픽**: Loaded Dice, Boss Slayer, Hot Streak 중 하나

**멘트**: 선택한 증강 효과 한 줄 설명

---

## 2. 1층 — Dice Tyrant (1분 30초)

**화면**: 보스 프리뷰 → Pattern A/B 힌트 → START FIGHT

> "1층 보스는 **Dice Tyrant**입니다. 전투 시작 시 Pattern A 또는 B가 랜덤으로 정해집니다.  
> 좌측 **주사위 보드**에 저주된 면(☠)이 표시되고, 매 공격마다 1d6이 굴러갑니다."

**시연 포인트**:
- Safe 버튼에 ☠ 표시 (Pattern A)
- 전투 로그: `Dice roll: 5`, `Boss pattern: [A] Stolen Number`
- Risk 위주로 보스 HP 깎기

**STAGE CLEAR** → HP 회복 25% → 증강 1회 더 선택

---

## 3. 1층 보스전 (1분)

> "일반전 클리어 후 **1층 보스전**입니다.  
> Pattern B면 1 또는 6이 저주되고, 보스는 All-in을 선호합니다.  
> HP 50% 이하에서 **ENRAGED** — 패턴이 강화됩니다."

**시연**: 보스전 2~3턴 후 승리 또는 Risk로 빠르게 마무리

**FLOOR 1 CLEAR** → 35% 회복

---

## 4. 2층 — Queen Of Cards (1분)

> "2층 보스는 **Queen Of Cards**입니다.  
> Pattern A는 **♛ 표식**이 Risk 등 한 슬롯에 붙고, 3턴마다 이동합니다.  
> Pattern B는 **조커**가 섞여 첫 미스에 큰 칩 피해가 옵니다."

**시연 포인트**:
- 카드 테이블 UI
- 첫 턴 Safe로 조커 회피 전략 언급
- Revealed Cup / Boss Slayer 증강 시너지 (있으면)

---

## 5. 클리어 & 마무리 (30초)

**VICTORY** 화면

> "2층 보스를 처치하면 런 클리어입니다.  
> HP가 0이 되면 즉시 게임오버 — 파산 방지 없이 긴장감을 유지했습니다."

**Q&A 예상 질문**:
- *왜 영어 UI?* → TMP 한글 폰트 깨짐 방지
- *밸런스?* → `Docs/combat_balance.md` 참고
- *협업 구조?* → A 전투/B 증강/C 런, `GameFlowController`로 통합

---

## 시연 전 체크리스트

- [ ] Unity 6000.4.8f1, `Main.unity` 열림
- [ ] Play → Start Run 정상
- [ ] 증강 3장 표시
- [ ] 보스전 좌측 미니게임 보드 보임
- [ ] STAGE CLEAR / VICTORY 패널 뜸
- [ ] (선택) 5분 전체 플레이 녹화 backup

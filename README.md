# GambleRogue

HP 배팅 로그라이크 (Unity 6000.4 / C#)

## 팀

| 역할 | 담당 |
|------|------|
| A — 전투 & 보스 | 김세환 |
| B — 증강 | 곽태호 |
| C — 런 & 유물 | 이다호 |

## 실행 방법

1. **Unity Hub** → Unity **6000.4.8f1** 설치
2. 프로젝트 폴더 열기: `gambleRPG-main 2`
3. 씬 열기: `Assets/Main.unity`
4. **Play** 버튼 클릭

## 조작

| 입력 | 동작 |
|------|------|
| **Start Run** | 새 런 시작 |
| **Safe / Risk / All-in** | 플레이어 공격 베팅 (턴제) |
| **Continue** | 스테이지 클리어 후 다음 단계 |
| **START FIGHT** | 보스 프리뷰에서 전투 시작 |
| 증강 카드 클릭 | 3장 중 1장 선택 |

## 게임 루프 (약 5~10분)

```
타이틀 → 시작 증강 1회
  → 1층 프리뷰 (Dice Tyrant)
  → 일반 전투 1회 → STAGE CLEAR (+25% HP) → 증강 선택
  → 1층 보스전 (패턴 A/B 랜덤 + 주사위 미니게임)
  → FLOOR 1 CLEAR (+35% HP)
  → 2층 프리뷰 (Queen Of Cards)
  → 일반 전투 1회 → STAGE CLEAR → 증강
  → 최종 보스전 (카드 패턴 + 조커/표식)
  → VICTORY / GAME OVER
```

## 핵심 시스템

- **턴제 배팅 전투**: 성공 = 베팅 HP 지불 + 적에게 피해 / 실패 = 작은 칩 피해만
- **증강 14종**: Silver / Gold / Prismatic (레벨업, 보스 전용, 리롤 등)
- **유물 3종**: 최대 HP, 공격력, 피해 감소
- **보스 패턴**: Dice Tyrant / Queen Of Cards 각각 Pattern A·B
- **보스 미니게임 보드**: 주사위 ☠ 면, 여왕 ♛ 표식, 조커 덱 UI

## 주요 파일

| 경로 | 설명 |
|------|------|
| `Assets/Scenes/Scripts/Core/GameFlowController.cs` | 전체 런 흐름 |
| `Assets/Scenes/Scripts/Combat/` | 전투, 보스 패턴, 미니게임 |
| `Assets/Scenes/ScriptableObjects/Augments/` | 증강 데이터 |
| `Assets/Scenes/ScriptableObjects/Bosses/` | 보스 데이터 |
| `Docs/combat_balance.md` | 전투 밸런스 |
| `Docs/augment_balance.md` | 증강 목록 |

## 발표용

5분 시연 스크립트: [`PRESENTATION_DEMO.md`](PRESENTATION_DEMO.md)

## 제출 시 포함

- Unity 프로젝트 전체 (`.gitignore`에 `Library/` 제외)
- `README.md` (본 파일)
- `PRESENTATION_DEMO.md`
- 실행 영상 (선택): Play 모드 5분 녹화

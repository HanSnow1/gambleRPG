# Combat balance (Role A — v1 MVP)

Last updated: step 5 polish. Code source: `Assets/Scenes/Scripts/Combat/CombatBalance.cs`.

## Design goals

| Goal | Target |
|------|--------|
| Boss fight length | ~6–12 turns with mixed Safe/Risk (RNG) |
| Game over risk | Real — no bankruptcy protection |
| Button clarity | Bet, **boss-adjusted** success %, damage, fail penalty |
| Boss identity | Dice = higher success; Card = smaller bonus |

---

## Turn structure (v2)

Alternating turns: **Player attack → Enemy attack → repeat**

| Turn result | Player attack | Enemy attack |
|-------------|---------------|--------------|
| **Success** | Pay bet HP, deal full damage | Pay bet HP, deal full damage to player |
| **Miss** | Small chip damage only (3 / 5 / 8) | Small chip damage to self only |

Between normal stage clears: heal **25% of max HP** (`CombatBalance.BetweenFightHealPercent`).

---

## Base stats

| Unit | HP | Notes |
|------|-----|--------|
| Player | 100 / 100 | Fixed at combat start (relics = Role C later) |
| Normal enemy | 52 | Risk 2–3 turns; 1–2 fails → ~5–25 HP left |
| Boss (Dice / Card) | 120 | `BossDefinition.maxHp` |

Rules:
- **Success**: attacker pays bet HP, deals hit damage.
- **Miss**: attacker takes chip damage only (no bet HP lost).
- Bet is clamped to current attacker HP.

---

## Bet actions (3 buttons)

| Action | Bet | Base success | Hit | Miss chip | Code |
|--------|-----|--------------|-----|-----------|------|
| Safe | 10 | 60% | 20 | 3 | `CombatBalance.Safe` |
| Risk | 18 | 45% | 46 | 5 | `CombatBalance.Risk` |
| All-in | 25 | 30% | 72 | 8 | `CombatBalance.AllIn` |

### Boss success bonus (applied in combat + shown on buttons)

| Boss type | Bonus | Example (Safe 60%) |
|-----------|-------|---------------------|
| Dice | +5% | 65% |
| Card | +3% | 63% |

---

## Expected value (per turn, math)

Formula:
- **Player HP loss** ≈ `bet + (1 - p) × failExtra`
- **Enemy damage** ≈ `p × damage`  
  (`p` = success chance after boss bonus)

### vs 120 HP boss

| Action | p (Dice) | E player loss | E enemy dmg | Turns if always hit |
|--------|----------|---------------|-------------|---------------------|
| Safe | 65% | 12.1 | 13.0 | 6 |
| Risk | 50% | 24.0 | 23.0 | 3 |
| All-in | 35% | 38.0 | 25.2 | 2 |

| Action | p (Card) | E player loss | E enemy dmg |
|--------|----------|---------------|-------------|
| Safe | 63% | 12.5 | 12.6 |
| Risk | 48% | 24.2 | 22.1 |
| All-in | 33% | 38.4 | 23.8 |

**Play feel:** Safe is sustainable but slow on 120 HP. Risk is the default “kill boss” line. All-in is spike damage but dangerous over many turns.

---

## Tuning knobs (change here first)

1. `CombatBalance.cs` — bet table + boss bonuses  
2. `BossDefinition.maxHp` — per boss asset (120 now)  
3. `CombatUI` Inspector — overrides for quick playtests only  

Do **not** duplicate numbers in multiple scripts; keep `CombatBalance` as source of truth.

---

## Role B hook (augments)

Success chance pipeline:

```
baseChance → BossGambleResolvers.ModifySuccessChance → (future) AugmentCombatModifiers
```

Call from `CombatUI.DoBet` after augment layer exists.

---

## Playtest checklist (step 5)

- [ ] Preview → Start Fight → buttons show **65% / 50% / 35%** vs Dice boss (not 60/45/30)
- [ ] Status line shows `Boss rule: Dice (+5% success)` during fight
- [ ] Boss 120 HP: winnable with mostly Risk in ~8–15 turns (RNG)
- [ ] All-in spam → Game Over before boss dies (intended)
- [ ] Victory / Game Over → bet buttons disabled, Restart works
- [ ] Card boss: buttons show +3% vs base table

---

## Boss patterns (minigame layer)

Each boss fight randomly picks **Pattern A or B** at combat start. A gamble board HUD shows live state.

### Dice Tyrant (Floor 1)

| Pattern | Board | Player counter |
|---------|-------|------------------|
| **A — Stolen Number** | Faces 1☠ 2☠ fixed | Avoid Safe (−15%); use Risk/All-in; Extra Pip helps |
| **B — Curse of Sixes** | Random curse on 1 or 6 | Read curse; avoid All-in if 6☠; boss prefers All-in (+5%) |

Dice roll each player attack: need Safe 4+ / Risk 5+ / All-in 6+; cursed face −20%, meet need +12%.

**Enrage (50% HP):** Pattern B adds curse on face 1.

### Queen Of Cards (Floor 2)

| Pattern | Board | Player counter |
|---------|-------|------------------|
| **A — Queen's Mark** | ♛ on one bet slot, rotates every 3 player turns | Avoid ♛ slot; Risk miss +3 chip |
| **B — Joker Shuffle** | Joker in deck | First miss +8 chip; open Safe 1–2 turns |

**Enrage (50% HP):** Pattern B shuffles a **second Joker**.

### Enemy damage scaling

Boss successful attacks scale by `BossDefinition.baseAttack` vs Risk baseline (12 → Risk damage 46).

### Playtest checklist (boss patterns)

- [ ] Boss preview shows Pattern A/B hints from pattern assets
- [ ] Combat start logs pattern name, narration, and counter tip
- [ ] Gamble board panel visible during boss fights only
- [ ] Dice board shows ☠ faces; roll logged each player turn
- [ ] Card board shows ♛ mark; mark moves every 3 player turns (Pattern A)
- [ ] Joker triggers once on first miss (+8); second Joker after 50% HP (Pattern B)
- [ ] Boss enrage message at 50% HP
- [ ] Bet buttons show ☠ or ♛ on risky options

---

## Future (post-MVP)

- Normal fights between augments on map  
- Difficulty tiers / ascension  

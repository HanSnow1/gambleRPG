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

## Base stats

| Unit | HP | Notes |
|------|-----|--------|
| Player | 100 / 100 | Fixed at combat start (relics = Role C later) |
| Normal enemy | 80 | `BettingCombatSystem.startingEnemyHp` when no boss |
| Boss (Dice / Card) | 120 | `BossDefinition.maxHp` |

Rules:
- Bet HP is paid **before** the roll.
- On fail: lose bet + `extraFailDamage`.
- Bet is clamped to current player HP (cannot bet more than you have).

---

## Bet actions (3 buttons)

| Action | Bet | Base success | Damage | Fail + | Code |
|--------|-----|--------------|--------|--------|------|
| Safe | 10 | 60% | 20 | 6 | `CombatBalance.Safe` |
| Risk | 18 | 45% | 46 | 12 | `CombatBalance.Risk` |
| All-in | 25 | 30% | 72 | 20 | `CombatBalance.AllIn` |

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

## Future (post-MVP)

- Normal fights (80 HP) between augments on map  
- Boss `baseAttack` when enemy attacks back (not implemented)  
- Difficulty tiers / ascension  

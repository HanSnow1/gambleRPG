# Role A — Step 5: Balance & polish

## What changed in code

| File | Purpose |
|------|---------|
| `Combat/CombatBalance.cs` | All MVP combat numbers in one place |
| `CombatBetOption.FormatButtonLabel(boss)` | Buttons show boss-adjusted % |
| `CombatUI` | Status shows boss rule; buttons refresh each turn |
| `Docs/combat_balance.md` | Full table + EV notes for team / report |

## Quick verify in Unity

1. Play → **START FIGHT** vs Dice boss.  
2. Bet buttons should show **65% / 50% / 35%** (not base 60/45/30).  
3. Top status: `CHOOSE YOUR BET` + Dice rule line.  
4. Fight mostly with **Risk** — boss should die before you hit 0 HP most runs.

## If boss feels too hard / too easy

1. Open `CombatBalance.cs` — tweak damage or bet costs.  
2. Or lower `Boss_Dice` / `Boss_Card` asset **Max Hp** (e.g. 120 → 100).  
3. Re-test 3 runs; note changes in `combat_balance.md`.

## For teammates

- **B:** Read “Role B hook” in `combat_balance.md` before adding augment % modifiers.  
- **C:** Normal enemies can use 80 HP default; boss uses `BossDefinition`.  

## Role A checklist (assignment)

- [x] Central balance file  
- [x] Boss-adjusted UI labels  
- [x] Balance doc with EV + playtest list  
- [ ] Git push to `feature/combat-boss` (when ready)  

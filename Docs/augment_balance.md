# Augment balance — v1 (7 cards)

Legend: `[실]` Silver · `[골]` Gold · `[플]` Prismatic · `+` = max level-up picks (then removed from pool)

| # | ID | Card | Tier | Max Lv | Effect (summary) |
|---|-----|------|------|--------|------------------|
| 1 | aug_gilded_mirror | Gilded Mirror | [골] (+[플] pool) | 2 (+) | Random relic stat ×2 (+/-). No relic → grant 1 random common relic. |
| 2 | aug_anchor_point | Anchor Point | [실][골] | 3 (+++) | This floor: +30% max HP → +40% → +50% (+10% per re-pick). |
| 3 | aug_double_down | Double Down | [골] | 3 (+++) | Gamble: 30→40→50% chance deal/take damage ×2; gamble weight +10% per level. |
| 4 | aug_crown_upgrade | Crown Upgrade | [실][골] | 2 (+) | Next augment draft +1 tier (×1 or ×2 charges at Lv2). |
| 5 | aug_second_wind | Second Wind | [플] | 2 (+) | Cheat death ×1→×2: negate lethal, heal 30% max HP. |
| 6 | aug_last_chip | Last Chip | [골] | 2 (+) | Cheat death ×1→×2: survive at 1 HP (no heal). |
| 7 | aug_loaded_dice | Loaded Dice | [골] | 3 (+++) | 30% chance: +15% → +22% → +30% bonus damage. |

Assets: `Assets/Scenes/ScriptableObjects/Augments/Augment_*.asset`  
Data: `Assets/Scenes/Scripts/Data/AugmentDefinition.cs`

## Role B hooks (not wired yet)

- `AugmentCombatModifiers` — #3, #7 in `CombatUI.DoBet`
- `PlayerAugmentState` — owned list + level
- Relic system (C) — #1 double-stat / fallback relic

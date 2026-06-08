# Role A — Step 1: Combat polish (done in code)

## What changed

- `CombatBetOption.cs` — bet stats in one place (Inspector editable)
- `CombatUI.cs` — button labels auto-filled with bet/damage/chance, combat end disables buttons
- `BettingCombatSystem.cs` — `IsCombatActive`, events when fight ends
- `Docs/combat_balance.md` — numbers for team / presentation

## Unity setup (5 min)

1. Open `Main.unity`, select **Canvas** → **Combat UI** component.
2. (Optional) Drag **BetSmall**, **BetMedium**, **BetLarge** into the three button slots — or leave empty (auto-find by name).
3. (Optional) Add **UI → Text - TMP** named `StatusText` at top center, assign to **Status Text**.
4. **Restart button (required for restart feature):**
   - Canvas right-click → **UI → Button - TextMeshPro**
   - Rename to **`RestartButton`** (exact name — code auto-finds it)
   - Child text: `Restart`
   - Position: Pos X `0`, Pos Y `-180`
   - **Either** drag it to Combat UI → **Restart Button** slot
   - **Or** Button → On Click → `+` → drag **Canvas** → **CombatUI → OnRestartCombat**
   - Do **not** leave On Click empty if you rely on Inspector wiring only
5. Select **PlayerHpText** / **EnemyHpText** — set default text empty or leave (code updates on Play).
6. **Save scene**, press **Play**, test checklist in `combat_balance.md`.

## Layout reference (already in scene)

| Object        | Pos X | Pos Y |
|---------------|-------|-------|
| PlayerHpText  | 0     | 150   |
| EnemyHpText   | 0     | 100   |
| CombatLogText | 0     | -100  |
| BetSmall      | -200  | -20   |
| BetMedium     | 0     | -20   |
| BetLarge      | 200   | -20   |

## Next (Step 2 for A)

- `BossDefinition` ScriptableObject + 2 boss assets (Dice / Card)

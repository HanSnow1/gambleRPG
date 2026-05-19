# Role A — Step 4: Boss preview UI

## What you get

- Play starts on **BOSS PREVIEW** (name, rule, weakness, patterns).
- **START FIGHT** hides preview and opens the betting combat UI.
- **Preview Dice / Preview Card** swap the boss (test until RunManager picks one).
- Role C later calls `BossPreviewUI.ShowPreview(boss)` from `RunManager`.

## Unity setup (about 3 minutes)

1. Open `Main.unity`.
2. Select **Canvas** in Hierarchy.
3. **Add Component** → search `Boss Preview UI` → add it.
4. In **Boss Preview UI**:
   - **Combat UI**: drag Canvas (same object) or leave empty (auto-find).
   - **Default Boss**: `Boss_Crad` (Queen Of Cards) or your run boss.
   - **Dice Boss**: `Assets/Scenes/ScriptableObjects/Bosses/Boss_Dice`
   - **Card Boss**: `Assets/Scenes/ScriptableObjects/Bosses/Boss_Crad`
5. **Ctrl+S** save scene, press **Play**.

You do **not** need to build the panel by hand — if **Preview Panel** is empty, the script creates `BossPreviewPanel` at runtime.

## Expected flow

1. Play → dark preview screen, boss info, three buttons.
2. **Preview Dice** / **Preview Card** → text updates.
3. **START FIGHT** → combat HUD + bet buttons appear.
4. Fight as before; **Restart** replays the same boss (no preview).

## Optional polish (manual UI)

Create an empty under Canvas named `BossPreviewPanel`, add your own TMP texts and buttons with these **exact names**:

- `PreviewTitleText`, `PreviewBossNameText`, `PreviewRuleText`, `PreviewHintsText`
- `StartFightButton`, `PreviewDiceButton`, `PreviewCardButton`

Assign the panel to **Preview Panel** and wire text/button slots in Inspector.

## For Role C (integration)

```csharp
// After RunPlanner picks boss:
bossPreviewUI.ShowPreview(runState.boss);
```

Do not call `CombatUI.BeginCombat` directly from C — use preview + Start Fight, or call `OnStartFight()` from code if you skip the button.

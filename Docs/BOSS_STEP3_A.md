# Role A — Step 3: Boss rules in combat

## Unity setup (5 min)

1. Open `Main.unity`, select **Canvas** → **Combat UI**.
2. **Test Boss** slot: drag `Boss_Dice` or `Boss_Card` from
   `Assets/Scenes/ScriptableObjects/Bosses/`.
3. (Optional) Add **Text - TMP** named `BossRuleText` under Canvas (y ~ 120),
   assign to **Boss Rule Text**.
4. Play: enemy name should match boss, log shows boss type, success chance slightly higher.

## Switch boss for test

- Change **Test Boss** asset in Inspector, press Play again, or Restart.
- Or call `SetTestBoss` from a future preview UI.

## Optional rename

- `Boss_Crad.asset` → rename to `Boss_Card.asset` in Project window (typo fix).

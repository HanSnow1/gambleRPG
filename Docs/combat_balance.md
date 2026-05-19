# Combat balance (Role A)

Player start: 100 / 100 HP  
Enemy start: 80 HP  
No bankruptcy protection — HP 0 = Game Over.

| Action | Bet HP | Success | Damage | Fail extra |
|--------|--------|---------|--------|------------|
| Safe   | 10     | 60%     | 20     | 6          |
| Risk   | 18     | 45%     | 46     | 12         |
| All-in | 25     | 30%     | 72     | 20         |

Notes:
- Bet is paid at start of turn (deducted before roll).
- On fail, extra HP loss is applied in addition to the bet.
- Max bet per turn = current player HP (clamped in code).

Test checklist:
- [ ] Play: buttons show Safe / Risk / All-in with stats
- [ ] Victory: enemy HP 0, bet buttons disabled, status VICTORY
- [ ] Game Over: player HP 0, buttons disabled, status GAME OVER
- [ ] Restart (if Restart button wired): full HP reset

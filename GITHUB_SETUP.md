# GitHub Upload Guide (GambleRogue)

## Quick start (recommended)

1. Open **PowerShell** in this folder (`My project`).
2. Run:

```powershell
.\scripts\push_to_github.ps1
```

3. Follow prompts. For first-time GitHub CLI:

```powershell
winget install GitHub.cli
gh auth login
```

Then run the script again and choose `y` for `gh repo create`.

---

## Manual steps

### 1. Already done in repo

- `.gitignore` (excludes `Library/`, `Temp/`, `Logs/`, etc.)
- `TEAM_ROLES.txt` (team task split)

### 2. Local git

```powershell
cd "C:\Users\may12\gambleRPG\My project"
git init
git add .
git status
```

Confirm **Library/** does NOT appear in the list.

```powershell
git commit -m "Initial commit: combat prototype and team roles doc"
```

### 3. Create GitHub repo (Public)

1. Go to https://github.com/new
2. Repository name: `GambleRogue`
3. Visibility: **Public**
4. Do **not** check README, .gitignore, or license
5. Click **Create repository**

### 4. Push

Replace `YOUR_USERNAME`:

```powershell
git branch -M main
git remote add origin https://github.com/YOUR_USERNAME/GambleRogue.git
git push -u origin main
```

### 5. Invite teammates

Repo **Settings** -> **Collaborators** -> **Add people**

Use branches from `TEAM_ROLES.txt`:

- `feature/combat-boss`
- `feature/augment`
- `feature/run-map`

---

## Clone (for teammates)

```powershell
git clone https://github.com/YOUR_USERNAME/GambleRogue.git
```

Open the project folder in **Unity Hub** (first open may take a few minutes while `Library/` is rebuilt).

---

## Troubleshooting

| Problem | Fix |
|---------|-----|
| `Library/` uploaded | Ensure `.gitignore` exists; `git rm -r --cached Library` then commit |
| Push rejected | Remote has README; use empty repo or `git pull --rebase origin main` |
| Auth failed | Use PAT or `gh auth login` |
| Path with spaces | Always quote: `cd "C:\...\My project"` |

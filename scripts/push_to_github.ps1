# GambleRogue - Git init and push to GitHub (Public)
# Run in PowerShell: right-click -> Run with PowerShell
# Or: cd to project root, then: .\scripts\push_to_github.ps1

$ErrorActionPreference = "Stop"
$ProjectRoot = Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path)
Set-Location $ProjectRoot

Write-Host "Project: $ProjectRoot" -ForegroundColor Cyan

# Check git
git --version
if ($LASTEXITCODE -ne 0) {
    Write-Host "Git is not installed. Install from https://git-scm.com/" -ForegroundColor Red
    exit 1
}

# Init if needed
if (-not (Test-Path ".git")) {
    git init
    Write-Host "Created new git repository." -ForegroundColor Green
}

# Stage and verify Library is ignored
git add .
$libraryStaged = git status --short | Select-String -Pattern "Library"
if ($libraryStaged) {
    Write-Host "WARNING: Library/ appears staged. Check .gitignore!" -ForegroundColor Yellow
    $libraryStaged
} else {
    Write-Host "OK: Library/ is not staged." -ForegroundColor Green
}

git status --short | Select-Object -First 30

# Commit
$hasCommits = git rev-parse HEAD 2>$null
if (-not $hasCommits) {
    git commit -m "Initial commit: combat prototype and team roles doc"
} else {
    $status = git status --porcelain
    if ($status) {
        git commit -m "Update project files"
    } else {
        Write-Host "Nothing to commit." -ForegroundColor Yellow
    }
}

# GitHub repo name (change if you want)
$RepoName = "GambleRogue"

Write-Host ""
Write-Host "=== GitHub push ===" -ForegroundColor Cyan
Write-Host "Option A: If you have GitHub CLI (gh):"
Write-Host "  gh auth login"
Write-Host "  gh repo create $RepoName --public --source=. --remote=origin --push"
Write-Host ""
Write-Host "Option B: Manual:"
Write-Host "  1. Create empty PUBLIC repo at https://github.com/new named: $RepoName"
Write-Host "  2. Do NOT add README or .gitignore on GitHub"
Write-Host "  3. Run (replace YOUR_USERNAME):"
Write-Host "     git branch -M main"
Write-Host "     git remote add origin https://github.com/YOUR_USERNAME/$RepoName.git"
Write-Host "     git push -u origin main"
Write-Host ""

$useGh = Read-Host "Run 'gh repo create' now? (y/n)"
if ($useGh -eq "y" -or $useGh -eq "Y") {
    gh auth status 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Run: gh auth login" -ForegroundColor Yellow
        exit 1
    }
    git branch -M main
    gh repo create $RepoName --public --source=. --remote=origin --push
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Done! Share repo URL with teammates." -ForegroundColor Green
        gh repo view --web
    }
}

Write-Host "Press Enter to close..."
Read-Host

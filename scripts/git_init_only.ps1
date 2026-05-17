$ErrorActionPreference = "Stop"
Set-Location (Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path))

if (-not (Test-Path ".git")) { git init }
git add .
$lib = git status --short | Select-String "Library"
if ($lib) { Write-Error "Library is staged - fix .gitignore first"; exit 1 }
if (-not (git rev-parse HEAD 2>$null)) {
    git commit -m "Initial commit: combat prototype and team roles doc"
    Write-Host "Commit OK. Files:" (git show --stat --oneline HEAD)
} else {
    $s = git status --porcelain
    if ($s) { git commit -m "Update project files"; Write-Host "Commit OK." }
    else { Write-Host "Already committed, working tree clean." }
}

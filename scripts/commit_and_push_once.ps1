$ErrorActionPreference = "Continue"
$ProjectRoot = Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path)
$LogFile = Join-Path $ProjectRoot "push_result.txt"
Set-Location $ProjectRoot

function Log($msg) {
  $line = "$(Get-Date -Format 'HH:mm:ss') $msg"
  Add-Content -Path $LogFile -Value $line -Encoding UTF8
}

"" | Set-Content -Path $LogFile -Encoding UTF8
Log "Project: $ProjectRoot"

Log "=== git status ==="
git status 2>&1 | ForEach-Object { Log $_ }

Log "=== git diff --stat ==="
git diff --stat 2>&1 | ForEach-Object { Log $_ }

Log "=== git add -A ==="
git add -A 2>&1 | ForEach-Object { Log $_ }

Log "=== git status after add ==="
git status --short 2>&1 | ForEach-Object { Log $_ }

$porcelain = git status --porcelain 2>&1
if ($porcelain) {
  Log "=== git commit ==="
  git commit -m "Add boss preview, combat balance, and UI polish" -m "Complete Role A combat MVP: boss preview flow, balance centralization, boss-adjusted bet labels, and Main scene UI tweaks." 2>&1 | ForEach-Object { Log $_ }
} else {
  Log "Nothing to commit."
}

$branch = git rev-parse --abbrev-ref HEAD 2>&1
$remote = git remote get-url origin 2>&1
$hash = git rev-parse HEAD 2>&1
Log "branch: $branch"
Log "remote: $remote"
Log "commit: $hash"

Log "=== git push ==="
$upstream = git rev-parse --abbrev-ref --symbolic-full-name "@{u}" 2>$null
if (-not $upstream) {
  git push -u origin HEAD 2>&1 | ForEach-Object { Log $_ }
} else {
  git push 2>&1 | ForEach-Object { Log $_ }
}
Log "push exit: $LASTEXITCODE"
Log "DONE"

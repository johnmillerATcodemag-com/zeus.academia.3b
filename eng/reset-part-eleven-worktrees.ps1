param(
  [string]$RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path,
  [string]$BaseBranch = "Part-Eleven",
  [string]$Remote = "origin"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Invoke-Git {
  param(
    [string[]]$Arguments,
    [switch]$AllowFailure
  )

  $previousErrorActionPreference = $ErrorActionPreference
  $ErrorActionPreference = "Continue"
  try {
    $output = & git -C $RepoRoot @Arguments 2>&1
    $gitExitCode = $LASTEXITCODE
  }
  finally {
    $ErrorActionPreference = $previousErrorActionPreference
  }

  if ($gitExitCode -ne 0 -and -not $AllowFailure) {
    throw "git $($Arguments -join ' ') failed.`n$output"
  }

  return $output
}

function Get-RepositorySlug {
  $remoteUrl = (Invoke-Git -Arguments @("config", "--get", "remote.$Remote.url")).Trim()

  if ($remoteUrl -match '^git@github\.com:(?<owner>[^/]+)/(?<repo>.+?)(\.git)?$') {
    return "$($Matches.owner)/$($Matches.repo)"
  }

  if ($remoteUrl -match '^https://github\.com/(?<owner>[^/]+)/(?<repo>.+?)(\.git)?$') {
    return "$($Matches.owner)/$($Matches.repo)"
  }

  throw "Unable to parse GitHub repository from remote URL: $remoteUrl"
}

function Test-LocalBranch {
  param([string]$Branch)

  $previousErrorActionPreference = $ErrorActionPreference
  $ErrorActionPreference = "Continue"
  try {
    $null = & git -C $RepoRoot rev-parse --verify "refs/heads/$Branch" 2>$null
    $gitExitCode = $LASTEXITCODE
  }
  finally {
    $ErrorActionPreference = $previousErrorActionPreference
  }

  return $gitExitCode -eq 0
}

function Test-RemoteBranch {
  param([string]$Branch)

  $previousErrorActionPreference = $ErrorActionPreference
  $ErrorActionPreference = "Continue"
  try {
    $null = & git -C $RepoRoot rev-parse --verify "refs/remotes/$Remote/$Branch" 2>$null
    $gitExitCode = $LASTEXITCODE
  }
  finally {
    $ErrorActionPreference = $previousErrorActionPreference
  }

  return $gitExitCode -eq 0
}

function Close-PullRequestForBranch {
  param(
    [string]$Repository,
    [string]$Branch
  )

  $prNumber = (& gh pr list --repo $Repository --state open --head $Branch --json number --jq '.[0].number' 2>$null).Trim()

  if ([string]::IsNullOrWhiteSpace($prNumber)) {
    Write-Host "No open PR found for branch '$Branch'." -ForegroundColor Yellow
    return
  }

  Write-Host "Closing PR #$prNumber for branch '$Branch'..." -ForegroundColor Cyan
  & gh pr close $prNumber --repo $Repository | Out-Null

  if ($LASTEXITCODE -ne 0) {
    throw "Failed to close PR #$prNumber for branch '$Branch'."
  }
}

function Remove-BranchAndWorktree {
  param(
    [string]$Branch,
    [string]$WorktreePath
  )

  if (Test-Path -LiteralPath $WorktreePath) {
    Write-Host "Removing worktree '$WorktreePath'..." -ForegroundColor Cyan
    Invoke-Git -Arguments @("worktree", "remove", "--force", $WorktreePath) | Out-Null
  }
  else {
    Write-Host "Worktree not found: $WorktreePath" -ForegroundColor Yellow
  }

  if (Test-LocalBranch -Branch $Branch) {
    Write-Host "Deleting local branch '$Branch'..." -ForegroundColor Cyan
    Invoke-Git -Arguments @("branch", "-D", $Branch) | Out-Null
  }
  else {
    Write-Host "Local branch not found: $Branch" -ForegroundColor Yellow
  }

  if (Test-RemoteBranch -Branch $Branch) {
    Write-Host "Deleting remote branch '$Remote/$Branch'..." -ForegroundColor Cyan
    Invoke-Git -Arguments @("push", $Remote, "--delete", $Branch) | Out-Null
  }
  else {
    Write-Host "Remote branch not found: $Remote/$Branch" -ForegroundColor Yellow
  }
}

function New-BranchWorktree {
  param(
    [string]$Branch,
    [string]$WorktreePath,
    [string]$StartPoint
  )

  if (Test-Path -LiteralPath $WorktreePath) {
    throw "Cannot create worktree. Path already exists: $WorktreePath"
  }

  Write-Host "Creating branch '$Branch' from '$StartPoint' at '$WorktreePath'..." -ForegroundColor Green
  Invoke-Git -Arguments @("worktree", "add", $WorktreePath, "-b", $Branch, $StartPoint) | Out-Null
}

Write-Host "Fetching latest refs from '$Remote'..." -ForegroundColor Cyan
Invoke-Git -Arguments @("fetch", $Remote, "--prune") | Out-Null

$repoSlug = Get-RepositorySlug
$worktreeParent = "$(Split-Path -Path $RepoRoot -Parent)\$(Split-Path -Path $RepoRoot -Leaf).worktrees"
$targets = @(
  @{
    Branch       = "Part-Eleven-ManageUniversities"
    WorktreePath = Join-Path $worktreeParent "Part-Eleven-ManageUniversities"
  },
  @{
    Branch       = "Part-Eleven-ProvisionExtensions"
    WorktreePath = Join-Path $worktreeParent "Part-Eleven-ProvisionExtensions"
  }
)

$startPoint = if (Test-LocalBranch -Branch $BaseBranch) {
  $BaseBranch
}
elseif (Test-RemoteBranch -Branch $BaseBranch) {
  "$Remote/$BaseBranch"
}
else {
  throw "Base branch '$BaseBranch' not found locally or on '$Remote'."
}

foreach ($target in $targets) {
  Close-PullRequestForBranch -Repository $repoSlug -Branch $target.Branch
}

foreach ($target in $targets) {
  Remove-BranchAndWorktree -Branch $target.Branch -WorktreePath $target.WorktreePath
}

foreach ($target in $targets) {
  New-BranchWorktree -Branch $target.Branch -WorktreePath $target.WorktreePath -StartPoint $startPoint
}

Write-Host "Done. Worktrees and branches have been reset for Part Eleven slices." -ForegroundColor Green

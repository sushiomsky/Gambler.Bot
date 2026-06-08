param(
    [Parameter(Mandatory = $true)]
    [string]$HookName,

    [Parameter(ValueFromRemainingArguments = $true)]
    [string[]]$Arguments = @()
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Get-RepoRoot {
    return (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
}

function Get-VersionFile {
    return Join-Path (Get-RepoRoot) 'Directory.Build.props'
}

function Get-VersionPrefix {
    [xml]$project = Get-Content (Get-VersionFile) -Raw
    $node = $project.Project.PropertyGroup.VersionPrefix | Select-Object -First 1
    if (-not $node) {
        throw 'VersionPrefix was not found in Directory.Build.props.'
    }

    return $node.InnerText.Trim()
}

function Set-VersionPrefix {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Version
    )

    [xml]$project = Get-Content (Get-VersionFile) -Raw
    $node = $project.Project.PropertyGroup.VersionPrefix | Select-Object -First 1
    if (-not $node) {
        throw 'VersionPrefix was not found in Directory.Build.props.'
    }

    $node.InnerText = $Version
    $project.Save((Get-VersionFile))
}

function Get-NextVersion {
    param(
        [Parameter(Mandatory = $true)]
        [string]$CurrentVersion,

        [string]$ReleaseToken = ''
    )

    if ($ReleaseToken) {
        $token = $ReleaseToken.Trim().ToLowerInvariant()
        if ($token -match '^(v)?\d+\.\d+\.\d+$') {
            return $token.TrimStart('v')
        }

        if ($token -eq 'major' -or $token -eq 'minor' -or $token -eq 'patch') {
            $parts = $CurrentVersion.Split('.')
            if ($parts.Count -lt 3) {
                throw "Current version '$CurrentVersion' is not in major.minor.patch format."
            }

            $major = [int]$parts[0]
            $minor = [int]$parts[1]
            $patch = [int]$parts[2]

            switch ($token) {
                'major' {
                    $major++
                    $minor = 0
                    $patch = 0
                }
                'minor' {
                    $minor++
                    $patch = 0
                }
                default {
                    $patch++
                }
            }

            return "$major.$minor.$patch"
        }
    }

    $parts = $CurrentVersion.Split('.')
    if ($parts.Count -lt 3) {
        throw "Current version '$CurrentVersion' is not in major.minor.patch format."
    }

    $major = [int]$parts[0]
    $minor = [int]$parts[1]
    $patch = [int]$parts[2]
    $patch++
    return "$major.$minor.$patch"
}

function Ensure-ReleaseTag {
    param(
        [Parameter(Mandatory = $true)]
        [string]$TagName
    )

    $existingTag = & git tag --list $TagName
    if ($LASTEXITCODE -ne 0) {
        throw 'Failed to inspect existing git tags.'
    }

    if (-not $existingTag) {
        & git tag -a $TagName -m "Release $TagName" HEAD
        if ($LASTEXITCODE -ne 0) {
            throw "Failed to create tag $TagName."
        }
    }
}

function Push-Release {
    param(
        [Parameter(Mandatory = $true)]
        [string]$TagName
    )

    & git push origin HEAD --follow-tags
    if ($LASTEXITCODE -ne 0) {
        throw 'Failed to push release commit and tags.'
    }

    Publish-GitHubRelease -TagName $TagName
}

function Publish-GitHubRelease {
    param(
        [Parameter(Mandatory = $true)]
        [string]$TagName
    )

    $token = $env:GITHUB_TOKEN
    if (-not $token) {
        $token = $env:GH_TOKEN
    }

    if (-not $token) {
        Write-Host "No GitHub token found. Tag $TagName was pushed, but the GitHub release was not created."
        return
    }

    $remoteUrl = (& git remote get-url origin).Trim()
    if ($LASTEXITCODE -ne 0 -or -not $remoteUrl) {
        Write-Host 'Unable to read the origin remote, so the GitHub release was skipped.'
        return
    }

    if ($remoteUrl -notmatch 'github\.com[:/](?<owner>[^/]+)/(?<repo>[^/.]+)') {
        Write-Host 'Origin is not a GitHub remote, so the GitHub release was skipped.'
        return
    }

    $owner = $Matches.owner
    $repo = $Matches.repo
    $apiBase = "https://api.github.com/repos/$owner/$repo"
    $headers = @{
        'Authorization' = "Bearer $token"
        'Accept' = 'application/vnd.github+json'
        'X-GitHub-Api-Version' = '2022-11-28'
        'User-Agent' = 'Gambler.Bot release hook'
    }

    try {
        Invoke-RestMethod -Method Get -Uri "$apiBase/releases/tags/$TagName" -Headers $headers | Out-Null
        Write-Host "GitHub release for $TagName already exists."
        return
    }
    catch {
        if ($_.Exception.Response -and $_.Exception.Response.StatusCode.value__ -ne 404) {
            throw
        }
    }

    $body = @{
        tag_name = $TagName
        target_commitish = 'HEAD'
        name = $TagName
        draft = $false
        prerelease = $false
    } | ConvertTo-Json -Depth 4

    Invoke-RestMethod -Method Post -Uri "$apiBase/releases" -Headers $headers -Body $body -ContentType 'application/json' | Out-Null
    Write-Host "Created GitHub release for $TagName."
}

switch ($HookName) {
    'prepare-commit-msg' {
        if (-not $Arguments -or $Arguments.Count -lt 1) {
            return
        }

        $messageFile = $Arguments[0]
        if (-not (Test-Path $messageFile)) {
            return
        }

        $content = Get-Content $messageFile -Raw
        if ($content -notmatch '^\s*release(?:\s+(?<token>[^\r\n]+))?\s*$') {
            return
        }

        $currentVersion = Get-VersionPrefix
        $releaseToken = $Matches.token
        $nextVersion = Get-NextVersion -CurrentVersion $currentVersion -ReleaseToken $releaseToken

        Set-VersionPrefix -Version $nextVersion
        & git add -- (Get-VersionFile)
        if ($LASTEXITCODE -ne 0) {
            throw 'Failed to stage the version file.'
        }

        $lines = $content -split "\r?\n", 2
        $lines[0] = "release v$nextVersion"
        Set-Content -Path $messageFile -Value ($lines -join "`n") -NoNewline
        Write-Host "Bumped version to v$nextVersion."
    }

    'post-commit' {
        $subject = (& git log -1 --pretty=%s).Trim()
        if ($LASTEXITCODE -ne 0 -or $subject -notmatch '^release v(?<version>\d+\.\d+\.\d+)$') {
            return
        }

        $tagName = "v$($Matches.version)"
        Ensure-ReleaseTag -TagName $tagName
        Push-Release -TagName $tagName
    }

    default {
        Write-Host "Unknown hook '$HookName'."
    }
}

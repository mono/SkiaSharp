Param(
    [switch] $UpdateBuildNumber
)

$ErrorActionPreference = 'Stop'

# Determine the feature name, if any
Write-Host "# Determining feature name..."
$featurePrefix = "refs/heads/$env:FEATURE_NAME_PREFIX"
if ("$env:BUILD_SOURCEBRANCH".StartsWith($featurePrefix)) {
    $feature = $env:BUILD_SOURCEBRANCH.Substring($featurePrefix.Length)
    Write-Host "Feature name: $feature"
    $env:FEATURE_NAME = $feature
    Write-Host "##vso[task.setvariable variable=FEATURE_NAME]$feature"
} else {
    Write-Host "No feature name."
}

# Update the PR variables for downstream builds
Write-Host "`n# Checking PR variables for downstream builds..."
if ($env:BUILD_REASON -eq "ResourceTrigger" -or $env:BUILD_REASON -eq "Manual") {
    $isPR = [regex]::Match("$env:BUILD_SOURCEBRANCH", 'refs\/pull\/(\d+)\/merge')
    if ($isPR) {
        if (-not $env:SYSTEM_PULLREQUEST_PULLREQUESTNUMBER) {
            $pr = $isPR.Groups[1].Value
            Write-Host "PR number: $pr"
            $env:SYSTEM_PULLREQUEST_PULLREQUESTNUMBER = $pr
            Write-Host "##vso[task.setvariable variable=SYSTEM_PULLREQUEST_PULLREQUESTNUMBER]$pr"
        }
        if (-not $env:SYSTEM_PULLREQUEST_SOURCECOMMITID) {
            $sha = [regex]::Match("$env:BUILD_SOURCEVERSIONMESSAGE", 'Merge (.+) into (.+)').Groups[1].Value
            Write-Host "PR SHA: $sha"
            $env:SYSTEM_PULLREQUEST_SOURCECOMMITID = $sha
            Write-Host "##vso[task.setvariable variable=SYSTEM_PULLREQUEST_SOURCECOMMITID]$sha"
        }
    } else {
        Write-Host "Not a PR build."
    }
} else {
    Write-Host "Not a downstream build."
}

# Handle preview labels based on build reason
Write-Host "`n# Setting preview label..."
if ($env:BUILD_REASON -eq "PullRequest") {
    # Use a special preview label for PRs
    $pr = "pr." + $env:SYSTEM_PULLREQUEST_PULLREQUESTNUMBER
    Write-Host "Preview label: $pr"
    $env:PREVIEW_LABEL = $pr
    Write-Host "##vso[task.setvariable variable=PREVIEW_LABEL]$pr"
} elseif ($env:BUILD_REASON -eq "Schedule") {
    # Use a special preview label for scheduled builds
    $nightly = "nightly"
    Write-Host "Preview label: $nightly"
    $env:PREVIEW_LABEL = $nightly
    Write-Host "##vso[task.setvariable variable=PREVIEW_LABEL]$nightly"
} else {
    Write-Host "No special preview label for this build reason: $env:BUILD_REASON."
}

# Override the preview label and build number if this is a secondary build
Write-Host "`n# Checking for secondary build information..."
if ($env:BUILD_REASON -eq "ResourceTrigger" -or $env:BUILD_REASON -eq "Manual") {
    Write-Host "Working with $env:RESOURCES_PIPELINE_SKIASHARP_RUNNAME"
    if ($env:RESOURCES_PIPELINE_SKIASHARP_RUNNAME) {
        $match = [regex]::Match("$env:RESOURCES_PIPELINE_SKIASHARP_RUNNAME", '.*\-(.+)\.(\d+)')
        $label = $match.Groups[1].Value
        Write-Host "Preview label: $label"
        $env:PREVIEW_LABEL = $label
        Write-Host "##vso[task.setvariable variable=PREVIEW_LABEL]$label"
        $buildnumber = $match.Groups[2].Value
        Write-Host "Build number: $buildnumber"
        $env:BUILD_NUMBER = $buildnumber
        Write-Host "##vso[task.setvariable variable=BUILD_NUMBER]$buildnumber"
    } else {
        Write-Host "Not a secondary build."
    }
} else {
    Write-Host "Not a secondary build for this build reason: $env:BUILD_REASON."
}

# Update the build number with a more readable one
Write-Host "`n# Setting build label..."
if ($UpdateBuildNumber) {
    $label = ""
    if ($env:RESOURCES_PIPELINE_SKIASHARP_RUNNAME) {
        $label = $env:RESOURCES_PIPELINE_SKIASHARP_RUNNAME
    } else {
        if ($env:BUILD_REASON -ne "PullRequest") {
            $label = "+" + $env:BUILD_SOURCEBRANCHNAME
        }
        $label = "$env:SKIASHARP_VERSION-$env:PREVIEW_LABEL.$env:BUILD_NUMBER$label"
    }
    Write-Host "Build label: $label"
    Write-Host "##vso[build.updatebuildnumber]$label"
} else {
    Write-Host "Skipping build number update."
}

exit $LASTEXITCODE
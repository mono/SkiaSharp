Param(
    [Parameter(Mandatory, ValueFromPipeline)]
    [ValidateNotNullOrEmpty()]
    [ScriptBlock] $ScriptBlock,
    [Int] $RetryCount = 3
)

Process {
    $Attempt = 0
    While ($Attempt -lt $RetryCount) {
        try {
            Invoke-Command -ScriptBlock $ScriptBlock
            if ($LASTEXITCODE -eq 0) {
                break;
            }
            throw "Script failed to execute."
        } catch {
            $Attempt = $Attempt + 1
            if ($Attempt -lt $RetryCount) {
                Write-Host "##vso[task.logissue type=warning] ($Attempt of $RetryCount) Script failed to execute, retrying..."
            } else {
                Write-Host "##vso[task.logissue type=warning] ($Attempt of $RetryCount) Script failed to execute."
                exit 1
            }
        }
    }
}

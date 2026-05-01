
# This was modified from StackOverflow 
# https://stackoverflow.com/questions/45574479/powershell-determine-new-url-of-a-permanently-moved-redirected-resource

Param (
    [Parameter(Mandatory, ValueFromPipeline)] [Uri] $Uri,
    [string] $OutFile,
    [int] $MaxRedirections = 50 # Use same default as [System.Net.HttpWebRequest]
)

process {
    $nextUri = $Uri
    $ultimateFound = $false
    foreach($i in 1..$($MaxRedirections+1)) {
        Write-Verbose "Examining: $nextUri"
        $request = [System.Net.HttpWebRequest]::Create($nextUri)
        $request.AllowAutoRedirect = $False
        try {
            $response = $request.GetResponse()
            $nextUriStr = $response.Headers['Location']
            $response.Close()
            if (-not $nextUriStr) {
                $ultimateFound = $true
                break
            }
        } catch [System.Net.WebException] {
            $nextUriStr = try { $_.Exception.Response.Headers['Location'] } catch {}
            if (-not $nextUriStr) { Throw }
        }
        Write-Verbose "Raw target: $nextUriStr"
        if ($nextUriStr -match '^https?:') {
            $nextUri = $prevUri = [Uri] $nextUriStr
        } else {
            $nextUri = $prevUri = [Uri] ($prevUri.Scheme + '://' + $prevUri.Authority + $nextUriStr)
        }
        if ($i -ge $MaxRedirections) {
            break
        }
    }
    if (-not $ultimateFound) {
        Throw "Enumeration of $Uri redirections ended before reaching the ultimate target."
    }
    Invoke-WebRequest -Uri $nextUri -OutFile $OutFile
}

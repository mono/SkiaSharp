param(
    [ValidateSet("linux", "windows", "")]
    [string]$Platform = ""
)

if ($Platform -eq "") {
    $Platform = if ($IsLinux -or $IsMacOS) { "linux" } else { "windows" }
}

$tag = "skiasharpsample/webapi"
$name = "skiasharpsample-webapi"

# we can only do --platform with Experimental=true
$buildPlatform = ""
$isExperimental = docker info -f "{{ json .ExperimentalBuild }}"
if ($isExperimental -eq "true") {
    $buildPlatform = "--platform=$Platform"
}

docker stop $name
docker rm $name

# build the container
docker build $buildPlatform --tag $tag --file "$Platform.Dockerfile" .

# run
docker run --detach --name $name --publish "80:80" $tag

# get the IP so we can talk
$ip = docker inspect -f "{{ .NetworkSettings.Networks.nat.IPAddress }}" $name
if (-not $ip) {
    $ip = docker inspect -f "{{ .NetworkSettings.IPAddress }}" $name
}

# get a response (retry on 404 as the server may take a second to start up)
try {
    $retry = 0
    do {
        $imageResponse = try { Invoke-WebRequest "http://$ip/api/images" } catch { $_.Exception.Response }

        # we were successful on 200 and > 0 content
        if ($imageResponse.StatusCode -eq 200) {
            if ($imageResponse.Content.Length -gt 0) {
                Write-Host "Success!"
                break
            } else {
                throw "Empty response."
            }
        }

        # errors
        if ($imageResponse -eq $null -or $imageResponse.StatusCode -ge 404) {
            # the server might not be up yet
            Write-Host "Retrying..."
            Start-Sleep 5
            $retry += 5
        } else {
            # some error occured
            throw "Error: $imageResponse.StatusDescription"
        }
    } while ($retry -lt 30)

    if ($retry -ge 30) {
        throw "Timed out."
    }
} finally {
    # clean up
    docker stop $name
    docker rm $name
}

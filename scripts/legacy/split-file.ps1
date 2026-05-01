Param(
    [string] $Path,
    [long] $ChunkSize = 1GB
)

$ErrorActionPreference = 'Stop'

if ($ChunkSize -le 0) {
    Write-Error "Only positive sizes allowed"
    exit 1
}

Write-Host "Splitting file $Path to parts of $ChunkSize byte size..."

[long] $bufferSize = 100MB
if ($bufferSize -gt $ChunkSize) {
    $bufferSize = $ChunkSize
}
$buffer = New-Object byte[] $bufferSize

$sourceFile = [System.IO.File]::OpenRead($Path)
$sourceReader = New-Object System.IO.BinaryReader($sourceFile)

[int] $index = 1
do {
    $filename = "$Path." + $index.ToString("000")
    Write-Host "Writing $filename..."

    $destFile = [System.IO.File]::Create($filename)
    $destWriter = New-Object System.IO.BinaryWriter($destFile)

    [long] $sizeSoFar = 0
    [long] $bytesRead = 0
    while ($sizeSoFar -lt $ChunkSize) {
        [long] $sizeToRead = $ChunkSize - $sizeSoFar
        if ($sizeToRead -gt $bufferSize) {
            $sizeToRead = $bufferSize
        }
        Write-Verbose "Reading $sizeToRead bytes..."
        $bytesRead = $sourceReader.Read($buffer, 0, $buffer.Length)
        Write-Verbose "Read $bytesRead bytes."
        if ($bytesRead -le 0) {
            break;
        }
        $sizeSoFar += $bytesRead
        $destWriter.Write($buffer, 0, $bytesRead)
    }
    $destWriter.Close()
    $destFile.Close()

    Write-Verbose "Wrote $bytesSoFar bytes to $filename."

    $index++
} while ($bytesRead -gt 0)

$sourceReader.Close()
$sourceFile.Close()

Write-Host "Split complete."

exit $LASTEXITCODE

<# https://msdn.microsoft.com/en-us/library/hh925568 #>

$winVer = (Get-WmiObject win32_operatingsystem).Version
$dotNetRegistry  = 'SOFTWARE\Microsoft\NET Framework Setup\NDP'
$dotNet4Registry = 'SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full'
$dotNet4Builds = @{
    '30319'  = @{ Version = [System.Version]'4.0'                                                     }
    '378389' = @{ Version = [System.Version]'4.5'                                                     }
    '378675' = @{ Version = [System.Version]'4.5.1'   ; Comment = '(8.1/2012R2)'                      }
    '378758' = @{ Version = [System.Version]'4.5.1'   ; Comment = '(8/7 SP1/Vista SP2)'               }
    '379893' = @{ Version = [System.Version]'4.5.2'                                                   }
    '380042' = @{ Version = [System.Version]'4.5'     ; Comment = 'and later with KB3168275 rollup'   }
    '393295' = @{ Version = [System.Version]'4.6'     ; Comment = '(Windows 10)'                      }
    '393297' = @{ Version = [System.Version]'4.6'     ; Comment = '(NON Windows 10)'                  }
    '394254' = @{ Version = [System.Version]'4.6.1'   ; Comment = '(Windows 10)'                      }
    '394271' = @{ Version = [System.Version]'4.6.1'   ; Comment = '(NON Windows 10)'                  }
    '394802' = @{ Version = [System.Version]'4.6.2'   ; Comment = '(Windows 10 1607)'                 }
    '394806' = @{ Version = [System.Version]'4.6.2'   ; Comment = '(NON Windows 10)'                  }
    '460798' = @{ Version = [System.Version]'4.7'     ; Comment = '(Windows 10 1703)'                 }
    '460805' = @{ Version = [System.Version]'4.7'     ; Comment = '(NON Windows 10)'                  }
    '461308' = @{ Version = [System.Version]'4.7.1'   ; Comment = '(Windows 10 1709)'                 }
    '461310' = @{ Version = [System.Version]'4.7.1'   ; Comment = '(NON Windows 10)'                  }
    '461808' = @{ Version = [System.Version]'4.7.2'   ; Comment = '(Windows 10 1803)'                 }
    '461814' = @{ Version = [System.Version]'4.7.2'   ; Comment = '(NON Windows 10)'                  }
}

Write-Host
Write-Host "Running Windows version $winVer..."

if($regKey = [Microsoft.Win32.Registry]::LocalMachine) {
    if ($netRegKey = $regKey.OpenSubKey("$dotNetRegistry")) {
        foreach ($versionKeyName in $netRegKey.GetSubKeyNames()) {
            if ($versionKeyName -match '^v[123]') {
                $versionKey = $netRegKey.OpenSubKey($versionKeyName)
                $version = [System.Version]($versionKey.GetValue('Version', ''))
                New-Object -TypeName PSObject -Property ([ordered]@{
                    Build = $version.Build
                    Version = $version
                    Comment = ''
                })
            }
        }
    }

    if ($net4RegKey = $regKey.OpenSubKey("$dotNet4Registry")) {
        if(-not ($net4Release = $net4RegKey.GetValue('Release'))) {
            $net4Release = 30319
        }

        New-Object -TypeName PSObject -Property ([ordered]@{
            Build = $net4Release
            Version = $dotNet4Builds["$net4Release"].Version
            Comment = $dotNet4Builds["$net4Release"].Comment
        })
    }
}

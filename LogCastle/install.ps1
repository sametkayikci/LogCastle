param($installPath, $toolsPath, $package, $project)

$jsonFilePath = Join-Path $project.FullName "appsettings.json"
$jsonDevelopmentFilePath = Join-Path $project.FullName "appsettings.Development.json"

# JSON dosyalarını oku
$jsonContent = Get-Content $jsonFilePath | ConvertFrom-Json
$jsonDevelopmentContent = Get-Content $jsonDevelopmentFilePath -ErrorAction SilentlyContinue | ConvertFrom-Json

# LogCastle ayarlarını güncelle
$logCastleSettings = @{
    "Enabled" = $true
    "MinimumLevel" = "Information"
    "Filter" = @{
        "IgnoreTypes" = @("SomeNamespace.SomeClass")
        "IgnoreMethods" = @("")
    }
    "Providers" = @{
        "Console" = @{
            "Type" = "LogCastle.Providers.ConsoleLogProvider, LogCastle"
            "Enabled" = $true
        }
        "File" = @{
            "Type" = "LogCastle.Providers.FileLogProvider, LogCastle"
            "Enabled" = $false
            "Parameters" = @{
                "FilePath" = "logfile.txt"
            }
        }
    }
}

# Belirli ortamın ayarlarını güncelle
$environment = $project.Properties.Item("Configuration").Value
if ($environment -eq "Development") {
    $jsonDevelopmentContent.LogCastle = $logCastleSettings
    $jsonDevelopmentContent | ConvertTo-Json | Set-Content $jsonDevelopmentFilePath -Encoding UTF8
    Write-Host "LogCastle ayarları (Development) başarıyla eklendi."
}
else {
    $jsonContent.LogCastle = $logCastleSettings
    $jsonContent | ConvertTo-Json | Set-Content $jsonFilePath -Encoding UTF8
    Write-Host "LogCastle ayarları başarıyla eklendi."
}

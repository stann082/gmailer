param(
    [switch]$cli,
    [switch]$ui
)

# If no arguments were passed, publish both
if (-not ($cli -or $ui)) {
    $cli = $true
    $ui = $true
}

# --- CLI Publish ---
if ($cli) {
    Write-Host "`nDeploying CLI app" -ForegroundColor Cyan

    Remove-Item .\pub\ -Recurse -ErrorAction SilentlyContinue
    dotnet publish .\src\cli\cli.csproj -c Release -o pub -v quiet

    $cliTarget = "$env:APPDATA\utils\gmailer.exe"
    New-Item -ItemType Directory -Force -Path (Split-Path $cliTarget) | Out-Null
    Copy-Item -Path .\pub\cli.exe -Destination $cliTarget -Verbose
}

# --- UI Publish ---
if ($ui) {
    Write-Host "`nDeploying MAUI app" -ForegroundColor Cyan

    $localApp = "$env:LOCALAPPDATA\Gmailer"
    Remove-Item $localApp -Recurse -ErrorAction SilentlyContinue
    dotnet publish .\src\ui\ui.csproj -f net9.0-windows10.0.19041.0 -c Release -o $localApp -v quiet

    $desktop = [Environment]::GetFolderPath('Desktop')
    $shortcutPath = Join-Path $desktop 'Gmailer.lnk'
    Write-Host "Creating shortcut at $shortcutPath"

    if (-not (Test-Path $desktop)) {
        New-Item -ItemType Directory -Path $desktop | Out-Null
    }

    if (Test-Path $shortcutPath) {
        Remove-Item $shortcutPath -Force
    }

    $exePath = "$localApp\ui.exe"
    $WshShell = New-Object -ComObject WScript.Shell
    $Shortcut = $WshShell.CreateShortcut($shortcutPath)
    $Shortcut.TargetPath = $exePath
    $Shortcut.WorkingDirectory = Split-Path $exePath
    $Shortcut.IconLocation = $exePath
    $Shortcut.Description = "Launch Gmailer"
    $Shortcut.Save()

    Write-Host "Shortcut created successfully!" -ForegroundColor Green
}

Write-Host "`nDeployment complete!" -ForegroundColor Green

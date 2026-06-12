<#
.SYNOPSIS
    Installs the Gmailer Sync worker as a Windows Service.

.DESCRIPTION
    Publishes the worker project as a single self-contained executable,
    copies it to C:\Program Files\Gmailer, and registers it as a Windows
    Service named "Gmailer Sync". Logs are written by the worker to
    %ProgramData%\Gmailer\logs.

    The script is idempotent. If the service already exists, it offers to
    reinstall (republish + replace binaries), stop, or uninstall it.

    Must be run from an elevated PowerShell prompt.
#>
#Requires -RunAsAdministrator

$ErrorActionPreference = 'Stop'

$ServiceName = 'Gmailer Sync'
$InstallDir  = 'C:\Program Files\Gmailer'
$RepoRoot    = Split-Path -Parent $PSScriptRoot
$Project     = Join-Path $RepoRoot 'src\worker\worker.csproj'
$StagingDir  = Join-Path $RepoRoot 'build\publish\worker'
$ExePath     = Join-Path $InstallDir 'GmailerSync.exe'

function Publish-Worker {
    Write-Host "Publishing worker to staging..." -ForegroundColor Cyan
    if (Test-Path $StagingDir) {
        Remove-Item $StagingDir -Recurse -Force
    }
    dotnet publish $Project -c Release -o $StagingDir
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet publish failed with exit code $LASTEXITCODE"
    }
}

function Copy-WorkerFiles {
    Write-Host "Copying files to $InstallDir..." -ForegroundColor Cyan
    New-Item -ItemType Directory -Force $InstallDir | Out-Null
    Copy-Item (Join-Path $StagingDir '*') $InstallDir -Recurse -Force
}

function Stop-GmailerService {
    $svc = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue
    if ($svc -and $svc.Status -eq 'Running') {
        Write-Host "Stopping service..." -ForegroundColor Cyan
        Stop-Service -Name $ServiceName -Force
        $svc.WaitForStatus('Stopped', [TimeSpan]::FromSeconds(30))
    }
}

function Start-GmailerService {
    Write-Host "Starting service..." -ForegroundColor Cyan
    try {
        Start-Service -Name $ServiceName
        Write-Host "Service started." -ForegroundColor Green
    }
    catch {
        Write-Warning $_.Exception.Message
        Write-Warning ("If the error is a logon failure, grant the account the 'Log on as a service' right: " +
                       "open services.msc -> '$ServiceName' -> Log On tab -> re-enter the credentials.")
    }
}

function New-GmailerService {
    Write-Host ""
    Write-Host "The service should run as YOUR user account so it can read your Gmail OAuth" -ForegroundColor Yellow
    Write-Host "token (%APPDATA%\GmailAPI) and your GMAIL_CLIENT_ID/GMAIL_CLIENT_SECRET" -ForegroundColor Yellow
    Write-Host "environment variables." -ForegroundColor Yellow
    $answer = Read-Host "Run the service as the current user ($env:USERDOMAIN\$env:USERNAME)? [Y/n]"

    $params = @{
        Name           = $ServiceName
        DisplayName    = $ServiceName
        Description    = 'Syncs Gmail messages into the local MongoDB cache every 5 minutes.'
        BinaryPathName = "`"$ExePath`""
        StartupType    = 'Automatic'
    }

    if ($answer -notmatch '^[nN]') {
        $params.Credential = Get-Credential -UserName "$env:USERDOMAIN\$env:USERNAME" -Message "Password for the service account"
    }

    Write-Host "Creating service '$ServiceName'..." -ForegroundColor Cyan
    New-Service @params | Out-Null
    Write-Host "Service created." -ForegroundColor Green
}

function Remove-GmailerService {
    Stop-GmailerService
    Write-Host "Deleting service '$ServiceName'..." -ForegroundColor Cyan
    sc.exe delete $ServiceName | Out-Null
    if ($LASTEXITCODE -ne 0) {
        throw "sc.exe delete failed with exit code $LASTEXITCODE"
    }
    Write-Host "Service deleted." -ForegroundColor Green

    if (Test-Path $InstallDir) {
        $answer = Read-Host "Remove installed files at $InstallDir as well? [y/N]"
        if ($answer -match '^[yY]') {
            Remove-Item $InstallDir -Recurse -Force
            Write-Host "Removed $InstallDir." -ForegroundColor Green
        }
    }
}

# --- Main ---

$existing = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue

if ($null -eq $existing) {
    Write-Host "Service '$ServiceName' is not installed. Performing fresh install." -ForegroundColor Cyan
    Publish-Worker
    Copy-WorkerFiles
    New-GmailerService

    $answer = Read-Host "Start the service now? [Y/n]"
    if ($answer -notmatch '^[nN]') {
        Start-GmailerService
    }
}
else {
    Write-Host "Service '$ServiceName' is already installed (status: $($existing.Status))." -ForegroundColor Yellow

    $choices = @(
        [System.Management.Automation.Host.ChoiceDescription]::new('&Reinstall', 'Republish the worker and replace the installed binaries.')
        [System.Management.Automation.Host.ChoiceDescription]::new('&Stop',      'Stop the running service and exit.')
        [System.Management.Automation.Host.ChoiceDescription]::new('&Uninstall', 'Stop and remove the service.')
        [System.Management.Automation.Host.ChoiceDescription]::new('&Cancel',    'Do nothing and exit.')
    )
    $choice = $Host.UI.PromptForChoice("Service already exists", "What would you like to do?", $choices, 0)

    switch ($choice) {
        0 {
            $wasRunning = $existing.Status -eq 'Running'
            Publish-Worker
            Stop-GmailerService
            Copy-WorkerFiles
            if ($wasRunning) {
                Start-GmailerService
            }
            else {
                Write-Host "Binaries replaced. Service left stopped (it was not running)." -ForegroundColor Green
            }
        }
        1 {
            if ($existing.Status -eq 'Running') {
                Stop-GmailerService
                Write-Host "Service stopped." -ForegroundColor Green
            }
            else {
                Write-Host "Service is not running; nothing to stop." -ForegroundColor Green
            }
        }
        2 { Remove-GmailerService }
        3 { Write-Host "Cancelled." }
    }
}

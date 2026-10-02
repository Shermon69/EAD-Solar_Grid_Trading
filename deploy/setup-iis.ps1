# File:        setup-iis.ps1
# Author:      Shermon H (IT22177964)
# Description: One-time IIS setup for the Solar Grid Web API. Run from an
#              ADMINISTRATOR PowerShell. It enables IIS, installs the ASP.NET
#              Core 8 Hosting Bundle, creates the app pool and website on port
#              8080, grants folder permissions and opens the firewall port.
#              Publish the API to $SitePath first (see docs/INTEGRATION_PLAN.md).
# Created:     02/10/2026

$SiteName = 'SolarGridApi'
$SitePath = 'D:\inetpub\SolarGridApi'
$Port     = 8080

# Stop early if this is not an elevated shell (IIS changes need admin rights).
$isAdmin = ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
if (-not $isAdmin) { throw 'Run this script from an ADMINISTRATOR PowerShell (right-click PowerShell > Run as administrator).' }
if (-not (Test-Path "$SitePath\SolarGrid.Api.dll")) { throw "Publish the API to $SitePath first." }
if (-not (Test-Path "$SitePath\appsettings.Production.json")) { throw "Create $SitePath\appsettings.Production.json first (MongoDbSettings + JwtSettings)." }

# 1. Turn on IIS and the management scripting tools.
Write-Host '1/6 Enabling IIS features...' -ForegroundColor Cyan
$features = 'IIS-WebServerRole', 'IIS-WebServer', 'IIS-CommonHttpFeatures', 'IIS-StaticContent', 'IIS-DefaultDocument',
            'IIS-HttpErrors', 'IIS-RequestFiltering', 'IIS-HttpLogging', 'IIS-ManagementConsole', 'IIS-ManagementScriptingTools'
Enable-WindowsOptionalFeature -Online -FeatureName $features -All -NoRestart | Out-Null

# 2. Install the ASP.NET Core 8 Hosting Bundle (the IIS module that runs the API).
Write-Host '2/6 Installing the ASP.NET Core 8 Hosting Bundle...' -ForegroundColor Cyan
# The Hosting Bundle installs the IIS module under Program Files (not System32).
$ModuleDll = "$env:ProgramFiles\IIS\Asp.Net Core Module\V2\aspnetcorev2.dll"
if (-not (Test-Path $ModuleDll)) {
    if (Get-Command winget -ErrorAction SilentlyContinue) {
        winget install --id Microsoft.DotNet.HostingBundle.8 -e --accept-source-agreements --accept-package-agreements
    } else {
        # winget is not always available in an admin shell, so download Microsoft's installer directly.
        [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
        $installer = 'D:\inetpub\dotnet-hosting-win.exe'
        Invoke-WebRequest 'https://aka.ms/dotnet/8.0/dotnet-hosting-win.exe' -OutFile $installer -UseBasicParsing
        $sig = Get-AuthenticodeSignature $installer
        if ($sig.Status -ne 'Valid' -or $sig.SignerCertificate.Subject -notmatch 'Microsoft') {
            throw "The downloaded installer is not signed by Microsoft (status: $($sig.Status)). Not running it."
        }
        Start-Process $installer -ArgumentList '/install', '/quiet', '/norestart' -Wait
    }
}
if (-not (Test-Path $ModuleDll)) {
    throw 'Hosting Bundle is not installed. Download it from https://dotnet.microsoft.com/download/dotnet/8.0 (Hosting Bundle), install it, then run this script again.'
}
net stop was /y | Out-Null
net start w3svc | Out-Null

# 3. App pool with "No Managed Code" (the API runs on .NET 8, not the old .NET Framework).
Write-Host '3/6 Creating the app pool and website...' -ForegroundColor Cyan
Import-Module WebAdministration
if (-not (Test-Path "IIS:\AppPools\$SiteName")) { New-WebAppPool -Name $SiteName | Out-Null }
Set-ItemProperty "IIS:\AppPools\$SiteName" -Name managedRuntimeVersion -Value ''

# 4. Website on port 8080 pointing at the published folder.
if (Get-Website -Name $SiteName -ErrorAction SilentlyContinue) { Remove-Website -Name $SiteName }
New-Website -Name $SiteName -PhysicalPath $SitePath -ApplicationPool $SiteName -Port $Port | Out-Null

# 5. Let the app pool identity read the published files.
Write-Host '4/6 Granting folder permissions...' -ForegroundColor Cyan
icacls $SitePath /grant "IIS AppPool\${SiteName}:(OI)(CI)RX" | Out-Null

# 6. Open the port so a phone on the same Wi-Fi can reach the API.
Write-Host '5/6 Opening firewall port...' -ForegroundColor Cyan
if (-not (Get-NetFirewallRule -DisplayName "SolarGrid API $Port" -ErrorAction SilentlyContinue)) {
    New-NetFirewallRule -DisplayName "SolarGrid API $Port" -Direction Inbound -Protocol TCP -LocalPort $Port -Action Allow -Profile Any | Out-Null
}

# 7. Check the site answers.
Write-Host '6/6 Testing the site...' -ForegroundColor Cyan
Start-Sleep -Seconds 3
try {
    $r = Invoke-WebRequest "http://localhost:$Port/swagger/v1/swagger.json" -UseBasicParsing -TimeoutSec 60
    Write-Host "OK - HTTP $($r.StatusCode). Open http://localhost:$Port/swagger" -ForegroundColor Green
} catch {
    Write-Host "Site did not answer: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Check: Event Viewer > Windows Logs > Application, or set stdoutLogEnabled=true in $SitePath\web.config" -ForegroundColor Yellow
}

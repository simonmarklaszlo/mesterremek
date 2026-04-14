# ----------------------------
# CONFIG
# ----------------------------
$project = "..\..\SzivarClubManager\SzivarClubManager.csproj"
$template = ".\InnoScripts\szivarclubmanager_install_win-x64.iss"
$generatedDir = ".\InnoScripts"
$innoPath = "C:\Program Files (x86)\Inno Setup 6\ISCC.exe"
$installerPrefix = "szivarclubmanager_install_"
$serverIp = "193.201.185.129"

# ----------------------------
# PLATFORM SELECTION
# ----------------------------
$platforms = @("win-x64", "win-x86", "win-arm", "win-arm64")

Write-Host "Select platform:"
for ($i = 0; $i -lt $platforms.Count; $i++) {
    Write-Host "$i`: $($platforms[$i])"
}

$choice = Read-Host "Enter number"
$runtime = $platforms[$choice]

Write-Host "Selected runtime: $runtime"

# ----------------------------
# PUBLISH
# ----------------------------
$publishDir = "..\SzivarClubManager\bin\Release\net10.0\$runtime\publish"

Write-Host "Publishing..."
dotnet publish $project -c Release -r $runtime --self-contained true

# ----------------------------
# CREATE / LOAD GENERATED ISS
# ----------------------------
$generatedFile = [System.IO.Path]::Combine($generatedDir, "$installerPrefix$runtime.iss")


if (!(Test-Path $generatedFile)) {
    Write-Host "Creating new installer script for $runtime..."

    $templateContent = Get-Content $template -Raw
    $newContent = $templateContent -replace "win-x64", $runtime

    Set-Content -Path $generatedFile -Value $newContent

} else {
    Write-Host "Using existing script..."
}

# ----------------------------
# FIX .ENV (optional)
# ----------------------------
$envFile = Join-Path $publishDir ".env"

if (Test-Path $envFile) {
    $lines = Get-Content $envFile
    if ($lines.Count -gt 0) {
        $lines[0] = "DB_HOST=$serverIp"
        Set-Content $envFile $lines
        Write-Host ".env updated"
    }
}

# ----------------------------
# UPDATE SOURCE PATH SAFETY
# ----------------------------
$content = Get-Content $generatedFile -Raw

$content = $content -replace `
    "win-x64", $runtime

$content = $content -replace `
    [regex]::Escape("..\SzivarClubManager\bin\Release\net10.0\win-x64\publish"), `
    "..\SzivarClubManager\bin\Release\net10.0\$runtime\publish"

Set-Content $generatedFile -Value $content

Write-Host "Installer script ready: $generatedFile"

# ----------------------------
# COMPILE
# ----------------------------
Write-Host "Compiling installer..."

Start-Process $innoPath -ArgumentList "`"$generatedFile`"" -Wait

Write-Host "DONE!"
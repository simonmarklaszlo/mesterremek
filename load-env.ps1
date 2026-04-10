function Load-Env($path = ".env") {
    if (-Not (Test-Path $path)) {
        Write-Host ".env file not found!"
        return
    }

    Get-Content $path | ForEach-Object {
        if ($_ -match "^\s*#") { return } # skip comments
        if ($_ -match "^\s*$") { return } # skip empty lines

        $parts = $_ -split "=", 2
        $name = $parts[0].Trim()
        $value = $parts[1].Trim()

        [System.Environment]::SetEnvironmentVariable($name, $value)
    }
}

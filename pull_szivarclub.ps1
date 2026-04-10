. .\load-env.ps1
Load-Env

$RemoteHost = $env:REMOTE_HOST
$RemotePort = $env:REMOTE_PORT
$RemoteUser = $env:REMOTE_USER
$RemotePassword = $env:REMOTE_PASSWORD
$RemoteDatabase = $env:REMOTE_DATABASE

$containerName = $env:DB_CONTAINER

$OutputFile = "sql/szivarclub.sql"

Write-Host "Dumping schema from remote database..."

$env:PGPASSWORD = $RemotePassword

cmd /c "docker exec -e PGPASSWORD=$RemotePassword -i $containerName pg_dump -h $RemoteHost -p $RemotePort -U $RemoteUser -d $RemoteDatabase > $OutputFile"

if ($LASTEXITCODE -eq 0) {
    Write-Host "Schema dumped successfully to $OutputFile"
} else {
    Write-Host "Dump failed"
}

Remove-Item Env:PGPASSWORD

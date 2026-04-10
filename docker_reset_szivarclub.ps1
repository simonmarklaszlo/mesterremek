. .\load-env.ps1
Load-Env

$DBContainer = $env:DB_CONTAINER
$DBName = $env:DB_NAME
$SecDBName = $env:SEC_DB_NAME
$DBUser = $env:DB_USER
$DBPassword = $env:DB_PASSWORD

$DropFile = ".\sql\drop_szivarclub.sql"
$ImportFile = ".\sql\szivarclub.sql"

Write-Host ""
Write-Host "Copying SQL files to container..."

docker cp $DropFile "${DBContainer}:/tmp/drop.sql"
docker cp $ImportFile "${DBContainer}:/tmp/import.sql"

Write-Host ""
Write-Host "Dropping database..."

docker exec -e PGPASSWORD=$DBPassword $DBContainer `
    psql -U $DBUser -d $SecDBName -f /tmp/drop.sql

Write-Host ""
Write-Host "Importing database schema..."

docker exec -e PGPASSWORD=$DBPassword $DBContainer `
    psql -U $DBUser -d $DBName -f /tmp/import.sql

Write-Host ""
Write-Host "Done."

. .\load-env.ps1
Load-Env

$containerName = $env:DB_CONTAINER
$containerData = $env:DB_CONTAINER_DATA
$image = "postgis/postgis:16-master"

$postgresPassword = $env:POSTGRES_PASSWORD

$user = $env:DB_USER
$password = $env:DB_PASSWORD
$database = $env:DB_NAME


Write-Host "Pulling PostGIS image..."
docker pull $image

Write-Host "Deleting old container..."
docker rm -f $containerName 2>$null

Write-Host "Creating volume..."
docker volume create $containerData

Write-Host "Creating container..."
docker run -d `
    --name $containerName `
    -e POSTGRES_PASSWORD=$password `
    -p 5432:5432 `
    -v ${containerData}:/var/lib/postgresql/data `
    $image

Write-Host "Waiting for PostgreSQL to start..."
do {
    Start-Sleep -Seconds 1
    $status = docker exec $containerName pg_isready -U postgres 2>&1
} while ($status -notmatch "accepting connections")

Write-Host "Setting up database..."
docker exec -i $containerName psql -U postgres -c "CREATE USER $user WITH PASSWORD '$password';"

# Write-Host "Granting privileges..."
docker exec -i $containerName psql -U postgres -c "ALTER USER $user WITH SUPERUSER CREATEDB CREATEROLE;"

# Write-Host "Creating dev database..."
docker exec -i $containerName psql -U postgres -c "CREATE DATABASE $database OWNER $user;"

# Write-Host "Enabling PostGIS extension..."
# docker exec -i $containerName psql -U postgres -d $database -c "CREATE EXTENSION postgis;"

Write-Host "Host: localhost"
Write-Host "Port: 5432"
Write-Host "User: $user"
Write-Host "Password: $password"
Write-Host "Database: $database"

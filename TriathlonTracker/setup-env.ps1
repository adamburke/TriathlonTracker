# TriathlonTracker Environment Setup Script
# This script helps set up environment variables for development

Write-Host "TriathlonTracker Environment Setup" -ForegroundColor Green
Write-Host "=================================" -ForegroundColor Green

# Check if environment variables are already set
$googleClientId = $env:GOOGLE_CLIENT_ID
$googleClientSecret = $env:GOOGLE_CLIENT_SECRET
$jwtKey = $env:JWT_KEY

Write-Host "`nCurrent Environment Variables:" -ForegroundColor Yellow
Write-Host "GOOGLE_CLIENT_ID: $(if ($googleClientId) { 'Set' } else { 'Not Set' })" -ForegroundColor $(if ($googleClientId) { 'Green' } else { 'Red' })
Write-Host "GOOGLE_CLIENT_SECRET: $(if ($googleClientSecret) { 'Set' } else { 'Not Set' })" -ForegroundColor $(if ($googleClientSecret) { 'Green' } else { 'Red' })
Write-Host "JWT_KEY: $(if ($jwtKey) { 'Set' } else { 'Not Set' })" -ForegroundColor $(if ($jwtKey) { 'Green' } else { 'Red' })

Write-Host "`nTo set environment variables for this session:" -ForegroundColor Cyan
Write-Host '$env:GOOGLE_CLIENT_ID = "your-google-client-id"' -ForegroundColor White
Write-Host '$env:GOOGLE_CLIENT_SECRET = "your-google-client-secret"' -ForegroundColor White
Write-Host '$env:JWT_KEY = "your-jwt-key"' -ForegroundColor White

Write-Host "`nTo set environment variables permanently (Windows):" -ForegroundColor Cyan
Write-Host '[Environment]::SetEnvironmentVariable("GOOGLE_CLIENT_ID", "your-google-client-id", "User")' -ForegroundColor White
Write-Host '[Environment]::SetEnvironmentVariable("GOOGLE_CLIENT_SECRET", "your-google-client-secret", "User")' -ForegroundColor White
Write-Host '[Environment]::SetEnvironmentVariable("JWT_KEY", "your-jwt-key", "User")' -ForegroundColor White

Write-Host "`nTo generate a secure JWT key:" -ForegroundColor Cyan
Write-Host '$jwtKey = [Convert]::ToBase64String((1..64 | ForEach-Object { Get-Random -Maximum 256 }))' -ForegroundColor White
Write-Host '$env:JWT_KEY = $jwtKey' -ForegroundColor White

Write-Host "`nAfter setting environment variables, restart your terminal and run:" -ForegroundColor Green
Write-Host "dotnet run" -ForegroundColor White

Write-Host "`nThe application will automatically seed the database with your credentials." -ForegroundColor Green 
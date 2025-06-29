# TriathlonTracker Deployment Validation Script
# This script validates all required environment variables and configuration before deployment

param(
    [string]$Environment = "Production"
)

Write-Host "TriathlonTracker Deployment Validation" -ForegroundColor Green
Write-Host "=====================================" -ForegroundColor Green
Write-Host "Environment: $Environment" -ForegroundColor Yellow
Write-Host ""

# Required environment variables
$requiredVars = @(
    @{ Name = "DATABASE_CONNECTION_STRING"; Description = "Database Connection String" },
    @{ Name = "GOOGLE_CLIENT_ID"; Description = "Google OAuth Client ID" },
    @{ Name = "GOOGLE_CLIENT_SECRET"; Description = "Google OAuth Client Secret" },
    @{ Name = "JWT_KEY"; Description = "JWT Signing Key" },
    @{ Name = "ENCRYPTION_KEY"; Description = "AES Encryption Key (32 chars)" },
    @{ Name = "ENCRYPTION_IV"; Description = "AES Encryption IV (16 chars)" }
)

# Optional environment variables
$optionalVars = @(
    @{ Name = "JWT_ISSUER"; Description = "JWT Issuer" },
    @{ Name = "JWT_AUDIENCE"; Description = "JWT Audience" },
    @{ Name = "BASE_URL"; Description = "Application Base URL" },
    @{ Name = "ASPNETCORE_ENVIRONMENT"; Description = "ASP.NET Core Environment" }
)

$allPassed = $true
$warnings = @()

Write-Host "Checking Required Environment Variables:" -ForegroundColor Cyan
Write-Host "----------------------------------------" -ForegroundColor Cyan

foreach ($var in $requiredVars) {
    $value = [Environment]::GetEnvironmentVariable($var.Name)
    if ([string]::IsNullOrEmpty($value)) {
        Write-Host "❌ $($var.Name): NOT SET - $($var.Description)" -ForegroundColor Red
        $allPassed = $false
    } else {
        Write-Host "✅ $($var.Name): SET - $($var.Description)" -ForegroundColor Green
    }
}

Write-Host ""
Write-Host "Checking Optional Environment Variables:" -ForegroundColor Cyan
Write-Host "----------------------------------------" -ForegroundColor Cyan

foreach ($var in $optionalVars) {
    $value = [Environment]::GetEnvironmentVariable($var.Name)
    if ([string]::IsNullOrEmpty($value)) {
        Write-Host "⚠️  $($var.Name): NOT SET - $($var.Description)" -ForegroundColor Yellow
        $warnings += "$($var.Name) is not set"
    } else {
        Write-Host "✅ $($var.Name): SET - $($var.Description)" -ForegroundColor Green
    }
}

Write-Host ""
Write-Host "Validating Configuration Values:" -ForegroundColor Cyan
Write-Host "--------------------------------" -ForegroundColor Cyan

# Validate encryption key length
$encryptionKey = [Environment]::GetEnvironmentVariable("ENCRYPTION_KEY")
if (![string]::IsNullOrEmpty($encryptionKey)) {
    if ($encryptionKey.Length -lt 32) {
        Write-Host "❌ ENCRYPTION_KEY: Too short (needs 32 chars, has $($encryptionKey.Length))" -ForegroundColor Red
        $allPassed = $false
    } else {
        Write-Host "✅ ENCRYPTION_KEY: Length OK ($($encryptionKey.Length) chars)" -ForegroundColor Green
    }
}

# Validate encryption IV length
$encryptionIV = [Environment]::GetEnvironmentVariable("ENCRYPTION_IV")
if (![string]::IsNullOrEmpty($encryptionIV)) {
    if ($encryptionIV.Length -lt 16) {
        Write-Host "❌ ENCRYPTION_IV: Too short (needs 16 chars, has $($encryptionIV.Length))" -ForegroundColor Red
        $allPassed = $false
    } else {
        Write-Host "✅ ENCRYPTION_IV: Length OK ($($encryptionIV.Length) chars)" -ForegroundColor Green
    }
}

# Validate JWT key strength
$jwtKey = [Environment]::GetEnvironmentVariable("JWT_KEY")
if (![string]::IsNullOrEmpty($jwtKey)) {
    if ($jwtKey.Length -lt 32) {
        Write-Host "❌ JWT_KEY: Too short (recommend at least 32 chars, has $($jwtKey.Length))" -ForegroundColor Red
        $allPassed = $false
    } else {
        Write-Host "✅ JWT_KEY: Length OK ($($jwtKey.Length) chars)" -ForegroundColor Green
    }
}

# Validate database connection string
$dbConnection = [Environment]::GetEnvironmentVariable("DATABASE_CONNECTION_STRING")
if (![string]::IsNullOrEmpty($dbConnection)) {
    if ($dbConnection -like "*localhost*" -and $Environment -eq "Production") {
        Write-Host "⚠️  DATABASE_CONNECTION_STRING: Contains 'localhost' in production environment" -ForegroundColor Yellow
        $warnings += "Database connection string contains localhost in production"
    } else {
        Write-Host "✅ DATABASE_CONNECTION_STRING: Format appears valid" -ForegroundColor Green
    }
}

# Validate Google OAuth configuration
$googleClientId = [Environment]::GetEnvironmentVariable("GOOGLE_CLIENT_ID")
$googleClientSecret = [Environment]::GetEnvironmentVariable("GOOGLE_CLIENT_SECRET")
if (![string]::IsNullOrEmpty($googleClientId) -and ![string]::IsNullOrEmpty($googleClientSecret)) {
    if ($googleClientId -like "*your-google-client-id*" -or $googleClientSecret -like "*your-google-client-secret*") {
        Write-Host "❌ Google OAuth: Still using placeholder values" -ForegroundColor Red
        $allPassed = $false
    } else {
        Write-Host "✅ Google OAuth: Real credentials configured" -ForegroundColor Green
    }
}

# Validate JWT issuer/audience for production
if ($Environment -eq "Production") {
    $jwtIssuer = [Environment]::GetEnvironmentVariable("JWT_ISSUER")
    $jwtAudience = [Environment]::GetEnvironmentVariable("JWT_AUDIENCE")
    
    if ([string]::IsNullOrEmpty($jwtIssuer) -or $jwtIssuer -like "*localhost*") {
        Write-Host "❌ JWT_ISSUER: Should be production domain, not localhost" -ForegroundColor Red
        $allPassed = $false
    }
    
    if ([string]::IsNullOrEmpty($jwtAudience) -or $jwtAudience -like "*localhost*") {
        Write-Host "❌ JWT_AUDIENCE: Should be production domain, not localhost" -ForegroundColor Red
        $allPassed = $false
    }
}

Write-Host ""
Write-Host "Deployment Validation Summary:" -ForegroundColor Cyan
Write-Host "=============================" -ForegroundColor Cyan

if ($allPassed) {
    Write-Host "✅ DEPLOYMENT READY - All required configuration is valid!" -ForegroundColor Green
} else {
    Write-Host "❌ DEPLOYMENT BLOCKED - Fix the issues above before deploying" -ForegroundColor Red
}

if ($warnings.Count -gt 0) {
    Write-Host ""
    Write-Host "⚠️  Warnings (should be addressed):" -ForegroundColor Yellow
    foreach ($warning in $warnings) {
        Write-Host "   - $warning" -ForegroundColor Yellow
    }
}

Write-Host ""
Write-Host "Next Steps:" -ForegroundColor Cyan
Write-Host "===========" -ForegroundColor Cyan

if ($Environment -eq "Production") {
    Write-Host "1. Verify Google Cloud Console redirect URIs include your production domain" -ForegroundColor White
    Write-Host "2. Ensure SSL certificates are configured for HTTPS" -ForegroundColor White
    Write-Host "3. Test database connectivity from the deployment environment" -ForegroundColor White
    Write-Host "4. Configure monitoring and logging" -ForegroundColor White
    Write-Host "5. Set up backup and disaster recovery procedures" -ForegroundColor White
} else {
    Write-Host "1. Test the application locally with these environment variables" -ForegroundColor White
    Write-Host "2. Verify Google OAuth works with your test environment" -ForegroundColor White
    Write-Host "3. Test database connectivity and seeding" -ForegroundColor White
}

Write-Host ""
Write-Host "For more information, see SETUP.md" -ForegroundColor Gray 
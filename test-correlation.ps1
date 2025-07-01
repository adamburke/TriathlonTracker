# Test Request Correlation Script
# This script tests the enhanced telemetry correlation features

Write-Host "Testing Enhanced Request Correlation..." -ForegroundColor Green

# Start the application
Write-Host "Starting application..." -ForegroundColor Yellow
Start-Process -FilePath "dotnet" -ArgumentList "run" -WorkingDirectory "TriathlonTracker" -WindowStyle Minimized

# Wait for application to start
Write-Host "Waiting for application to start..." -ForegroundColor Yellow
Start-Sleep -Seconds 10

# Test endpoints to generate telemetry events
Write-Host "Testing endpoints to generate telemetry events..." -ForegroundColor Yellow

# Test 1: Home page
Write-Host "Test 1: Accessing home page..." -ForegroundColor Cyan
try {
    $response = Invoke-WebRequest -Uri "http://localhost:5217/" -UseBasicParsing
    Write-Host "Home page status: $($response.StatusCode)" -ForegroundColor Green
} catch {
    Write-Host "Home page error: $($_.Exception.Message)" -ForegroundColor Red
}

Start-Sleep -Seconds 2

# Test 2: Admin page (will redirect to login)
Write-Host "Test 2: Accessing admin page..." -ForegroundColor Cyan
try {
    $response = Invoke-WebRequest -Uri "http://localhost:5217/Admin" -UseBasicParsing -MaximumRedirection 0
    Write-Host "Admin page status: $($response.StatusCode)" -ForegroundColor Green
} catch {
    Write-Host "Admin page error (expected redirect): $($_.Exception.Message)" -ForegroundColor Yellow
}

Start-Sleep -Seconds 2

# Test 3: Telemetry health endpoint
Write-Host "Test 3: Testing telemetry health endpoint..." -ForegroundColor Cyan
try {
    $response = Invoke-WebRequest -Uri "http://localhost:5217/api/Telemetry/health" -UseBasicParsing
    $healthData = $response.Content | ConvertFrom-Json
    Write-Host "Telemetry health: Enabled=$($healthData.IsEnabled), QueueSize=$($healthData.QueueSize)" -ForegroundColor Green
} catch {
    Write-Host "Telemetry health error: $($_.Exception.Message)" -ForegroundColor Red
}

Start-Sleep -Seconds 2

# Test 4: Get correlation summary
Write-Host "Test 4: Getting correlation summary..." -ForegroundColor Cyan
try {
    $response = Invoke-WebRequest -Uri "http://localhost:5217/api/Telemetry/correlation/summary" -UseBasicParsing
    $summaryData = $response.Content | ConvertFrom-Json
    Write-Host "Correlation summary: TotalEvents=$($summaryData.TotalEvents), UniqueRequests=$($summaryData.UniqueRequests)" -ForegroundColor Green
    
    if ($summaryData.RecentRequests.Count -gt 0) {
        Write-Host "Recent requests:" -ForegroundColor Cyan
        foreach ($request in $summaryData.RecentRequests) {
            Write-Host "  RequestId: $($request.RequestId), Events: $($request.EventCount)" -ForegroundColor White
        }
    }
} catch {
    Write-Host "Correlation summary error: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`nRequest correlation test completed!" -ForegroundColor Green
Write-Host "Check the application logs for detailed correlation information." -ForegroundColor Yellow
Write-Host "Look for log entries with RequestId patterns like '0HNDO8A36QE3K:00000001'" -ForegroundColor Yellow

# Keep the application running for manual testing
Write-Host "`nApplication is still running. Press any key to stop..." -ForegroundColor Magenta
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")

# Stop the application
Write-Host "Stopping application..." -ForegroundColor Yellow
Get-Process -Name "dotnet" -ErrorAction SilentlyContinue | Where-Object { $_.ProcessName -eq "dotnet" } | Stop-Process -Force 
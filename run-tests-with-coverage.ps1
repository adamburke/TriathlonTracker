Remove-Item -Recurse -Force ".\TriathlonTracker.Tests\TestResults\";
dotnet clean; 
dotnet build; 
dotnet test TriathlonTracker.Tests --collect:"XPlat Code Coverage" --settings ".\TriathlonTracker.Tests\coverlet.runsettings"; 

# Find the coverage file
$coverageFile = Get-ChildItem -Path . -Recurse -Name "coverage.cobertura.xml" | Select-Object -First 1

# Generate HTML report
reportgenerator -reports:$coverageFile -targetdir:coverage -reporttypes:Html

#reportgenerator -reports ".\**\TestResults\**\coverage.cobertura.xml" -targetdir ".\coverage" -reporttypes:Html
#$coverageFile = Get-ChildItem -Path ".\TriathlonTracker.Tests\TestResults" -Recurse -Filter coverage.cobertura.xml | Select-Object -First 1
#reportgenerator -reports $coverageFile.FullName -targetdir ".\coverage" -reporttypes:Html

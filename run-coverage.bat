@echo off
echo Running tests with code coverage...

REM Run tests with coverage
dotnet test TriathlonTracker.sln --collect:"XPlat Code Coverage" --settings TriathlonTracker.Tests/coverlet.runsettings

REM Find the coverage file
for /r %%i in (coverage.cobertura.xml) do set COVERAGE_FILE=%%i

if defined COVERAGE_FILE (
    echo Generating HTML coverage report...
    
    REM Create coverage directory if it doesn't exist
    if not exist "coverage" mkdir coverage
    
    REM Generate HTML report
    reportgenerator -reports:"%COVERAGE_FILE%" -targetdir:coverage -reporttypes:Html
    
    echo Coverage report generated in 'coverage' directory
    echo Open 'coverage/index.html' in your browser to view the report
) else (
    echo No coverage file found!
)

pause 
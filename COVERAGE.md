# Code Coverage Setup

This project has been configured with code coverage using Coverlet and ReportGenerator.

## Prerequisites

The following tools are required:

1. **Coverlet Collector** - Already added to the test project
2. **ReportGenerator** - Global tool for generating HTML reports

## Running Tests with Coverage

### Option 1: Using PowerShell Script (Recommended)
```powershell
.\run-tests-with-coverage.ps1
```

### Option 2: Using Batch File (Windows)
```cmd
run-coverage.bat
```

### Option 3: Manual Command
```bash
dotnet test TriathlonTracker.sln --collect:"XPlat Code Coverage" --settings TriathlonTracker.Tests/coverlet.runsettings
```

## Generating HTML Report

After running tests with coverage, generate an HTML report:

```bash
# Find the coverage file
$coverageFile = Get-ChildItem -Path . -Recurse -Name "coverage.cobertura.xml" | Select-Object -First 1

# Generate HTML report
reportgenerator -reports:$coverageFile -targetdir:coverage -reporttypes:Html
```

## Viewing Coverage Reports

1. Open `coverage/index.html` in your web browser
2. Navigate through the different classes and methods to see coverage details
3. The report shows:
   - Overall coverage percentage
   - Line-by-line coverage
   - Branch coverage
   - Uncovered code highlighted in red

## Coverage Configuration

The coverage is configured in `TriathlonTracker.Tests/coverlet.runsettings`:

- **Format**: Cobertura XML
- **Exclusions**: 
  - Program.cs and Startup.cs
  - Migration files
  - View files
  - wwwroot files
- **Inclusions**: Only TriathlonTracker project files
- **Source Link**: Enabled for better debugging

## Coverage Goals

- **Line Coverage**: Aim for 90%+ coverage
- **Branch Coverage**: Aim for 80%+ coverage
- **Critical Paths**: Ensure all business logic is covered

## Troubleshooting

### No Coverage File Generated
- Ensure all tests are passing
- Check that coverlet.collector package is installed
- Verify the runsettings file path is correct

### Missing Source Files in Report
- The 404 errors for GitHub URLs are normal for local development
- Coverage data is still collected correctly
- Source files will be available when the project is in a Git repository

### Low Coverage
- Add more test cases for uncovered code
- Focus on business logic and critical paths
- Consider excluding generated code or framework code

## Continuous Integration

To integrate coverage into CI/CD:

1. Add coverage collection to your build pipeline
2. Set coverage thresholds for pull requests
3. Generate and publish coverage reports as build artifacts
4. Use coverage data to identify areas needing more tests 
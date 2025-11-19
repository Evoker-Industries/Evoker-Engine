@echo off
REM Script to run tests and generate HTML report for Windows

echo Running Evoker-Engine Test Suite...
echo ======================================
echo.

REM Create test results directory
if not exist TestResults mkdir TestResults

REM Run tests with TRX logger
dotnet test --logger "trx;LogFileName=TestResults.trx" --results-directory ./TestResults

set TEST_EXIT_CODE=%ERRORLEVEL%

echo.
echo Generating HTML test report...

REM Use PowerShell to convert TRX to HTML
powershell -ExecutionPolicy Bypass -Command "$trxFiles = Get-ChildItem -Path 'TestResults' -Filter '*.trx' -Recurse; if ($trxFiles.Count -eq 0) { Write-Host 'No TRX file found!'; exit 1 }; $trxFile = $trxFiles[0].FullName; Write-Host \"Processing: $trxFile\"; [xml]$xml = Get-Content $trxFile; $ns = New-Object Xml.XmlNamespaceManager $xml.NameTable; $ns.AddNamespace('x', 'http://microsoft.com/schemas/VisualStudio/TeamTest/2010'); $counters = $xml.SelectSingleNode('//x:Counters', $ns); $total = [int]$counters.total; $passed = [int]$counters.passed; $failed = [int]$counters.failed; $skipped = [int]$counters.inconclusive; $results = $xml.SelectNodes('//x:UnitTestResult', $ns); $testResultsHtml = ''; foreach ($result in $results) { $testName = $result.testName; $outcome = $result.outcome; $duration = $result.duration; $timeParts = $duration.Split(':'); $durationMs = ([double]$timeParts[0] * 3600 + [double]$timeParts[1] * 60 + [double]$timeParts[2]) * 1000; $statusClass = $outcome.ToLower(); $statusIcon = if ($outcome -eq 'Passed') { '✓' } else { '✗' }; $errorMsg = ''; $output = $result.SelectSingleNode('x:Output', $ns); if ($output) { $errorInfo = $output.SelectSingleNode('x:ErrorInfo', $ns); if ($errorInfo) { $message = $errorInfo.SelectSingleNode('x:Message', $ns); if ($message) { $errorMsg = $message.InnerText } } }; $testResultsHtml += \"<div class='test-item $statusClass'><div class='test-name'>$testName</div><div class='test-duration'>$([math]::Round($durationMs))ms</div><div class='test-status $statusClass'>$statusIcon $outcome</div></div>\"; if ($errorMsg) { $testResultsHtml += \"<div class='error-message'>$errorMsg</div>\" } }; $date = Get-Date -Format 'yyyy-MM-dd HH:mm:ss'; $passRate = if ($total -gt 0) { ($passed / $total * 100) } else { 0 }; $html = @\"
<!DOCTYPE html>
<html>
<head>
<meta charset='UTF-8'>
<title>Evoker-Engine Test Results</title>
<style>*{margin:0;padding:0;box-sizing:border-box}body{font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,sans-serif;background:linear-gradient(135deg,#667eea 0%%,#764ba2 100%%);padding:20px;min-height:100vh}.container{max-width:1200px;margin:0 auto;background:white;border-radius:12px;box-shadow:0 20px 60px rgba(0,0,0,0.3);overflow:hidden}.header{background:linear-gradient(135deg,#667eea 0%%,#764ba2 100%%);color:white;padding:40px;text-align:center}.header h1{font-size:36px;margin-bottom:10px}.summary{display:grid;grid-template-columns:repeat(auto-fit,minmax(200px,1fr));gap:20px;padding:40px;background:#f8f9fa}.summary-card{background:white;padding:25px;border-radius:8px;text-align:center;box-shadow:0 2px 8px rgba(0,0,0,0.1)}.summary-card .number{font-size:48px;font-weight:bold;margin-bottom:10px}.summary-card .label{color:#6c757d;font-size:14px;text-transform:uppercase}.passed{color:#28a745}.failed{color:#dc3545}.total{color:#007bff}.tests{padding:40px}.tests h2{margin-bottom:20px;color:#333}.test-item{background:#f8f9fa;padding:15px 20px;margin-bottom:10px;border-radius:6px;border-left:4px solid #ddd;display:flex;justify-content:space-between;align-items:center}.test-item.passed{border-left-color:#28a745}.test-item.failed{border-left-color:#dc3545;background:#fff5f5}.test-name{flex:1;font-weight:500}.test-duration{color:#6c757d;font-size:14px;margin:0 20px}.test-status{padding:5px 15px;border-radius:20px;font-size:12px;font-weight:bold;text-transform:uppercase}.test-status.passed{background:#d4edda;color:#155724}.test-status.failed{background:#f8d7da;color:#721c24}.error-message{margin-top:10px;padding:10px;background:#fff;border-left:3px solid #dc3545;font-family:monospace;font-size:12px;color:#721c24}.footer{background:#343a40;color:white;padding:20px;text-align:center;font-size:14px}.progress-bar{width:100%%;height:8px;background:#e9ecef;border-radius:4px;overflow:hidden;margin-top:20px}.progress-fill{height:100%%;background:linear-gradient(90deg,#28a745 0%%,#20c997 100%%)}</style>
</head>
<body>
<div class='container'>
<div class='header'><h1>🎮 Evoker-Engine Test Results</h1><div class='subtitle'>Generated on $date</div></div>
<div class='summary'>
<div class='summary-card'><div class='number total'>$total</div><div class='label'>Total Tests</div></div>
<div class='summary-card'><div class='number passed'>✓ $passed</div><div class='label'>Passed</div></div>
<div class='summary-card'><div class='number failed'>✗ $failed</div><div class='label'>Failed</div></div>
<div class='summary-card'><div class='number skipped'>⊘ $skipped</div><div class='label'>Skipped</div></div>
</div>
<div style='padding:0 40px'><div class='progress-bar'><div class='progress-fill' style='width:$([math]::Round($passRate))%%'></div></div></div>
<div class='tests'><h2>Test Details</h2>$testResultsHtml</div>
<div class='footer'><strong>Evoker-Engine</strong> - A complete C# game engine<br>Test Suite: $([math]::Round($passRate, 1))%% Success Rate</div>
</div>
</body>
</html>
\"@; $html | Out-File -FilePath 'TestResults/TestResults.html' -Encoding UTF8; Write-Host \"HTML report generated: TestResults/TestResults.html\"; Write-Host \"Tests: $total total, $passed passed, $failed failed, $skipped skipped\""

if exist "TestResults\TestResults.html" (
    echo.
    echo HTML report generated successfully!
    echo Location: TestResults\TestResults.html
    echo.
    echo Open the report with:
    echo   start TestResults\TestResults.html
) else (
    echo Failed to generate HTML report
)

echo.
if %TEST_EXIT_CODE%==0 (
    echo All tests passed!
) else (
    echo Some tests failed
)

exit /b %TEST_EXIT_CODE%

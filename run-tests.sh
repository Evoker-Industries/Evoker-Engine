#!/bin/bash
# Script to run tests and generate HTML report

# Colors for output
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo -e "${YELLOW}Running Evoker-Engine Test Suite...${NC}"
echo "======================================"

# Create test results directory
mkdir -p TestResults

# Run tests with TRX logger
dotnet test --logger "trx;LogFileName=TestResults.trx" --results-directory ./TestResults

TEST_EXIT_CODE=$?

# Convert TRX to HTML
echo ""
echo -e "${YELLOW}Generating HTML test report...${NC}"

# Create HTML report from TRX file
python3 << 'PYTHON_SCRIPT'
import xml.etree.ElementTree as ET
import datetime
import os
import glob

# Find the TRX file
trx_files = glob.glob("TestResults/**/*.trx", recursive=True)
if not trx_files:
    print("No TRX file found!")
    exit(1)

trx_file = trx_files[0]
print(f"Processing: {trx_file}")

# Parse TRX file
tree = ET.parse(trx_file)
root = tree.getroot()

# XML namespace
ns = {'': 'http://microsoft.com/schemas/VisualStudio/TeamTest/2010'}

# Get test results
results = root.find('.//Results', ns)
summary = root.find('.//ResultSummary', ns)
counters = summary.find('Counters', ns) if summary is not None else None

total = int(counters.get('total', 0)) if counters is not None else 0
passed = int(counters.get('passed', 0)) if counters is not None else 0
failed = int(counters.get('failed', 0)) if counters is not None else 0
skipped = int(counters.get('inconclusive', 0)) if counters is not None else 0

# Get test execution times
test_results = []
if results is not None:
    for result in results.findall('UnitTestResult', ns):
        test_name = result.get('testName', 'Unknown')
        outcome = result.get('outcome', 'Unknown')
        duration = result.get('duration', '00:00:00')
        
        # Parse duration
        time_parts = duration.split(':')
        if len(time_parts) == 3:
            duration_ms = (float(time_parts[0]) * 3600 + 
                          float(time_parts[1]) * 60 + 
                          float(time_parts[2])) * 1000
        else:
            duration_ms = 0
        
        # Get error message if failed
        error_msg = ""
        output_elem = result.find('Output', ns)
        if output_elem is not None:
            error_info = output_elem.find('ErrorInfo', ns)
            if error_info is not None:
                message = error_info.find('Message', ns)
                if message is not None:
                    error_msg = message.text or ""
        
        test_results.append({
            'name': test_name,
            'outcome': outcome,
            'duration': duration_ms,
            'error': error_msg
        })

# Generate HTML
html = f'''<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Evoker-Engine Test Results</title>
    <style>
        * {{
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }}
        
        body {{
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            padding: 20px;
            min-height: 100vh;
        }}
        
        .container {{
            max-width: 1200px;
            margin: 0 auto;
            background: white;
            border-radius: 12px;
            box-shadow: 0 20px 60px rgba(0,0,0,0.3);
            overflow: hidden;
        }}
        
        .header {{
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            padding: 40px;
            text-align: center;
        }}
        
        .header h1 {{
            font-size: 36px;
            margin-bottom: 10px;
            text-shadow: 0 2px 4px rgba(0,0,0,0.2);
        }}
        
        .header .subtitle {{
            font-size: 18px;
            opacity: 0.9;
        }}
        
        .summary {{
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
            gap: 20px;
            padding: 40px;
            background: #f8f9fa;
        }}
        
        .summary-card {{
            background: white;
            padding: 25px;
            border-radius: 8px;
            text-align: center;
            box-shadow: 0 2px 8px rgba(0,0,0,0.1);
            transition: transform 0.2s;
        }}
        
        .summary-card:hover {{
            transform: translateY(-5px);
            box-shadow: 0 4px 12px rgba(0,0,0,0.15);
        }}
        
        .summary-card .number {{
            font-size: 48px;
            font-weight: bold;
            margin-bottom: 10px;
        }}
        
        .summary-card .label {{
            color: #6c757d;
            font-size: 14px;
            text-transform: uppercase;
            letter-spacing: 1px;
        }}
        
        .passed {{ color: #28a745; }}
        .failed {{ color: #dc3545; }}
        .skipped {{ color: #ffc107; }}
        .total {{ color: #007bff; }}
        
        .tests {{
            padding: 40px;
        }}
        
        .tests h2 {{
            margin-bottom: 20px;
            color: #333;
            font-size: 24px;
        }}
        
        .test-item {{
            background: #f8f9fa;
            padding: 15px 20px;
            margin-bottom: 10px;
            border-radius: 6px;
            border-left: 4px solid #ddd;
            display: flex;
            justify-content: space-between;
            align-items: center;
            transition: all 0.2s;
        }}
        
        .test-item:hover {{
            background: #e9ecef;
            transform: translateX(5px);
        }}
        
        .test-item.passed {{
            border-left-color: #28a745;
        }}
        
        .test-item.failed {{
            border-left-color: #dc3545;
            background: #fff5f5;
        }}
        
        .test-name {{
            flex: 1;
            font-weight: 500;
        }}
        
        .test-duration {{
            color: #6c757d;
            font-size: 14px;
            margin: 0 20px;
        }}
        
        .test-status {{
            padding: 5px 15px;
            border-radius: 20px;
            font-size: 12px;
            font-weight: bold;
            text-transform: uppercase;
        }}
        
        .test-status.passed {{
            background: #d4edda;
            color: #155724;
        }}
        
        .test-status.failed {{
            background: #f8d7da;
            color: #721c24;
        }}
        
        .error-message {{
            margin-top: 10px;
            padding: 10px;
            background: #fff;
            border-left: 3px solid #dc3545;
            font-family: 'Courier New', monospace;
            font-size: 12px;
            color: #721c24;
            overflow-x: auto;
        }}
        
        .footer {{
            background: #343a40;
            color: white;
            padding: 20px;
            text-align: center;
            font-size: 14px;
        }}
        
        .progress-bar {{
            width: 100%;
            height: 8px;
            background: #e9ecef;
            border-radius: 4px;
            overflow: hidden;
            margin-top: 20px;
        }}
        
        .progress-fill {{
            height: 100%;
            background: linear-gradient(90deg, #28a745 0%, #20c997 100%);
            transition: width 1s ease;
        }}
    </style>
</head>
<body>
    <div class="container">
        <div class="header">
            <h1>🎮 Evoker-Engine Test Results</h1>
            <div class="subtitle">Generated on {datetime.datetime.now().strftime("%Y-%m-%d %H:%M:%S")}</div>
        </div>
        
        <div class="summary">
            <div class="summary-card">
                <div class="number total">{total}</div>
                <div class="label">Total Tests</div>
            </div>
            <div class="summary-card">
                <div class="number passed">✓ {passed}</div>
                <div class="label">Passed</div>
            </div>
            <div class="summary-card">
                <div class="number failed">✗ {failed}</div>
                <div class="label">Failed</div>
            </div>
            <div class="summary-card">
                <div class="number skipped">⊘ {skipped}</div>
                <div class="label">Skipped</div>
            </div>
        </div>
        
        <div style="padding: 0 40px;">
            <div class="progress-bar">
                <div class="progress-fill" style="width: {(passed/total*100) if total > 0 else 0}%"></div>
            </div>
        </div>
        
        <div class="tests">
            <h2>Test Details</h2>
'''

# Add test results
for test in sorted(test_results, key=lambda x: x['name']):
    status_class = test['outcome'].lower()
    status_icon = '✓' if test['outcome'] == 'Passed' else '✗'
    
    html += f'''
            <div class="test-item {status_class}">
                <div class="test-name">{test['name']}</div>
                <div class="test-duration">{test['duration']:.0f}ms</div>
                <div class="test-status {status_class}">{status_icon} {test['outcome']}</div>
            </div>
'''
    
    if test['error']:
        html += f'''
            <div class="error-message">{test['error']}</div>
'''

html += f'''
        </div>
        
        <div class="footer">
            <strong>Evoker-Engine</strong> - A complete C# game engine with Vulkan rendering<br>
            Test Suite Execution: {(passed/total*100) if total > 0 else 0:.1f}% Success Rate
        </div>
    </div>
</body>
</html>
'''

# Write HTML file
output_file = 'TestResults/TestResults.html'
with open(output_file, 'w', encoding='utf-8') as f:
    f.write(html)

print(f"HTML report generated: {output_file}")
print(f"Tests: {total} total, {passed} passed, {failed} failed, {skipped} skipped")
PYTHON_SCRIPT

# Check if HTML was generated
if [ -f "TestResults/TestResults.html" ]; then
    echo -e "${GREEN}✓ HTML report generated successfully!${NC}"
    echo "  Location: TestResults/TestResults.html"
    echo ""
    echo "Open the report with:"
    echo "  xdg-open TestResults/TestResults.html  # Linux"
    echo "  open TestResults/TestResults.html      # macOS"
    echo "  start TestResults/TestResults.html     # Windows"
else
    echo -e "${RED}✗ Failed to generate HTML report${NC}"
fi

echo ""
if [ $TEST_EXIT_CODE -eq 0 ]; then
    echo -e "${GREEN}✓ All tests passed!${NC}"
else
    echo -e "${RED}✗ Some tests failed${NC}"
fi

exit $TEST_EXIT_CODE

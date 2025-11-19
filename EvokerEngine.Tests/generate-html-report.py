#!/usr/bin/env python3
"""
HTML Test Report Generator for Evoker-Engine
Converts TRX test results to beautiful HTML reports
"""
import xml.etree.ElementTree as ET
import datetime
import os
import sys
import glob

def find_trx_file():
    """Find the most recent TRX file"""
    trx_files = glob.glob("**/TestResults/**/*.trx", recursive=True)
    if not trx_files:
        trx_files = glob.glob("**/*.trx", recursive=True)
    
    if not trx_files:
        print("No TRX file found!", file=sys.stderr)
        return None
    
    # Get the most recent file
    trx_files.sort(key=os.path.getmtime, reverse=True)
    return trx_files[0]

def parse_trx(trx_file):
    """Parse TRX file and extract test results"""
    print(f"Processing: {trx_file}")
    
    tree = ET.parse(trx_file)
    root = tree.getroot()
    
    # XML namespace
    ns = {'': 'http://microsoft.com/schemas/VisualStudio/TeamTest/2010'}
    
    # Get summary
    summary = root.find('.//ResultSummary', ns)
    counters = summary.find('Counters', ns) if summary is not None else None
    
    total = int(counters.get('total', 0)) if counters is not None else 0
    passed = int(counters.get('passed', 0)) if counters is not None else 0
    failed = int(counters.get('failed', 0)) if counters is not None else 0
    skipped = int(counters.get('inconclusive', 0)) if counters is not None else 0
    
    # Get test results
    results_elem = root.find('.//Results', ns)
    test_results = []
    
    if results_elem is not None:
        for result in results_elem.findall('UnitTestResult', ns):
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
                    stack = error_info.find('StackTrace', ns)
                    if message is not None:
                        error_msg = message.text or ""
                    if stack is not None and stack.text:
                        error_msg += "\n\n" + stack.text
            
            test_results.append({
                'name': test_name,
                'outcome': outcome,
                'duration': duration_ms,
                'error': error_msg
            })
    
    return {
        'total': total,
        'passed': passed,
        'failed': failed,
        'skipped': skipped,
        'tests': test_results
    }

def generate_html(data, output_file):
    """Generate HTML report from test data"""
    
    total = data['total']
    passed = data['passed']
    failed = data['failed']
    skipped = data['skipped']
    tests = data['tests']
    
    pass_rate = (passed / total * 100) if total > 0 else 0
    
    html = f'''<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Evoker-Engine Test Results</title>
    <style>
        * {{ margin: 0; padding: 0; box-sizing: border-box; }}
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
            padding: 15px;
            background: #fff;
            border-left: 3px solid #dc3545;
            font-family: 'Courier New', monospace;
            font-size: 12px;
            color: #721c24;
            overflow-x: auto;
            white-space: pre-wrap;
            word-wrap: break-word;
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
        .filter-buttons {{
            margin-bottom: 20px;
            display: flex;
            gap: 10px;
        }}
        .filter-btn {{
            padding: 8px 16px;
            border: 2px solid #ddd;
            background: white;
            border-radius: 6px;
            cursor: pointer;
            font-weight: 500;
            transition: all 0.2s;
        }}
        .filter-btn:hover {{
            background: #f8f9fa;
        }}
        .filter-btn.active {{
            background: #667eea;
            color: white;
            border-color: #667eea;
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
                <div class="progress-fill" style="width: {pass_rate:.1f}%"></div>
            </div>
        </div>
        
        <div class="tests">
            <h2>Test Details ({len(tests)} tests)</h2>
            
            <div class="filter-buttons">
                <button class="filter-btn active" onclick="filterTests('all')">All Tests</button>
                <button class="filter-btn" onclick="filterTests('passed')">✓ Passed ({passed})</button>
                <button class="filter-btn" onclick="filterTests('failed')">✗ Failed ({failed})</button>
            </div>
            
            <div id="test-list">
'''
    
    # Add test results
    for test in sorted(tests, key=lambda x: (x['outcome'] != 'Passed', x['name'])):
        status_class = test['outcome'].lower()
        status_icon = '✓' if test['outcome'] == 'Passed' else '✗'
        
        html += f'''
            <div class="test-item {status_class}" data-status="{status_class}">
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
        </div>
        
        <div class="footer">
            <strong>Evoker-Engine</strong> - A complete C# game engine with Vulkan rendering<br>
            Test Suite Execution: {pass_rate:.1f}% Success Rate • {total} Tests • {passed} Passed • {failed} Failed
        </div>
    </div>
    
    <script>
        function filterTests(filter) {{
            // Update button states
            document.querySelectorAll('.filter-btn').forEach(btn => {{
                btn.classList.remove('active');
            }});
            event.target.classList.add('active');
            
            // Filter test items
            const items = document.querySelectorAll('.test-item');
            items.forEach(item => {{
                if (filter === 'all') {{
                    item.style.display = 'flex';
                }} else {{
                    item.style.display = item.dataset.status === filter ? 'flex' : 'none';
                }}
            }});
            
            // Also hide/show error messages
            const errors = document.querySelectorAll('.error-message');
            errors.forEach(error => {{
                const prevItem = error.previousElementSibling;
                error.style.display = prevItem.style.display === 'flex' ? 'block' : 'none';
            }});
        }}
    </script>
</body>
</html>
'''
    
    with open(output_file, 'w', encoding='utf-8') as f:
        f.write(html)
    
    print(f"HTML report generated: {output_file}")
    print(f"Tests: {total} total, {passed} passed, {failed} failed, {skipped} skipped")
    print(f"Success rate: {pass_rate:.1f}%")

def main():
    """Main entry point"""
    trx_file = find_trx_file()
    if not trx_file:
        sys.exit(1)
    
    data = parse_trx(trx_file)
    
    # Determine output location
    output_file = os.path.join(os.path.dirname(trx_file), 'TestResults.html')
    
    generate_html(data, output_file)
    
    # Return exit code based on test results
    return 0 if data['failed'] == 0 else 1

if __name__ == '__main__':
    sys.exit(main())

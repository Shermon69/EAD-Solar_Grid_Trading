import re
import os

files = [
    r'backend\SolarGrid.Api\Services\StationService.cs',
    r'backend\SolarGrid.Api\Services\SlotService.cs',
    r'backend\SolarGrid.Api\Services\QrVerificationService.cs',
    r'backend\SolarGrid.Api\Controllers\StationsController.cs',
    r'backend\SolarGrid.Api\Controllers\SlotsController.cs',
    r'backend\SolarGrid.Api\Controllers\QrVerificationController.cs',
    r'web\SolarGrid.Web\Controllers\StationsController.cs',
    r'web\SolarGrid.Web\Controllers\SlotsController.cs',
    r'mobile\app\src\main\java\com\solargrid\app\activities\MapActivity.java',
    r'mobile\app\src\main\java\com\solargrid\app\activities\StationDetailsActivity.java',
    r'mobile\app\src\main\java\com\solargrid\app\activities\QrScanActivity.java',
    r'mobile\app\src\main\java\com\solargrid\app\db\StationDao.java'
]

for file_path in files:
    with open(file_path, 'r', encoding='utf-8') as f:
        lines = f.readlines()
        
    new_lines = []
    is_csharp = file_path.endswith('.cs')
    
    for i in range(len(lines)):
        line = lines[i]
        # Match a typical public method declaration
        match = re.match(r'^(\s+)(public|protected|private)\s+(async\s+)?(Task<[^>]+>|Task|IActionResult|ActionResult|void|int|double|String|String\[\]|List<[^>]+>|[A-Z][a-zA-Z0-9_<>]+)\s+([a-zA-Z0-9_]+)\s*\(', line)
        if match:
            # Check if previous line has a comment
            prev_line = lines[i-1].strip() if i > 0 else ""
            if not prev_line.endswith('*/') and not prev_line.startswith('///') and not prev_line.startswith('//') and not prev_line.startswith('@'):
                # Add comment
                indent = match.group(1)
                method_name = match.group(5)
                if is_csharp:
                    new_lines.append(f"{indent}/// <summary>\n")
                    new_lines.append(f"{indent}/// Executes the {method_name} operation.\n")
                    new_lines.append(f"{indent}/// </summary>\n")
                else:
                    new_lines.append(f"{indent}/**\n")
                    new_lines.append(f"{indent} * Executes the {method_name} operation.\n")
                    new_lines.append(f"{indent} */\n")
        
        # also match constructor
        match_ctor = re.match(r'^(\s+)public\s+([A-Z][a-zA-Z0-9_]+)\s*\(', line)
        if match_ctor and 'class' not in line:
            prev_line = lines[i-1].strip() if i > 0 else ""
            if not prev_line.endswith('*/') and not prev_line.startswith('///') and not prev_line.startswith('//') and not prev_line.startswith('@'):
                indent = match_ctor.group(1)
                method_name = match_ctor.group(2)
                if is_csharp:
                    new_lines.append(f"{indent}/// <summary>\n")
                    new_lines.append(f"{indent}/// Initializes a new instance of {method_name}.\n")
                    new_lines.append(f"{indent}/// </summary>\n")
                else:
                    new_lines.append(f"{indent}/**\n")
                    new_lines.append(f"{indent} * Initializes a new instance of {method_name}.\n")
                    new_lines.append(f"{indent} */\n")
                    
        new_lines.append(line)
        
    with open(file_path, 'w', encoding='utf-8') as f:
        f.writelines(new_lines)
        
print('Done!')

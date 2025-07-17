# Fix Markdown Linting Issues
# This script fixes common MD040 issues (missing language specifiers)

$markdownFiles = Get-ChildItem -Path "." -Recurse -Filter "*.md" | Where-Object { $_.FullName -notmatch "node_modules|\.git" }

foreach ($file in $markdownFiles) {
    Write-Host "Processing: $($file.Name)" -ForegroundColor Green
    
    $content = Get-Content $file.FullName -Raw
    
    # Fix MD040 - Add language specifiers to code blocks
    $content = $content -replace '(?m)^```$', '```text'
    $content = $content -replace '(?m)^```\s*\r?\n├', '```text' + [Environment]::NewLine + '├'
    $content = $content -replace '(?m)^```\s*\r?\n❌', '```text' + [Environment]::NewLine + '❌'
    $content = $content -replace '(?m)^```\s*\r?\n📁', '```text' + [Environment]::NewLine + '📁'
    $content = $content -replace '(?m)^```\s*\r?\n#', '```bash' + [Environment]::NewLine + '#'
    $content = $content -replace '(?m)^```\s*\r?\ndotnet', '```bash' + [Environment]::NewLine + 'dotnet'
    $content = $content -replace '(?m)^```\s*\r?\nnpm', '```bash' + [Environment]::NewLine + 'npm'
    $content = $content -replace '(?m)^```\s*\r?\ngit', '```bash' + [Environment]::NewLine + 'git'
    $content = $content -replace '(?m)^```\s*\r?\n\.\/', '```powershell' + [Environment]::NewLine + './'
    $content = $content -replace '(?m)^```\s*\r?\nSELECT', '```sql' + [Environment]::NewLine + 'SELECT'
    $content = $content -replace '(?m)^```\s*\r?\nUSE', '```sql' + [Environment]::NewLine + 'USE'
    $content = $content -replace '(?m)^```\s*\r?\nCREATE', '```sql' + [Environment]::NewLine + 'CREATE'
    $content = $content -replace '(?m)^```\s*\r?\n\{', '```json' + [Environment]::NewLine + '{'
    $content = $content -replace '(?m)^```\s*\r?\n<', '```xml' + [Environment]::NewLine + '<'
    
    # Fix MD009 - Remove trailing spaces
    $content = $content -replace '\s+$', ''
    
    # Write back to file
    Set-Content -Path $file.FullName -Value $content -NoNewline
    
    Write-Host "Fixed: $($file.Name)" -ForegroundColor Yellow
}

Write-Host "All markdown files processed!" -ForegroundColor Cyan

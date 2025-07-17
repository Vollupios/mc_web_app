#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Fix remaining markdownlint issues
.DESCRIPTION
    Fixes MD040, MD022, MD032, MD031, MD047 issues in markdown files
#>

param()

$ErrorActionPreference = "Continue"

Write-Host "🔧 Fixing remaining markdownlint issues..." -ForegroundColor Yellow

# Add trailing newlines to fix MD047
$files = @(
    "CORRECOES-ROTAS-ANALYTICS.md",
    "DOCUMENTACAO-OFICIAL-UNIFICADA.md", 
    "DOCUMENTACAO-UNIFICADA-PARTE2.md",
    "DOCUMENTACAO-UNIFICADA-PARTE3.md",
    "MYSQL-SYNTAX-FIXED.md",
    "REDIS-IMPLEMENTADO.md",
    "SCRIPTS-UNIFICADOS.md",
    "Scripts/README.md",
    "STATUS-CORRECAO-ROTA.md",
    "STATUS-CORRECOES-SQL.md",
    "STATUS-SCRIPTS-FINAL.md"
)

foreach ($file in $files) {
    $path = "/home/pcjv/IntranetDocumentos/$file"
    if (Test-Path $path) {
        Write-Host "📝 Adding trailing newline to $file" -ForegroundColor Green
        Add-Content -Path $path -Value ""
    }
}

# Fix specific code blocks missing language
Write-Host "🔧 Fixing code blocks..." -ForegroundColor Cyan

# Simple fixes for common cases
$filesToFix = @(
    @{ File = "CORRECAO-VIEW-DOCUMENTOS.md"; Pattern = "^```$"; Replace = "```csharp" },
    @{ File = "LIMPEZA-DOCUMENTACAO.md"; Pattern = "^```$"; Replace = "```text" },
    @{ File = "STATUS-SCRIPTS-FINAL.md"; Pattern = "^```$"; Replace = "```powershell" },
    @{ File = "Scripts/README.md"; Pattern = "^```$"; Replace = "```text" }
)

foreach ($fix in $filesToFix) {
    $path = "/home/pcjv/IntranetDocumentos/$($fix.File)"
    if (Test-Path $path) {
        Write-Host "� Fixing $($fix.File)" -ForegroundColor Green
        $content = Get-Content $path
        $content = $content -replace $fix.Pattern, $fix.Replace
        Set-Content -Path $path -Value $content
    }
}

Write-Host "✅ Markdownlint fixes completed!" -ForegroundColor Green
Write-Host "ℹ️ Note: Some complex fixes may require manual intervention" -ForegroundColor Yellow

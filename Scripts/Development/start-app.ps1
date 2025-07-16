#!/usr/bin/env pwsh
Set-Location "C:\Users\Usuário\mc_web_app-main"
Write-Host "=== Verificando status atual do projeto ==="
dotnet build IntranetDocumentos.csproj
if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ Build bem-sucedido"
    Write-Host "=== Executando aplicação ==="
    dotnet run --project IntranetDocumentos.csproj
} else {
    Write-Host "✗ Build falhou"
    exit 1
}

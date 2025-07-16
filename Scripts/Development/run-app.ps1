#!/usr/bin/env pwsh
# Script para executar a aplicação IntranetDocumentos
Write-Host "Iniciando IntranetDocumentos..." -ForegroundColor Green
Write-Host "A aplicação estará disponível em:" -ForegroundColor Yellow
Write-Host "  HTTPS: https://localhost:7168" -ForegroundColor Cyan
Write-Host "  HTTP:  http://localhost:5098" -ForegroundColor Cyan
dotnet run --project IntranetDocumentos.csproj

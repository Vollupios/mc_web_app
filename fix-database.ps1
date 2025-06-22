# Script para corrigir problemas de migração
Set-Location "C:\Users\Usuário\mc_web_app-main"
Write-Host "Removendo database existente..."
Remove-Item "IntranetDocumentos.db*" -Force -ErrorAction SilentlyContinue
Write-Host "Removendo migrações existentes..."
Remove-Item "Migrations" -Recurse -Force -ErrorAction SilentlyContinue
Write-Host "Criando nova migração inicial..."
dotnet ef migrations add InitialCreateWithAnalytics
Write-Host "Aplicando migração..."
dotnet ef database update
Write-Host "Concluído!"

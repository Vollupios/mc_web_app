@echo off
echo === Building IntranetDocumentos Project ===

cd "c:\Users\Usuário\mc_web_app-main"

echo Cleaning and restoring packages...
dotnet clean
dotnet restore

echo Building project...
dotnet build --configuration Release

if %ERRORLEVEL% EQU 0 (
    echo ✅ Build successful!
    
    echo Creating migration for DocumentDownload...
    dotnet ef migrations add AddDocumentDownloadAnalytics --force
    
    echo Applying database updates...
    dotnet ef database update
    
    echo ✅ Analytics implementation completed successfully!
    echo.
    echo 📊 Analytics Features Available:
    echo - Dashboard: /Analytics/Dashboard
    echo - Document Statistics: /Analytics/DocumentStatistics  
    echo - Meeting Metrics: /Analytics/MeetingMetrics
    echo - Department Activity: /Analytics/DepartmentActivity
    echo.
    echo 🔒 Access: Admin and Gestor roles only
    
) else (
    echo ❌ Build failed! Please check the errors above.
    exit /b 1
)

echo === Analytics Implementation Complete ===
pause

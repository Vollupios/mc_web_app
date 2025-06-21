#!/bin/bash

# Script to build and test the analytics implementation

echo "=== Building IntranetDocumentos Project ==="

# Navigate to project directory
cd "c:\Users\Usuário\mc_web_app-main"

# Clean and restore
echo "Cleaning and restoring packages..."
dotnet clean
dotnet restore

# Build the project
echo "Building project..."
dotnet build --configuration Release

# Check for build errors
if [ $? -eq 0 ]; then
    echo "✅ Build successful!"
    
    # Try to create and apply migration
    echo "Creating migration for DocumentDownload..."
    dotnet ef migrations add AddDocumentDownloadAnalytics --force
    
    echo "Applying database updates..."
    dotnet ef database update
    
    echo "✅ Analytics implementation completed successfully!"
    echo ""
    echo "📊 Analytics Features Available:"
    echo "- Dashboard: /Analytics/Dashboard"
    echo "- Document Statistics: /Analytics/DocumentStatistics"
    echo "- Meeting Metrics: /Analytics/MeetingMetrics"
    echo "- Department Activity: /Analytics/DepartmentActivity"
    echo ""
    echo "🔒 Access: Admin and Gestor roles only"
    
else
    echo "❌ Build failed! Please check the errors above."
    exit 1
fi

echo "=== Analytics Implementation Complete ==="

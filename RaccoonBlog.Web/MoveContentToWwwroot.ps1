# PowerShell script to move Content folder to wwwroot for ASP.NET Core migration
# Run this from the RaccoonBlog.Web directory

$webRoot = $PSScriptRoot
$contentPath = Join-Path $webRoot "Content"
$wwwrootPath = Join-Path $webRoot "wwwroot"

Write-Host "Moving Content folder to wwwroot..." -ForegroundColor Cyan

# Create wwwroot if it doesn't exist
if (-not (Test-Path $wwwrootPath)) {
    New-Item -Path $wwwrootPath -ItemType Directory | Out-Null
    Write-Host "Created wwwroot folder" -ForegroundColor Green
}

# Move Content folder contents to wwwroot
if (Test-Path $contentPath) {
    # Get all items in Content folder
    $items = Get-ChildItem -Path $contentPath -Recurse
    
    foreach ($item in $items) {
        $relativePath = $item.FullName.Substring($contentPath.Length + 1)
        $destinationPath = Join-Path $wwwrootPath $relativePath
        
        if ($item.PSIsContainer) {
            # Create directory
            if (-not (Test-Path $destinationPath)) {
                New-Item -Path $destinationPath -ItemType Directory | Out-Null
            }
        } else {
            # Copy file
            $destDir = Split-Path $destinationPath -Parent
            if (-not (Test-Path $destDir)) {
                New-Item -Path $destDir -ItemType Directory -Force | Out-Null
            }
            Copy-Item -Path $item.FullName -Destination $destinationPath -Force
            Write-Host "Copied: $relativePath" -ForegroundColor Gray
        }
    }
    
    Write-Host "`nContent folder copied to wwwroot successfully!" -ForegroundColor Green
    Write-Host "Original Content folder preserved for reference" -ForegroundColor Yellow
} else {
    Write-Host "Content folder not found at: $contentPath" -ForegroundColor Red
}

# Handle Admin area Content
$adminContentPath = Join-Path $webRoot "Areas\Admin\Content"
$adminWwwrootPath = Join-Path $wwwrootPath "admin"

if (Test-Path $adminContentPath) {
    if (-not (Test-Path $adminWwwrootPath)) {
        New-Item -Path $adminWwwrootPath -ItemType Directory | Out-Null
    }
    
    $adminItems = Get-ChildItem -Path $adminContentPath -Recurse
    
    foreach ($item in $adminItems) {
        $relativePath = $item.FullName.Substring($adminContentPath.Length + 1)
        $destinationPath = Join-Path $adminWwwrootPath $relativePath
        
        if ($item.PSIsContainer) {
            if (-not (Test-Path $destinationPath)) {
                New-Item -Path $destinationPath -ItemType Directory | Out-Null
            }
        } else {
            $destDir = Split-Path $destinationPath -Parent
            if (-not (Test-Path $destDir)) {
                New-Item -Path $destDir -ItemType Directory -Force | Out-Null
            }
            Copy-Item -Path $item.FullName -Destination $destinationPath -Force
            Write-Host "Copied: admin\$relativePath" -ForegroundColor Gray
        }
    }
    
    Write-Host "`nAdmin Content folder copied to wwwroot/admin successfully!" -ForegroundColor Green
}

Write-Host "`nNext steps:" -ForegroundColor Cyan
Write-Host "1. Update view files to use new paths (~/ instead of ~/Content/)" -ForegroundColor White
Write-Host "2. Update Admin views to use ~/admin/ instead of ~/Areas/Admin/Content/" -ForegroundColor White
Write-Host "3. Test the application" -ForegroundColor White
Write-Host "4. After verification, you can delete the original Content folders" -ForegroundColor White

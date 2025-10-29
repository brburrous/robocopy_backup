# BackupScript.ps1
# This is a TEMPLATE - Replace with your actual backup script
#
# This script will be called by the NAS Backup Application with the following parameters:
# - NasAddress: The IP or hostname of your NAS
# - ShareName: The network share name
# - SourcePath: The directory to backup
# - ArchivePath: Where to store the backup
# - Compress: Optional switch flag for compression

param(
    [Parameter(Mandatory=$true)]
    [string]$NasAddress,
    
    [Parameter(Mandatory=$true)]
    [string]$ShareName,
    
    [Parameter(Mandatory=$true)]
    [string]$SourcePath,
    
    [Parameter(Mandatory=$true)]
    [string]$ArchivePath,
    
    [Parameter(Mandatory=$false)]
    [switch]$Compress
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "NAS Backup Script (TEMPLATE)" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "Parameters Received:" -ForegroundColor Yellow
Write-Host "  NAS Address: $NasAddress"
Write-Host "  Share Name: $ShareName"
Write-Host "  Source Path: $SourcePath"
Write-Host "  Archive Path: $ArchivePath"
Write-Host "  Compress: $($Compress.IsPresent)"
Write-Host ""

Write-Host "IMPORTANT: This is a template script!" -ForegroundColor Red
Write-Host "Replace this file with your actual backup implementation." -ForegroundColor Red
Write-Host ""

# Example: Check if compress flag is set
if ($Compress) {
    Write-Host "Compression is ENABLED" -ForegroundColor Green
    Write-Host "TODO: Implement your compression logic here"
} else {
    Write-Host "Compression is DISABLED" -ForegroundColor Yellow
    Write-Host "TODO: Implement your standard backup logic here"
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Template Script Complete" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# Exit with success
exit 0

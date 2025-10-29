# NAS Backup Application

A cross-platform desktop application built with Avalonia UI for backing up data to a NAS (Network Attached Storage) with support for multiple named configurations, persistent settings, and optional data compression.

## Features

- **Settings Management**: Configuration hidden behind a dedicated settings window
- **Multiple Named Configurations**: Create, save, load, and manage multiple backup configurations
- **Persistent Storage**: All configurations are automatically saved and restored between sessions
- **Compression Support**: Optional data compression flag passed to your PowerShell script
- **PowerShell Integration**: Runs backup operations via your custom PowerShell script
- **Real-time Logging**: Console-style output showing backup progress
- **Modern UI**: Built with Avalonia's Fluent theme

## Prerequisites

- .NET 8.0 SDK or later
- Windows (for PowerShell script execution)
- PowerShell 5.1 or later
- Your own PowerShell backup script (see requirements below)

## PowerShell Script Requirements

The application expects a PowerShell script named `BackupScript.ps1` in the same directory as the executable. Your script should accept the following parameters:

```powershell
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
```

### Parameters Explained:
- **NasAddress**: The IP address or hostname of your NAS
- **ShareName**: The network share name on the NAS
- **SourcePath**: The local directory to back up
- **ArchivePath**: The local directory where backups should be stored
- **Compress**: Optional switch flag indicating whether to compress the backup

### Example Script Structure:
```powershell
param(
    [string]$NasAddress,
    [string]$ShareName,
    [string]$SourcePath,
    [string]$ArchivePath,
    [switch]$Compress
)

Write-Host "Starting backup..."

if ($Compress) {
    Write-Host "Compression enabled"
    # Your compression logic here
} else {
    Write-Host "Compression disabled"
    # Your standard backup logic here
}

# Your backup implementation...
```

## Building the Application

1. Navigate to the project directory:
   ```bash
   cd NasBackupApp
   ```

2. Restore dependencies:
   ```bash
   dotnet restore
   ```

3. Build the project:
   ```bash
   dotnet build
   ```

4. Run the application:
   ```bash
   dotnet run
   ```

## Publishing

To create a standalone executable:

```bash
# For Windows
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true

# For Linux
dotnet publish -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true

# For macOS
dotnet publish -c Release -r osx-x64 --self-contained true -p:PublishSingleFile=true
```

The executable will be in `bin/Release/net8.0/{runtime}/publish/`

**Important**: Place your `BackupScript.ps1` file in the same directory as the published executable.

## Usage

### First-Time Setup

1. **Open Settings**: Click the "⚙ Settings" button
2. **Create Configuration**: Click "New" to create a new configuration
3. **Configure Settings**:
   - Enter a name for the configuration
   - Enter NAS Address (e.g., `192.168.1.100`)
   - Enter Share Name (e.g., `Backups`)
   - Browse and select Source Path
   - Browse and select Archive Path
   - Check "Compress data" if you want compression
4. **Save Configuration**: Click "Save"
5. **Apply Configuration**: Click "Apply" to use this configuration

### Running a Backup

1. Ensure a configuration is loaded (shown at the top of the main window)
2. Optionally toggle the "Compress data during backup" checkbox
3. Click "Start Backup"
4. Monitor progress in the log console

### Managing Configurations

- **Load Configuration**: Open Settings → Select from dropdown
- **Create New**: Open Settings → Click "New"
- **Edit Existing**: Open Settings → Select configuration → Modify fields → Click "Save"
- **Delete Configuration**: Open Settings → Select configuration → Click "Delete"
- **Switch Configurations**: Open Settings → Select different configuration → Click "Apply"

## Configuration Storage

Configurations are stored in JSON format at:
- **Windows**: `%APPDATA%\NasBackupApp\configurations.json`
- **Linux**: `~/.config/NasBackupApp/configurations.json`
- **macOS**: `~/Library/Application Support/NasBackupApp/configurations.json`

## Troubleshooting

### PowerShell Script Not Found

If you see an error about the script not being found:
1. Ensure `BackupScript.ps1` is in the same directory as the executable
2. Check the log for the expected path
3. Place your script at the indicated location

### PowerShell Execution Policy

If you encounter execution policy errors, run PowerShell as Administrator and execute:
```powershell
Set-ExecutionPolicy RemoteSigned -Scope CurrentUser
```

### NAS Connection Issues

- Ensure the NAS is on the same network
- Verify the share name is correct
- Check that you have proper permissions to access the share
- Try mapping the network drive manually first: `net use Z: \\NAS_ADDRESS\SHARE_NAME`

### Configuration Not Saving

- Check that the application has write permissions to the AppData directory
- On first run, the directory will be created automatically
- Check the application logs for any error messages

## Customization

### Modifying the UI

Edit `Views/MainWindow.axaml` or `Views/SettingsWindow.axaml` to customize:
- Window size and layout
- Colors and styling
- Button labels
- Log console appearance

### Extending Configuration Options

To add new configuration fields:
1. Add properties to `Models/BackupConfiguration.cs`
2. Add UI controls to `Views/SettingsWindow.axaml`
3. Update `SettingsWindow.axaml.cs` to bind the new fields
4. Update `MainWindow.axaml.cs` to pass new parameters to the PowerShell script

## License

This is a sample application. Modify and use as needed for your projects.

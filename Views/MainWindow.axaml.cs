using Avalonia.Controls;
using Avalonia.Interactivity;
using NasBackupApp.Models;
using NasBackupApp.Services;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace NasBackupApp.Views
{
    public partial class MainWindow : Window
    {
        private readonly ConfigurationManager _configManager;
        private BackupConfiguration? _currentConfiguration;

        public MainWindow()
        {
            InitializeComponent();
            
            _configManager = new ConfigurationManager();
            
            // Wire up event handlers
            SettingsButton.Click += SettingsButton_Click;
            StartBackupButton.Click += StartBackupButton_Click;

            // Load the last used configuration or default
            LoadLastConfiguration();
        }

        private void LoadLastConfiguration()
        {
            var configurations = _configManager.LoadConfigurations();
            if (configurations.Count > 0)
            {
                _currentConfiguration = configurations[0];
                UpdateConfigurationDisplay();
            }
        }

        private async void SettingsButton_Click(object? sender, RoutedEventArgs e)
        {
            var settingsWindow = new SettingsWindow();
            settingsWindow.ConfigurationApplied += SettingsWindow_ConfigurationApplied;
            await settingsWindow.ShowDialog(this);
        }

        private void SettingsWindow_ConfigurationApplied(object? sender, BackupConfiguration config)
        {
            _currentConfiguration = config;
            UpdateConfigurationDisplay();
            AppendLog($"Configuration '{config.Name}' loaded successfully.");
        }

        private void UpdateConfigurationDisplay()
        {
            if (_currentConfiguration != null)
            {
                CurrentConfigTextBlock.Text = _currentConfiguration.Name;
                CurrentConfigTextBlock.FontStyle = Avalonia.Media.FontStyle.Normal;
                
                ConfigStatusTextBlock.Text = $"NAS: {_currentConfiguration.NasAddress} | " +
                                            $"Share: {_currentConfiguration.ShareName} | " +
                                            $"Source: {_currentConfiguration.SourcePath} | " +
                                            $"Archive: {_currentConfiguration.ArchivePath}";
                
                CompressDataCheckBox.IsChecked = _currentConfiguration.CompressData;
                CompressDataCheckBox.IsEnabled = true;
                StartBackupButton.IsEnabled = true;
            }
            else
            {
                CurrentConfigTextBlock.Text = "None";
                CurrentConfigTextBlock.FontStyle = Avalonia.Media.FontStyle.Italic;
                ConfigStatusTextBlock.Text = "No configuration loaded. Click Settings to create or load a configuration.";
                StartBackupButton.IsEnabled = false;
                CompressDataCheckBox.IsEnabled = false;
            }
        }

        private async void StartBackupButton_Click(object? sender, RoutedEventArgs e)
        {
            if (_currentConfiguration == null)
            {
                AppendLog("Error: No configuration loaded. Please configure settings first.");
                return;
            }

            // Validate configuration
            if (string.IsNullOrWhiteSpace(_currentConfiguration.NasAddress))
            {
                AppendLog("Error: NAS Address is not configured.");
                return;
            }

            if (string.IsNullOrWhiteSpace(_currentConfiguration.ShareName))
            {
                AppendLog("Error: Share Name is not configured.");
                return;
            }

            if (string.IsNullOrWhiteSpace(_currentConfiguration.SourcePath))
            {
                AppendLog("Error: Source Path is not configured.");
                return;
            }

            if (string.IsNullOrWhiteSpace(_currentConfiguration.ArchivePath))
            {
                AppendLog("Error: Archive Path is not configured.");
                return;
            }

            // Disable controls during backup
            StartBackupButton.IsEnabled = false;
            SettingsButton.IsEnabled = false;
            CompressDataCheckBox.IsEnabled = false;
            LogTextBox.Text = string.Empty;
            
            AppendLog("========================================");
            AppendLog("Starting backup process...");
            AppendLog($"Configuration: {_currentConfiguration.Name}");
            AppendLog($"NAS Address: {_currentConfiguration.NasAddress}");
            AppendLog($"Share Name: {_currentConfiguration.ShareName}");
            AppendLog($"Source Path: {_currentConfiguration.SourcePath}");
            AppendLog($"Archive Path: {_currentConfiguration.ArchivePath}");
            AppendLog($"Compress Data: {(CompressDataCheckBox.IsChecked == true ? "Yes" : "No")}");
            AppendLog("========================================");

            try
            {
                await RunBackupScriptAsync(
                    _currentConfiguration.NasAddress,
                    _currentConfiguration.ShareName,
                    _currentConfiguration.SourcePath,
                    _currentConfiguration.ArchivePath,
                    CompressDataCheckBox.IsChecked == true
                );
                
                AppendLog("========================================");
                AppendLog("Backup completed successfully!");
                AppendLog("========================================");
            }
            catch (Exception ex)
            {
                AppendLog("========================================");
                AppendLog($"Error during backup: {ex.Message}");
                AppendLog("========================================");
            }
            finally
            {
                StartBackupButton.IsEnabled = true;
                SettingsButton.IsEnabled = true;
                CompressDataCheckBox.IsEnabled = true;
            }
        }

        private async Task RunBackupScriptAsync(string nasAddress, string shareName, string sourcePath, string archivePath, bool compress)
        {
            // Path to the PowerShell script (user-provided)
            string scriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BackupScript.ps1");
            
            if (!File.Exists(scriptPath))
            {
                AppendLog($"ERROR: PowerShell script not found at: {scriptPath}");
                AppendLog("Please place your BackupScript.ps1 file in the application directory.");
                AppendLog($"Expected location: {scriptPath}");
                return;
            }

            // Build PowerShell arguments
            var arguments = $"-ExecutionPolicy Bypass -File \"{scriptPath}\" " +
                          $"-NasAddress \"{nasAddress}\" " +
                          $"-ShareName \"{shareName}\" " +
                          $"-SourcePath \"{sourcePath}\" " +
                          $"-ArchivePath \"{archivePath}\"";

            // Add compress flag if enabled
            if (compress)
            {
                arguments += " -Compress";
            }

            var startInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = startInfo };
            
            // Handle output data
            process.OutputDataReceived += (sender, args) =>
            {
                if (!string.IsNullOrEmpty(args.Data))
                {
                    Avalonia.Threading.Dispatcher.UIThread.Post(() => AppendLog(args.Data));
                }
            };

            process.ErrorDataReceived += (sender, args) =>
            {
                if (!string.IsNullOrEmpty(args.Data))
                {
                    Avalonia.Threading.Dispatcher.UIThread.Post(() => AppendLog($"ERROR: {args.Data}"));
                }
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                AppendLog($"Process exited with code: {process.ExitCode}");
            }
        }

        private void AppendLog(string message)
        {
            LogTextBox.Text += $"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}";
            
            // Auto-scroll to bottom
            LogScrollViewer?.ScrollToEnd();
        }
    }
}

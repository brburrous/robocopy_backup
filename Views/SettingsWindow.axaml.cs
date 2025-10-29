using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using NasBackupApp.Models;
using NasBackupApp.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NasBackupApp.Views
{
    public partial class SettingsWindow : Window
    {
        private readonly ConfigurationManager _configManager;
        private List<BackupConfiguration> _configurations;
        private BackupConfiguration? _currentConfiguration;

        public event EventHandler<BackupConfiguration>? ConfigurationApplied;

        public SettingsWindow()
        {
            InitializeComponent();
            _configManager = new ConfigurationManager();
            _configurations = _configManager.LoadConfigurations();

            // Wire up event handlers
            ConfigurationComboBox.SelectionChanged += ConfigurationComboBox_SelectionChanged;
            NewConfigButton.Click += NewConfigButton_Click;
            SaveConfigButton.Click += SaveConfigButton_Click;
            DeleteConfigButton.Click += DeleteConfigButton_Click;
            BrowseSourceButton.Click += BrowseSourceButton_Click;
            BrowseArchiveButton.Click += BrowseArchiveButton_Click;
            ApplyButton.Click += ApplyButton_Click;
            CloseButton.Click += CloseButton_Click;

            LoadConfigurationList();
        }

        private void LoadConfigurationList()
        {
            ConfigurationComboBox.ItemsSource = _configurations.Select(c => c.Name).ToList();
            
            if (_configurations.Any())
            {
                ConfigurationComboBox.SelectedIndex = 0;
            }
        }

        private void ConfigurationComboBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (ConfigurationComboBox.SelectedItem is string configName)
            {
                _currentConfiguration = _configurations.FirstOrDefault(c => c.Name == configName);
                if (_currentConfiguration != null)
                {
                    LoadConfigurationToForm(_currentConfiguration);
                }
            }
        }

        private void LoadConfigurationToForm(BackupConfiguration config)
        {
            ConfigNameTextBox.Text = config.Name;
            NasAddressTextBox.Text = config.NasAddress;
            ShareNameTextBox.Text = config.ShareName;
            SourcePathTextBox.Text = config.SourcePath;
            ArchivePathTextBox.Text = config.ArchivePath;
            CompressDataCheckBox.IsChecked = config.CompressData;
        }

        private BackupConfiguration GetConfigurationFromForm()
        {
            return new BackupConfiguration
            {
                Name = ConfigNameTextBox.Text ?? "Unnamed",
                NasAddress = NasAddressTextBox.Text ?? string.Empty,
                ShareName = ShareNameTextBox.Text ?? string.Empty,
                SourcePath = SourcePathTextBox.Text ?? string.Empty,
                ArchivePath = ArchivePathTextBox.Text ?? string.Empty,
                CompressData = CompressDataCheckBox.IsChecked ?? false
            };
        }

        private void NewConfigButton_Click(object? sender, RoutedEventArgs e)
        {
            var newConfig = new BackupConfiguration
            {
                Name = $"Configuration {_configurations.Count + 1}"
            };

            ConfigNameTextBox.Text = newConfig.Name;
            NasAddressTextBox.Text = string.Empty;
            ShareNameTextBox.Text = string.Empty;
            SourcePathTextBox.Text = string.Empty;
            ArchivePathTextBox.Text = string.Empty;
            CompressDataCheckBox.IsChecked = false;

            _currentConfiguration = newConfig;
        }

        private async void SaveConfigButton_Click(object? sender, RoutedEventArgs e)
        {
            var config = GetConfigurationFromForm();

            if (string.IsNullOrWhiteSpace(config.Name))
            {
                await ShowMessageAsync("Error", "Configuration name cannot be empty.");
                return;
            }

            try
            {
                // Check if renaming existing configuration
                if (_currentConfiguration != null && _currentConfiguration.Name != config.Name)
                {
                    // Remove old configuration
                    _configurations.RemoveAll(c => c.Name == _currentConfiguration.Name);
                }
                else
                {
                    // Remove existing configuration with same name
                    _configurations.RemoveAll(c => c.Name == config.Name);
                }

                _configurations.Add(config);
                _configManager.SaveConfigurations(_configurations);
                _currentConfiguration = config;

                LoadConfigurationList();
                ConfigurationComboBox.SelectedItem = config.Name;

                await ShowMessageAsync("Success", $"Configuration '{config.Name}' saved successfully.");
            }
            catch (Exception ex)
            {
                await ShowMessageAsync("Error", $"Failed to save configuration: {ex.Message}");
            }
        }

        private async void DeleteConfigButton_Click(object? sender, RoutedEventArgs e)
        {
            if (_currentConfiguration == null || string.IsNullOrWhiteSpace(_currentConfiguration.Name))
            {
                await ShowMessageAsync("Error", "No configuration selected.");
                return;
            }

            if (_configurations.Count <= 1)
            {
                await ShowMessageAsync("Error", "Cannot delete the last configuration.");
                return;
            }

            var result = await ShowConfirmAsync("Delete Configuration", 
                $"Are you sure you want to delete the configuration '{_currentConfiguration.Name}'?");

            if (result)
            {
                try
                {
                    _configManager.DeleteConfiguration(_currentConfiguration.Name, _configurations);
                    _configurations = _configManager.LoadConfigurations();
                    LoadConfigurationList();
                    await ShowMessageAsync("Success", "Configuration deleted successfully.");
                }
                catch (Exception ex)
                {
                    await ShowMessageAsync("Error", $"Failed to delete configuration: {ex.Message}");
                }
            }
        }

        private async void BrowseSourceButton_Click(object? sender, RoutedEventArgs e)
        {
            var folder = await SelectFolderAsync("Select Source Directory");
            if (folder != null)
            {
                SourcePathTextBox.Text = folder;
            }
        }

        private async void BrowseArchiveButton_Click(object? sender, RoutedEventArgs e)
        {
            var folder = await SelectFolderAsync("Select Archive Directory");
            if (folder != null)
            {
                ArchivePathTextBox.Text = folder;
            }
        }

        private async Task<string?> SelectFolderAsync(string title)
        {
            var topLevel = GetTopLevel(this);
            if (topLevel == null) return null;

            var folders = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = title,
                AllowMultiple = false
            });

            if (folders.Count > 0)
            {
                return folders[0].Path.LocalPath;
            }

            return null;
        }

        private void ApplyButton_Click(object? sender, RoutedEventArgs e)
        {
            var config = GetConfigurationFromForm();
            ConfigurationApplied?.Invoke(this, config);
            Close();
        }

        private void CloseButton_Click(object? sender, RoutedEventArgs e)
        {
            Close();
        }

        private async Task ShowMessageAsync(string title, string message)
        {
            var messageBox = new Window
            {
                Title = title,
                Width = 400,
                Height = 150,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                CanResize = false
            };

            var panel = new StackPanel { Margin = new Avalonia.Thickness(20) };
            panel.Children.Add(new TextBlock { Text = message, TextWrapping = Avalonia.Media.TextWrapping.Wrap, Margin = new Avalonia.Thickness(0, 0, 0, 20) });
            
            var button = new Button { Content = "OK", Width = 80, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center };
            button.Click += (s, e) => messageBox.Close();
            panel.Children.Add(button);

            messageBox.Content = panel;
            await messageBox.ShowDialog(this);
        }

        private async Task<bool> ShowConfirmAsync(string title, string message)
        {
            var result = false;
            var messageBox = new Window
            {
                Title = title,
                Width = 400,
                Height = 150,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                CanResize = false
            };

            var panel = new StackPanel { Margin = new Avalonia.Thickness(20) };
            panel.Children.Add(new TextBlock { Text = message, TextWrapping = Avalonia.Media.TextWrapping.Wrap, Margin = new Avalonia.Thickness(0, 0, 0, 20) });
            
            var buttonPanel = new StackPanel { Orientation = Avalonia.Layout.Orientation.Horizontal, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center };
            
            var yesButton = new Button { Content = "Yes", Width = 80, Margin = new Avalonia.Thickness(0, 0, 10, 0) };
            yesButton.Click += (s, e) => { result = true; messageBox.Close(); };
            
            var noButton = new Button { Content = "No", Width = 80 };
            noButton.Click += (s, e) => { result = false; messageBox.Close(); };
            
            buttonPanel.Children.Add(yesButton);
            buttonPanel.Children.Add(noButton);
            panel.Children.Add(buttonPanel);

            messageBox.Content = panel;
            await messageBox.ShowDialog(this);
            return result;
        }
    }
}

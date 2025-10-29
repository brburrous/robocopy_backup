using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using NasBackupApp.Models;

namespace NasBackupApp.Services
{
    public class ConfigurationManager
    {
        private readonly string _configDirectory;
        private readonly string _configurationsFile;

        public ConfigurationManager()
        {
            _configDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "NasBackupApp"
            );
            _configurationsFile = Path.Combine(_configDirectory, "configurations.json");

            // Ensure directory exists
            if (!Directory.Exists(_configDirectory))
            {
                Directory.CreateDirectory(_configDirectory);
            }
        }

        public List<BackupConfiguration> LoadConfigurations()
        {
            try
            {
                if (!File.Exists(_configurationsFile))
                {
                    // Return default configuration if no file exists
                    return new List<BackupConfiguration>
                    {
                        new BackupConfiguration { Name = "Default" }
                    };
                }

                string json = File.ReadAllText(_configurationsFile);
                var configs = JsonSerializer.Deserialize<List<BackupConfiguration>>(json);
                return configs ?? new List<BackupConfiguration> { new BackupConfiguration { Name = "Default" } };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading configurations: {ex.Message}");
                return new List<BackupConfiguration> { new BackupConfiguration { Name = "Default" } };
            }
        }

        public void SaveConfigurations(List<BackupConfiguration> configurations)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                string json = JsonSerializer.Serialize(configurations, options);
                File.WriteAllText(_configurationsFile, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving configurations: {ex.Message}");
                throw;
            }
        }

        public void SaveConfiguration(BackupConfiguration configuration, List<BackupConfiguration> allConfigurations)
        {
            var existing = allConfigurations.FirstOrDefault(c => c.Name == configuration.Name);
            if (existing != null)
            {
                // Update existing
                allConfigurations.Remove(existing);
            }
            
            allConfigurations.Add(configuration);
            SaveConfigurations(allConfigurations);
        }

        public void DeleteConfiguration(string configName, List<BackupConfiguration> allConfigurations)
        {
            var config = allConfigurations.FirstOrDefault(c => c.Name == configName);
            if (config != null)
            {
                allConfigurations.Remove(config);
                SaveConfigurations(allConfigurations);
            }
        }
    }
}

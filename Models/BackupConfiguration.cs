using System.Text.Json.Serialization;

namespace NasBackupApp.Models
{
    public class BackupConfiguration
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = "Default";

        [JsonPropertyName("nasAddress")]
        public string NasAddress { get; set; } = string.Empty;

        [JsonPropertyName("shareName")]
        public string ShareName { get; set; } = string.Empty;

        [JsonPropertyName("sourcePath")]
        public string SourcePath { get; set; } = string.Empty;

        [JsonPropertyName("archivePath")]
        public string ArchivePath { get; set; } = string.Empty;

        [JsonPropertyName("compressData")]
        public bool CompressData { get; set; } = false;

        public BackupConfiguration Clone()
        {
            return new BackupConfiguration
            {
                Name = this.Name,
                NasAddress = this.NasAddress,
                ShareName = this.ShareName,
                SourcePath = this.SourcePath,
                ArchivePath = this.ArchivePath,
                CompressData = this.CompressData
            };
        }
    }
}

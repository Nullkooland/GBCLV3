using GBCLV3.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace GBCLV3.Models.Download
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum CheckUpdateStatus
    {
        Unknown,

        [LocalizedDescription("CheckingUpdate")]
        Checking,

        [LocalizedDescription("AlreadyUpToDate")]
        UpToDate,

        [LocalizedDescription("UpdateAvailable")]
        UpdateAvailable,

        [LocalizedDescription("UpdateCheckFailed")]
        CheckFailed,
    }

    // Github Release API
    // See https://developer.github.com/v3/repos/releases/

    public class UpdateAsset
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("browser_download_url")]
        public string Url { get; set; }

        [JsonPropertyName("size")]
        public int Size { get; set; }
    }

    public class UpdateInfo
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("tag_name")]
        public string Version { get; set; }

        [JsonPropertyName("prerelease")]
        public bool PreRelease { get; set; }

        [JsonPropertyName("created_at")]
        public DateTime ReleaseTime { get; set; }

        [JsonPropertyName("body")]
        public string Description { get; set; }

        [JsonPropertyName("assets")]
        public List<UpdateAsset> Assets { get; set; }
    }

    public class UpdateChangelog
    {
        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("details")]
        public string[] Details { get; set; }
    }
}

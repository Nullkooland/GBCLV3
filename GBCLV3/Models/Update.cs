using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Documents;
using GBCLV3.Utils;

namespace GBCLV3.Models
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    enum UpdateStatus
    {
        Unknown,

        [LocalizedDescription("AlreadyUpToDate")]
        UpToDate,

        [LocalizedDescription("UpdateAvailable")]
        UpdateAvailable,

        [LocalizedDescription("UpdateCheckFailed")]
        CheckFailed,
    }

    // Github Release API
    // See https://developer.github.com/v3/repos/releases/

    class UpdateAsset
    {
        [JsonPropertyName("browser_download_url")]
        public string Url { get; set; }

        [JsonPropertyName("size")]
        public int Size { get; set; }
    }

    class UpdateInfo
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

        public bool IsCheckFailed { get; set; }
    }
}

using GBCLV3.Models.Auxiliary;
using System;
using System.Buffers.Text;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace GBCLV3.Services.Auxiliary
{
    class SkinService
    {
        #region Private Fields

        private const string PROFILE_SERVER = "https://sessionserver.mojang.com/session/minecraft/profile/";

        private static readonly HttpClient _client = new HttpClient() { Timeout = TimeSpan.FromSeconds(15) };

        private static readonly JsonSerializerOptions _jsonOptions
            = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        #endregion

        public async ValueTask<string> GetProfileAsync(string uuid)
        {
            try
            {
                string profileJson = await _client.GetStringAsync(PROFILE_SERVER + uuid);
                using var profile = JsonDocument.Parse(profileJson);

                return profile.RootElement
                              .GetProperty("properties")[0]
                              .GetProperty("value")
                              .GetString();
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine(ex.ToString());
            }
            catch (OperationCanceledException)
            {
                Debug.WriteLine("[ERROR] Index json download time out");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            return null;
        }

        public async ValueTask<Skin> GetSkinAsync(string profile)
        {
            try
            {
                string skinJson = Encoding.UTF8.GetString(Convert.FromBase64String(profile));
                using var skinDoc = JsonDocument.Parse(skinJson);
                var textures = skinDoc.RootElement.GetProperty("textures");

                var skin = new Skin();

                if (textures.TryGetProperty("SKIN", out var body))
                {
                    string url = body.GetProperty("url").GetString();
                    skin.IsSlim = body.TryGetProperty("metadata", out _);

                    using var httpStream = await _client.GetStreamAsync(url);
                    skin.Body = await DownloadImageAsync(httpStream);
                }

                if (textures.TryGetProperty("CAPE", out var cape))
                {
                    string url = cape.GetProperty("url").GetString();
                    using var httpStream = await _client.GetStreamAsync(url);
                    skin.Cape = await DownloadImageAsync(httpStream);
                }

                skin.Face = GetFace(skin.Body);
                return skin;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                return null;
            }
        }

        #region Private Methods

        private static async ValueTask<BitmapImage> DownloadImageAsync(Stream httpStream)
        {
            using var memStream = new MemoryStream();
            await httpStream.CopyToAsync(memStream);

            var img = new BitmapImage();
            img.BeginInit();
            img.StreamSource = memStream;
            img.CacheOption = BitmapCacheOption.OnLoad;
            img.EndInit();
            img.Freeze();

            return img;
        }

        private static CroppedBitmap GetFace(BitmapImage body)
        {
            int regionSize = body.PixelWidth / 8;
            return new CroppedBitmap(body, new Int32Rect(regionSize, regionSize, regionSize, regionSize));
        }

        #endregion
    }
}

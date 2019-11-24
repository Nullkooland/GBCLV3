using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using GBCLV3.Models;

namespace GBCLV3.Services
{
    class SkinService
    {
        #region Private Members

        private const string _profileServer = "https://sessionserver.mojang.com/session/minecraft/profile/";

        private static readonly HttpClient _client = new HttpClient() { Timeout = TimeSpan.FromSeconds(15) };

        private static readonly JsonSerializerOptions _jsonOptions
            = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        #endregion

        public async Task<Skin> GetSkinAsync(string uuid)
        {
            try
            {
                string profileJson = await _client.GetStringAsync(_profileServer + uuid);
                using var profileDoc = JsonDocument.Parse(profileJson);

                string profile = profileDoc.RootElement
                                           .GetProperty("properties")[0]
                                           .GetProperty("value")
                                           .GetString();

                string skinJson = Encoding.UTF8.GetString(Convert.FromBase64String(profile));
                using var skinDoc = JsonDocument.Parse(skinJson);
                var textures = skinDoc.RootElement.GetProperty("textures");

                var skin = new Skin();

                if (textures.TryGetProperty("SKIN", out var body))
                {
                    string url = body.GetProperty("url").GetString();
                    skin.IsSlim = body.TryGetProperty("metadata", out _);

                    var httpStream = await _client.GetStreamAsync(url);
                    skin.Body = await DownloadImage(httpStream);
                }

                if (textures.TryGetProperty("CAPE", out var cape))
                {
                    string url = cape.GetProperty("url").GetString();
                    var httpStream = await _client.GetStreamAsync(url);
                    skin.Cape = await DownloadImage(httpStream);
                }

                return skin;
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine(ex.ToString());
                return null;
            }
            catch (OperationCanceledException)
            {
                Debug.WriteLine("[ERROR] Index json download time out");
                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                return null;
            }
        }

        public CroppedBitmap GetFace(BitmapImage bodySkin)
        {
            int regionSize = bodySkin.PixelWidth / 8;
            return new CroppedBitmap(bodySkin, new Int32Rect(regionSize, regionSize, regionSize, regionSize));
        }

        #region Private Methods

        private static async Task<BitmapImage> DownloadImage(Stream httpStream)
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

        #endregion
    }
}

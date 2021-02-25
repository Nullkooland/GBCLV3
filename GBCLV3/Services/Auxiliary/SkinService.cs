using GBCLV3.Models.Auxiliary;
using GBCLV3.Services.Launch;
using StyletIoC;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GBCLV3.Services.Auxiliary
{
    public class SkinService
    {
        #region Private Fields

        private const string MOJANG_PROFILE_SERVER = "https://sessionserver.mojang.com/session/minecraft/profile";

        private readonly GamePathService _gamePathService;
        private readonly HttpClient _client;

        private static readonly JsonSerializerOptions _jsonOptions
            = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        #endregion

        #region Constructor

        [Inject]
        public SkinService(GamePathService gamePathService, HttpClient client)
        {
            _gamePathService = gamePathService;
            _client = client;
        }

        #endregion

        public async ValueTask<string> GetProfileAsync(string uuid, string profileServer = null)
        {
            try
            {
                var profileJson = await _client.GetByteArrayAsync($"{profileServer ?? MOJANG_PROFILE_SERVER}/{uuid}");
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
                var skinJson = Convert.FromBase64String(profile);
                using var skinDoc = JsonDocument.Parse(skinJson);
                var textures = skinDoc.RootElement.GetProperty("textures");

                var skin = new Skin();

                if (textures.TryGetProperty("SKIN", out var body))
                {
                    skin.IsSlim = body.TryGetProperty("metadata", out _);
                    string url = body.GetProperty("url").GetString();
                    skin.Body = await LoadSkin(url);
                }

                if (textures.TryGetProperty("CAPE", out var cape))
                {
                    string url = body.GetProperty("url").GetString();
                    skin.Body = await LoadSkin(url);
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

        private async ValueTask<BitmapImage> LoadSkin(string url)
        {
            int pos = url.LastIndexOf('/') + 1;
            string hash = url[pos..];
            string path = $"{_gamePathService.AssetsDir}/skins/{hash[..2]}/{hash}";

            if (!File.Exists(path))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));

                await using var httpStream = await _client.GetStreamAsync(url);
                await using var fileStream = File.OpenWrite(path);
                await httpStream.CopyToAsync(fileStream);
                fileStream.Flush();
            }

            return LoadFromDisk(path);
        }

        private static BitmapImage LoadFromDisk(string path)
        {
            var img = new BitmapImage();
            img.BeginInit();
            img.UriSource = new Uri(path, UriKind.Absolute);
            img.CacheOption = BitmapCacheOption.OnLoad;
            img.EndInit();
            img.Freeze();

            return img;
        }

        private static BitmapSource GetFace(BitmapImage body)
        {
            if (body.PixelWidth % 8 != 0)
            {
                throw new InvalidOperationException("Invalid skin size!");
            }

            int size = body.PixelWidth / 8;
            int bytesPerPixel = PixelFormats.Bgra32.BitsPerPixel / 8;
            int stride = size * bytesPerPixel;

            int pixelCount = size * size * bytesPerPixel / 4;
            var bufferMain = new uint[pixelCount];
            var bufferOverlay = new uint[pixelCount];

            var faceMain = new CroppedBitmap(body, new Int32Rect(size, size, size, size));
            var faceOverlay = new CroppedBitmap(body, new Int32Rect(size * 5, size, size, size));

            faceMain.CopyPixels(bufferMain, stride, 0);
            faceOverlay.CopyPixels(bufferOverlay, stride, 0);

            // I can't believe I'm doing manual alpha blending
            for (int i = 0; i < pixelCount; i++)
            {
                uint pixel = bufferOverlay[i];

                if ((pixel & 0xFF000000) != 0x0)
                {
                    bufferMain[i] = pixel;
                }
            }

            var faceCombined = BitmapSource.Create(size, size, 96, 96,
                PixelFormats.Bgra32, null,
                bufferMain, size * bytesPerPixel);

            faceCombined.Freeze();
            return faceCombined;
        }

        #endregion
    }
}
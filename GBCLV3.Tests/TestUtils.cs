using System.Net.Http;

namespace GBCLV3.Tests
{
    static class TestUtils
    {
        private static readonly HttpClient _client = new HttpClient();

        public static bool IsDownloadable(DownloadItem item)
        {
            using var response = _client.GetAsync(item.Url).Result;
            return response.IsSuccessStatusCode;
        }
    }
}

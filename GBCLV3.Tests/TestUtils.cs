using System.Net;
using System.Net.Http;
using GBCLV3.Models;

namespace GBCLV3.Tests
{
    static class TestUtils
    {
        private static readonly HttpClient _client = new HttpClient();

        public static bool IsDownloadable(DownloadItem item)
        {
            using (var response = _client.GetAsync(item.Url).Result)
            {
                return (response.StatusCode == HttpStatusCode.OK);
            }
        }
    }
}

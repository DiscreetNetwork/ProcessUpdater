using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CLI.Services
{
    public class GithubReleaseService
    {
        private readonly HttpClient _httpClient;

        public GithubReleaseService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.DefaultRequestHeaders.Add("user-agent", "C#");
        }

        /// <summary>
        /// Get data for the latest release of a given repository
        /// </summary>
        /// <param name="repository">Organization/Repository</param>
        /// <returns></returns>
        public async Task<GithubReleaseResponse> GetRelease(string repository)
        {
            var request = await _httpClient.GetAsync($"https://api.github.com/repos/{repository}/releases/latest");
            var content = await request.Content.ReadAsStringAsync();
            if (!request.IsSuccessStatusCode) return null;

            GithubReleaseResponse release = JsonSerializer.Deserialize<GithubReleaseResponse>(content);
            return release;
        }

        public async Task<List<GithubAsset>> GetAssets(string sourceUrl)
        {
            var request = await _httpClient.GetAsync(sourceUrl);
            var content = await request.Content.ReadAsStringAsync();
            List<GithubAsset> assets = JsonSerializer.Deserialize<List<GithubAsset>>(content);
            return assets;
        }

        public async Task<byte[]> DownloadAsset(GithubAsset asset)
        {
            try
            {
                var bytes = await _httpClient.GetByteArrayAsync(asset.DownloadUrl);
                return bytes;
            }
            catch (Exception e)
            {
                return null;
            }
        }
    }

    public class GithubReleaseResponse
    {
        [System.Text.Json.Serialization.JsonPropertyName("assets_url")]
        public string AssetsUrl { get; set; }
    }

    public class GithubAsset
    {
        [System.Text.Json.Serialization.JsonPropertyName("browser_download_url")]
        public string DownloadUrl { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("name")]
        public string Name { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("content_type")]
        public string ContentType { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("state")]
        public string State { get; set; }
    }
}

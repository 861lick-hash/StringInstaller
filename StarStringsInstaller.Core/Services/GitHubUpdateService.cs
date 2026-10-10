using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace StarStringsInstaller.Services
{
    public class ReleaseInfo
    {
        public string TagName { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string ZipUrl { get; set; } = string.Empty;
        public string PublishedAt { get; set; } = string.Empty;
    }

    public class GitHubUpdateService
    {
        private const string RepositoryOwner = "MrKraken";
        private const string RepositoryName = "StarStrings";
        private readonly HttpClient _httpClient;

        public GitHubUpdateService(HttpClient? httpClient = null)
        {
            _httpClient = httpClient ?? new HttpClient();
            if (!_httpClient.DefaultRequestHeaders.Contains("User-Agent"))
            {
                _httpClient.DefaultRequestHeaders.Add("User-Agent", "StarStringsInstaller-App");
            }
        }

        public async Task<ReleaseInfo?> GetLatestReleaseAsync()
        {
            try
            {
                string apiUrl = $"https://api.github.com/repos/{RepositoryOwner}/{RepositoryName}/releases/latest";
                HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);

                if (!response.IsSuccessStatusCode)
                {
                    // Fallback to tags if no official release
                    return await GetLatestTagAsync();
                }

                string json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                string tagName = root.GetProperty("tag_name").GetString() ?? "";
                string name = root.TryGetProperty("name", out var nameProp) ? nameProp.GetString() ?? tagName : tagName;

                string zipUrl = "";
                if (root.TryGetProperty("zipball_url", out var zipProp))
                {
                    zipUrl = zipProp.GetString() ?? "";
                }

                // If assets exist, prefer zip asset
                if (root.TryGetProperty("assets", out var assetsProp) && assetsProp.ValueKind == JsonValueKind.Array)
                {
                    foreach (var asset in assetsProp.EnumerateArray())
                    {
                        string browserUrl = asset.GetProperty("browser_download_url").GetString() ?? "";
                        if (browserUrl.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                        {
                            zipUrl = browserUrl;
                            break;
                        }
                    }
                }

                if (string.IsNullOrEmpty(zipUrl))
                {
                    zipUrl = $"https://github.com/{RepositoryOwner}/{RepositoryName}/archive/refs/tags/{tagName}.zip";
                }

                return new ReleaseInfo
                {
                    TagName = tagName,
                    Name = name,
                    ZipUrl = zipUrl,
                    PublishedAt = root.TryGetProperty("published_at", out var pubProp) ? pubProp.GetString() ?? "" : ""
                };
            }
            catch
            {
                return await GetLatestTagAsync();
            }
        }

        private async Task<ReleaseInfo?> GetLatestTagAsync()
        {
            try
            {
                string apiUrl = $"https://api.github.com/repos/{RepositoryOwner}/{RepositoryName}/tags";
                HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);
                if (!response.IsSuccessStatusCode) return null;

                string json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.ValueKind == JsonValueKind.Array && doc.RootElement.GetArrayLength() > 0)
                {
                    var firstTag = doc.RootElement[0];
                    string tagName = firstTag.GetProperty("name").GetString() ?? "";
                    string zipUrl = firstTag.GetProperty("zipball_url").GetString() ?? $"https://github.com/{RepositoryOwner}/{RepositoryName}/archive/refs/tags/{tagName}.zip";

                    return new ReleaseInfo
                    {
                        TagName = tagName,
                        Name = tagName,
                        ZipUrl = zipUrl
                    };
                }
            }
            catch
            {
                // Return null if failed
            }

            return null;
        }

        public async Task DownloadAndExtractZipAsync(string zipUrl, string targetEnvironmentDirectory)
        {
            if (string.IsNullOrWhiteSpace(zipUrl))
                throw new ArgumentException("Zip URL cannot be empty", nameof(zipUrl));

            if (!Directory.Exists(targetEnvironmentDirectory))
            {
                Directory.CreateDirectory(targetEnvironmentDirectory);
            }

            string tempZipPath = Path.Combine(Path.GetTempPath(), $"starstrings_{Guid.NewGuid():N}.zip");
            string tempExtractPath = Path.Combine(Path.GetTempPath(), $"starstrings_extract_{Guid.NewGuid():N}");

            try
            {
                byte[] zipBytes = await _httpClient.GetByteArrayAsync(zipUrl);
                await File.WriteAllBytesAsync(tempZipPath, zipBytes);

                if (Directory.Exists(tempExtractPath))
                    Directory.Delete(tempExtractPath, true);

                ZipFile.ExtractToDirectory(tempZipPath, tempExtractPath);

                // Look for source files (data folder or src folder or root contents)
                string sourceDataDir = FindDataDirectory(tempExtractPath);

                if (!string.IsNullOrEmpty(sourceDataDir) && Directory.Exists(sourceDataDir))
                {
                    string destDataDir = Path.Combine(targetEnvironmentDirectory, "data");
                    CopyDirectoryContent(sourceDataDir, destDataDir);
                }
                else
                {
                    throw new InvalidOperationException("Could not locate 'data' directory in the downloaded release archive.");
                }
            }
            finally
            {
                if (File.Exists(tempZipPath))
                {
                    try { File.Delete(tempZipPath); } catch { }
                }
                if (Directory.Exists(tempExtractPath))
                {
                    try { Directory.Delete(tempExtractPath, true); } catch { }
                }
            }
        }

        public string FindDataDirectory(string rootExtractFolder)
        {
            // First check if root/data exists
            string directData = Path.Combine(rootExtractFolder, "data");
            if (Directory.Exists(directData)) return directData;

            // GitHub zipballs usually extract with a top-level directory like MrKraken-StarStrings-xxxx/
            string[] subDirs = Directory.GetDirectories(rootExtractFolder);
            foreach (var subDir in subDirs)
            {
                string candidate = Path.Combine(subDir, "data");
                if (Directory.Exists(candidate)) return candidate;

                string srcCandidate = Path.Combine(subDir, "src", "data");
                if (Directory.Exists(srcCandidate)) return srcCandidate;
            }

            // Search recursively for Localization/english or data/
            var allDataDirs = Directory.GetDirectories(rootExtractFolder, "data", SearchOption.AllDirectories);
            if (allDataDirs.Length > 0) return allDataDirs[0];

            return string.Empty;
        }

        public void CopyDirectoryContent(string sourceDir, string targetDir)
        {
            Directory.CreateDirectory(targetDir);

            foreach (string file in Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories))
            {
                string relativePath = Path.GetRelativePath(sourceDir, file);
                string destinationFile = Path.Combine(targetDir, relativePath);

                string? destDir = Path.GetDirectoryName(destinationFile);
                if (!string.IsNullOrEmpty(destDir) && !Directory.Exists(destDir))
                {
                    Directory.CreateDirectory(destDir);
                }

                File.Copy(file, destinationFile, overwrite: true);
            }
        }
    }
}

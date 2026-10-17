using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using StarStringsInstaller.Services;
using Xunit;

namespace StarStringsInstaller.Tests
{
    public class UserCfgManagerTests
    {
        [Fact]
        public void EnsureLanguageConfigured_CreatesFileIfMissing()
        {
            string tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            string userCfgPath = Path.Combine(tempDir, "user.cfg");

            try
            {
                var manager = new UserCfgManager();
                bool result = manager.EnsureLanguageConfigured(userCfgPath);

                Assert.True(result);
                Assert.True(File.Exists(userCfgPath));
                string content = File.ReadAllText(userCfgPath);
                Assert.Contains("g_language = english", content);
            }
            finally
            {
                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, true);
            }
        }

        [Fact]
        public void EnsureLanguageConfigured_AppendsIfNotPresent()
        {
            string tempFile = Path.GetTempFileName();
            try
            {
                File.WriteAllText(tempFile, "r_displayinfo = 1" + Environment.NewLine);

                var manager = new UserCfgManager();
                bool result = manager.EnsureLanguageConfigured(tempFile);

                Assert.True(result);
                string content = File.ReadAllText(tempFile);
                Assert.Contains("r_displayinfo = 1", content);
                Assert.Contains("g_language = english", content);
            }
            finally
            {
                if (File.Exists(tempFile)) File.Delete(tempFile);
            }
        }

        [Fact]
        public void EnsureLanguageConfigured_DoesNotDuplicateIfPresent()
        {
            string tempFile = Path.GetTempFileName();
            try
            {
                File.WriteAllText(tempFile, "g_language = english" + Environment.NewLine);

                var manager = new UserCfgManager();
                bool result = manager.EnsureLanguageConfigured(tempFile);

                Assert.False(result);
                string[] lines = File.ReadAllLines(tempFile);
                int count = 0;
                foreach (var line in lines)
                {
                    if (line.Trim().Replace(" ", "").ToLowerInvariant() == "g_language=english")
                        count++;
                }
                Assert.Equal(1, count);
            }
            finally
            {
                if (File.Exists(tempFile)) File.Delete(tempFile);
            }
        }
    }

    public class SettingsManagerTests
    {
        [Fact]
        public void LoadAndSave_WorksCorrectly()
        {
            string tempFile = Path.GetTempFileName();
            try
            {
                var manager = new SettingsManager(tempFile);

                var settings = new SettingsManager.AppSettings
                {
                    StarCitizenPath = @"C:\Games\StarCitizen",
                    TargetEnvironmentPath = @"C:\Games\StarCitizen\LIVE",
                    InstalledVersion = "v1.2.3"
                };

                manager.Save(settings);

                var loaded = manager.Load();
                Assert.Equal(@"C:\Games\StarCitizen", loaded.StarCitizenPath);
                Assert.Equal(@"C:\Games\StarCitizen\LIVE", loaded.TargetEnvironmentPath);
                Assert.Equal("v1.2.3", loaded.InstalledVersion);
            }
            finally
            {
                if (File.Exists(tempFile)) File.Delete(tempFile);
            }
        }
    }

    public class StarCitizenDetectorTests
    {
        [Fact]
        public void IsValidStarCitizenDir_ReturnsTrueWhenLIVEExists()
        {
            string tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            Directory.CreateDirectory(Path.Combine(tempDir, "LIVE"));

            try
            {
                var detector = new StarCitizenDetector();
                bool isValid = detector.IsValidStarCitizenDir(tempDir);
                Assert.True(isValid);
            }
            finally
            {
                if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
            }
        }

        [Fact]
        public void FindEnvironments_ReturnsEnvironments()
        {
            string tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            Directory.CreateDirectory(Path.Combine(tempDir, "LIVE"));
            Directory.CreateDirectory(Path.Combine(tempDir, "PTU"));

            try
            {
                var detector = new StarCitizenDetector();
                var envs = detector.FindEnvironments(tempDir);

                Assert.Equal(2, envs.Count);
                Assert.Contains(Path.Combine(tempDir, "LIVE"), envs);
                Assert.Contains(Path.Combine(tempDir, "PTU"), envs);
            }
            finally
            {
                if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
            }
        }
    }

    public class MockHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _handler;

        public MockHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> handler)
        {
            _handler = handler;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(_handler(request));
        }
    }

    public class GitHubUpdateServiceTests
    {
        [Fact]
        public async Task GetLatestReleaseAsync_ParsesJsonResponseCorrectly()
        {
            string jsonResponse = @"{
                ""tag_name"": ""latest"",
                ""name"": ""SC LIVE Build (release-2026-10-09-292e6be)"",
                ""published_at"": ""2026-10-09T14:31:59Z"",
                ""zipball_url"": ""https://api.github.com/repos/MrKraken/StarStrings/zipball/latest""
            }";

            var handler = new MockHttpMessageHandler(req => new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonResponse)
            });

            using var httpClient = new HttpClient(handler);
            var service = new GitHubUpdateService(httpClient);

            var release = await service.GetLatestReleaseAsync("LIVE");

            Assert.NotNull(release);
            Assert.Equal("latest", release!.TagName);
            Assert.Equal("SC LIVE Build (release-2026-10-09-292e6be)", release.Name);
            Assert.Equal("https://api.github.com/repos/MrKraken/StarStrings/zipball/latest", release.ZipUrl);

            var (build, releaseStr) = release.GetFormattedBuildAndRelease();
            Assert.Equal("SC LIVE Build", build);
            Assert.Equal("release-2026-10-09-292e6be", releaseStr);
            Assert.Equal($"Build: SC LIVE Build{Environment.NewLine}Release: release-2026-10-09-292e6be", release.GetFormattedDisplayString());
        }

        [Fact]
        public async Task GetLatestReleaseAsync_RequestsPtuTagWhenEnvironmentIsPtu()
        {
            string requestedUri = "";
            string jsonResponse = @"{
                ""tag_name"": ""latest-ptu"",
                ""name"": ""SC PTU Build (release-2026-10-06-d59ccf1)"",
                ""zipball_url"": ""https://api.github.com/repos/MrKraken/StarStrings/zipball/latest-ptu""
            }";

            var handler = new MockHttpMessageHandler(req =>
            {
                requestedUri = req.RequestUri?.ToString() ?? "";
                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(jsonResponse)
                };
            });

            using var httpClient = new HttpClient(handler);
            var service = new GitHubUpdateService(httpClient);

            var release = await service.GetLatestReleaseAsync("PTU");

            Assert.Contains("releases/tags/latest-ptu", requestedUri);
            Assert.NotNull(release);
            Assert.Equal("SC PTU Build (release-2026-10-06-d59ccf1)", release!.Name);
        }

        [Fact]
        public void EnvironmentItem_FormatsFolderNameAndFullPathCorrectly()
        {
            string fullPath = @"C:\Program Files\Roberts Space Industries\StarCitizen\LIVE";
            var envItem = new EnvironmentItem(fullPath);

            Assert.Equal("LIVE", envItem.FolderName);
            Assert.Equal("LIVE", envItem.ToString());
            Assert.Equal(fullPath, envItem.FullPath);
        }

        [Fact]
        public void FindDataDirectory_LocatesDataInZipStructure()
        {
            string tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            string subDir = Path.Combine(tempDir, "MrKraken-StarStrings-abc123");
            string dataDir = Path.Combine(subDir, "data");
            Directory.CreateDirectory(dataDir);

            try
            {
                var service = new GitHubUpdateService();
                string result = service.FindDataDirectory(tempDir);

                Assert.Equal(dataDir, result);
            }
            finally
            {
                if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
            }
        }

        [Fact]
        public void ExtractBuildVersionFromReadme_ParsesBuildTag()
        {
            string readmeContent = @"# StarStrings
[Build: sc-alpha-4.10.2_live_12881860]

Welcome to StarStrings repository.";

            var service = new GitHubUpdateService();
            string? buildVersion = service.ExtractBuildVersionFromReadme(readmeContent);

            Assert.Equal("sc-alpha-4.10.2_live_12881860", buildVersion);
        }

        [Fact]
        public void NormalizeVersion_NormalizesUnderscoresAndPrefixes()
        {
            string raw = "sc-alpha-4.10.2_live_12881860";
            string normalized = GitHubUpdateService.NormalizeVersion(raw);

            Assert.Equal("4.10.2-live.12881860", normalized);
        }
    }

    public class GameVersionServiceTests
    {
        [Fact]
        public void GetGameVersion_ParsesBuildManifestCorrectly()
        {
            string tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), "LIVE");
            Directory.CreateDirectory(tempDir);
            string manifestPath = Path.Combine(tempDir, "build_manifest.id");

            string manifestJson = @"{
                ""Data"": {
                    ""Branch"": ""sc-alpha-4.10.2"",
                    ""BuildDateStamp"": ""Mon Oct 12 2026"",
                    ""RequestedP4ChangeNum"": ""12913470""
                }
            }";

            File.WriteAllText(manifestPath, manifestJson);

            try
            {
                var service = new GameVersionService();
                string? version = service.GetGameVersion(tempDir);

                Assert.NotNull(version);
                Assert.Equal("4.10.2-live.12913470", version);
            }
            finally
            {
                string? parent = Directory.GetParent(tempDir)?.FullName;
                if (!string.IsNullOrEmpty(parent) && Directory.Exists(parent))
                {
                    Directory.Delete(parent, true);
                }
            }
        }

        [Fact]
        public void GetGameVersion_ReturnsNullWhenFileMissing()
        {
            string tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);

            try
            {
                var service = new GameVersionService();
                string? version = service.GetGameVersion(tempDir);

                Assert.Null(version);
            }
            finally
            {
                if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
            }
        }

        [Fact]
        public void FormatGameVersion_FormatsBranchAndEnvCorrectly()
        {
            var service = new GameVersionService();
            string formatted = service.FormatGameVersion("sc-alpha-4.10.2", "12913470", "ptu");

            Assert.Equal("4.10.2-ptu.12913470", formatted);
        }
    }
}

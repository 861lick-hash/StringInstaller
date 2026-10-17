using System;
using System.IO;
using System.Text.Json;

namespace StarStringsInstaller.Services
{
    public class BuildManifestData
    {
        public string Branch { get; set; } = string.Empty;
        public string RequestedP4ChangeNum { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
    }

    public class BuildManifestWrapper
    {
        public BuildManifestData? Data { get; set; }
    }

    public class GameVersionService
    {
        public virtual string? GetGameVersion(string environmentPath)
        {
            if (string.IsNullOrWhiteSpace(environmentPath) || !Directory.Exists(environmentPath))
            {
                return null;
            }

            string manifestPath = Path.Combine(environmentPath, "build_manifest.id");
            if (!File.Exists(manifestPath))
            {
                return null;
            }

            try
            {
                string json = File.ReadAllText(manifestPath);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                var wrapper = JsonSerializer.Deserialize<BuildManifestWrapper>(json, options);
                var data = wrapper?.Data;

                if (data == null)
                {
                    return null;
                }

                string envName = Path.GetFileName(environmentPath.TrimEnd('/', '\\')).ToLowerInvariant();
                return FormatGameVersion(data.Branch, data.RequestedP4ChangeNum, envName);
            }
            catch
            {
                return null;
            }
        }

        public string FormatGameVersion(string branch, string changeNum, string envName)
        {
            if (string.IsNullOrWhiteSpace(branch))
            {
                return string.Empty;
            }

            string cleanBranch = branch.Trim();
            if (cleanBranch.StartsWith("sc-", StringComparison.OrdinalIgnoreCase))
            {
                cleanBranch = cleanBranch.Substring(3);
            }

            if (cleanBranch.Length > 0 && char.IsLower(cleanBranch[0]))
            {
                cleanBranch = char.ToUpper(cleanBranch[0]) + cleanBranch.Substring(1);
            }
            else if (cleanBranch.Length > 0 && char.IsDigit(cleanBranch[0]))
            {
                string stage = string.IsNullOrWhiteSpace(envName) ? "Alpha" : (char.ToUpper(envName[0]) + envName.Substring(1).ToLowerInvariant());
                cleanBranch = $"{stage}-{cleanBranch}";
            }

            if (!string.IsNullOrWhiteSpace(changeNum))
            {
                return $"{cleanBranch}.{changeNum}";
            }

            return cleanBranch;
        }
    }
}

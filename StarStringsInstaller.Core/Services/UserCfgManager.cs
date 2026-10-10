using System;
using System.IO;
using System.Text;

namespace StarStringsInstaller.Services
{
    public class UserCfgManager
    {
        public const string TargetConfigLine = "g_language = english";

        public bool EnsureLanguageConfigured(string userCfgPath)
        {
            if (string.IsNullOrWhiteSpace(userCfgPath))
                throw new ArgumentException("Path cannot be empty", nameof(userCfgPath));

            if (!File.Exists(userCfgPath))
            {
                var dir = Path.GetDirectoryName(userCfgPath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                File.WriteAllText(userCfgPath, TargetConfigLine + Environment.NewLine, Encoding.UTF8);
                return true;
            }

            var lines = File.ReadAllLines(userCfgPath);
            foreach (var line in lines)
            {
                var trimmed = line.Trim().Replace(" ", "").ToLowerInvariant();
                if (trimmed == "g_language=english")
                {
                    // Already configured
                    return false;
                }
            }

            // Append config line
            using (var writer = new StreamWriter(userCfgPath, append: true, encoding: Encoding.UTF8))
            {
                if (lines.Length > 0 && !string.IsNullOrWhiteSpace(lines[lines.Length - 1]))
                {
                    writer.WriteLine();
                }
                writer.WriteLine(TargetConfigLine);
            }

            return true;
        }
    }
}

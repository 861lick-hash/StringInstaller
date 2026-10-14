using System;
using System.Collections.Generic;
using System.IO;

namespace StarStringsInstaller.Services
{
    public class EnvironmentItem
    {
        public string FolderName { get; }
        public string FullPath { get; }

        public EnvironmentItem(string fullPath)
        {
            FullPath = fullPath;
            string normalized = fullPath.TrimEnd('/', '\\').Replace('\\', '/');
            FolderName = System.IO.Path.GetFileName(normalized);
            if (string.IsNullOrEmpty(FolderName))
            {
                FolderName = fullPath;
            }
        }

        public override string ToString() => FolderName;
    }

    public class StarCitizenDetector
    {
        private static readonly string[] CommonPaths = new[]
        {
            @"C:\Program Files\Roberts Space Industries\StarCitizen",
            @"C:\Program Files (x86)\Roberts Space Industries\StarCitizen",
            @"D:\Program Files\Roberts Space Industries\StarCitizen",
            @"D:\StarCitizen",
            @"E:\StarCitizen",
            @"F:\StarCitizen",
            @"G:\StarCitizen",
            @"D:\Games\StarCitizen",
            @"E:\Games\StarCitizen",
            @"F:\Games\StarCitizen",
            @"G:\Games\StarCitizen",
            @"C:\Games\StarCitizen"
        };

        public virtual string? DetectStarCitizenPath()
        {
            // Check drives/common paths first
            foreach (var path in CommonPaths)
            {
                if (Directory.Exists(path) && IsValidStarCitizenDir(path))
                {
                    return path;
                }
            }

            // Check drive roots for Roberts Space Industries or StarCitizen
            try
            {
                var drives = DriveInfo.GetDrives();
                foreach (var drive in drives)
                {
                    if (!drive.IsReady) continue;

                    string candidate1 = Path.Combine(drive.Name, "Roberts Space Industries", "StarCitizen");
                    if (Directory.Exists(candidate1) && IsValidStarCitizenDir(candidate1))
                    {
                        return candidate1;
                    }

                    string candidate2 = Path.Combine(drive.Name, "Games", "Roberts Space Industries", "StarCitizen");
                    if (Directory.Exists(candidate2) && IsValidStarCitizenDir(candidate2))
                    {
                        return candidate2;
                    }
                }
            }
            catch
            {
                // Ignore drive access exceptions
            }

            return null;
        }

        public virtual List<string> FindEnvironments(string starCitizenPath)
        {
            var result = new List<string>();
            if (string.IsNullOrWhiteSpace(starCitizenPath) || !Directory.Exists(starCitizenPath))
            {
                return result;
            }

            // Check LIVE, PTU, EPTU, TECH-PREVIEW
            string[] possibleEnvs = new[] { "LIVE", "PTU", "EPTU", "TECH-PREVIEW" };
            foreach (var env in possibleEnvs)
            {
                string envDir = Path.Combine(starCitizenPath, env);
                if (Directory.Exists(envDir))
                {
                    result.Add(envDir);
                }
            }

            // If selected folder IS the environment folder (e.g. C:\...\StarCitizen\LIVE)
            if (result.Count == 0 && Path.GetFileName(starCitizenPath).Equals("LIVE", StringComparison.OrdinalIgnoreCase) ||
                Path.GetFileName(starCitizenPath).Equals("PTU", StringComparison.OrdinalIgnoreCase))
            {
                result.Add(starCitizenPath);
            }

            return result;
        }

        public virtual bool IsValidStarCitizenDir(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
                return false;

            // Check if directory contains LIVE, PTU or is itself a LIVE folder (or has Data.p4k)
            if (Directory.Exists(Path.Combine(path, "LIVE")) ||
                Directory.Exists(Path.Combine(path, "PTU")) ||
                File.Exists(Path.Combine(path, "Data.p4k")) ||
                File.Exists(Path.Combine(path, "user.cfg")))
            {
                return true;
            }

            return false;
        }
    }
}

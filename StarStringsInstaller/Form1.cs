using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using StarStringsInstaller.Services;

namespace StarStringsInstaller
{
    public partial class Form1 : Form
    {
        private readonly StarCitizenDetector _detector;
        private readonly GitHubUpdateService _updateService;
        private readonly UserCfgManager _cfgManager;
        private readonly SettingsManager _settingsManager;
        private readonly GameVersionService _gameVersionService;

        private ReleaseInfo? _latestRelease;

        public Form1()
        {
            InitializeComponent();
            _detector = new StarCitizenDetector();
            _updateService = new GitHubUpdateService();
            _cfgManager = new UserCfgManager();
            _settingsManager = new SettingsManager();
            _gameVersionService = new GameVersionService();
        }

        public Form1(
            StarCitizenDetector detector,
            GitHubUpdateService updateService,
            UserCfgManager cfgManager,
            SettingsManager settingsManager,
            GameVersionService? gameVersionService = null)
        {
            InitializeComponent();
            _detector = detector;
            _updateService = updateService;
            _cfgManager = cfgManager;
            _settingsManager = settingsManager;
            _gameVersionService = gameVersionService ?? new GameVersionService();
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            Log("Application started.");

            var settings = _settingsManager.Load();
            lblInstalledVersion.Text = string.IsNullOrEmpty(settings.InstalledVersion) ? "None" : settings.InstalledVersion;

            string scPath = settings.StarCitizenPath;
            if (string.IsNullOrWhiteSpace(scPath) || !_detector.IsValidStarCitizenDir(scPath))
            {
                Log("Auto-detecting Star Citizen installation directory...");
                scPath = _detector.DetectStarCitizenPath() ?? string.Empty;
            }

            if (!string.IsNullOrEmpty(scPath))
            {
                txtStarCitizenPath.Text = scPath;
                Log($"Star Citizen directory set to: {scPath}");
                PopulateEnvironments(scPath, settings.TargetEnvironmentPath);
            }
            else
            {
                Log("Star Citizen directory could not be auto-detected. Please browse to your Star Citizen folder.");
            }

            await CheckForUpdatesAsync();
        }

        private void Log(string message)
        {
            string timeStamp = DateTime.Now.ToString("HH:mm:ss");
            txtLog.AppendText($"[{timeStamp}] {message}{Environment.NewLine}");
            lblStatus.Text = message;
        }

        private void PopulateEnvironments(string path, string? preferredEnv = null)
        {
            cmbEnvironment.Items.Clear();
            var envs = _detector.FindEnvironments(path);

            if (envs.Count == 0)
            {
                if (_detector.IsValidStarCitizenDir(path))
                {
                    cmbEnvironment.Items.Add(new EnvironmentItem(path));
                }
            }
            else
            {
                foreach (var env in envs)
                {
                    cmbEnvironment.Items.Add(new EnvironmentItem(env));
                }
            }

            if (cmbEnvironment.Items.Count > 0)
            {
                int selectedIndex = 0;
                if (!string.IsNullOrEmpty(preferredEnv))
                {
                    for (int i = 0; i < cmbEnvironment.Items.Count; i++)
                    {
                        if (cmbEnvironment.Items[i] is EnvironmentItem item)
                        {
                            if (item.FullPath.Equals(preferredEnv, StringComparison.OrdinalIgnoreCase) ||
                                item.FolderName.Equals(preferredEnv, StringComparison.OrdinalIgnoreCase))
                            {
                                selectedIndex = i;
                                break;
                            }
                        }
                    }
                }
                cmbEnvironment.SelectedIndex = selectedIndex;
            }
        }

        private async void btnCheckUpdates_Click(object sender, EventArgs e)
        {
            await CheckForUpdatesAsync();
        }

        public async Task CheckForUpdatesAsync()
        {
            btnCheckUpdates.Enabled = false;
            lblLatestVersion.Text = "Checking...";
            lblGameVersion.Text = "Checking...";
            Log("Checking GitHub for the latest StarStrings release and game version...");

            string targetEnvPath = GetSelectedEnvironmentPath();
            string? envName = (cmbEnvironment.SelectedItem as EnvironmentItem)?.FolderName;

            try
            {
                // Detect local game version
                string? localGameVersion = _gameVersionService.GetGameVersion(targetEnvPath);
                if (!string.IsNullOrEmpty(localGameVersion))
                {
                    lblGameVersion.Text = localGameVersion;
                    Log($"Local Game Version: {localGameVersion}");
                }
                else
                {
                    lblGameVersion.Text = "Unknown";
                    Log("Could not detect local game version from build_manifest.id.");
                }

                // Check GitHub README version & release
                string? gitHubReadmeVersion = await _updateService.GetGitHubReadmeGameVersionAsync();
                _latestRelease = await _updateService.GetLatestReleaseAsync(envName);

                string latestVersionDisplay = "Unknown";
                if (!string.IsNullOrEmpty(gitHubReadmeVersion))
                {
                    latestVersionDisplay = GitHubUpdateService.NormalizeVersion(gitHubReadmeVersion);
                }
                else if (_latestRelease != null && !string.IsNullOrEmpty(_latestRelease.Name))
                {
                    latestVersionDisplay = GitHubUpdateService.NormalizeVersion(_latestRelease.Name);
                }

                lblLatestVersion.Text = latestVersionDisplay;

                if (_latestRelease != null && (!string.IsNullOrEmpty(_latestRelease.Name) || !string.IsNullOrEmpty(_latestRelease.TagName)))
                {
                    Log($"Latest release found: {_latestRelease.Name} ({latestVersionDisplay})");
                }
                else
                {
                    Log("Failed to retrieve release information from GitHub.");
                }

                // Compare installed version, latest version, and game version
                CheckAndHighlightVersionMismatch(lblInstalledVersion.Text, latestVersionDisplay, localGameVersion);
            }
            catch (Exception ex)
            {
                lblLatestVersion.Text = "Error";
                lblGameVersion.Text = "Error";
                Log($"Error checking updates: {ex.Message}");
            }
            finally
            {
                btnCheckUpdates.Enabled = true;
            }
        }

        private void CheckAndHighlightVersionMismatch(string installedVersion, string latestVersion, string? localGameVersion)
        {
            string normInstalled = GitHubUpdateService.NormalizeVersion(installedVersion);
            string normLatest = GitHubUpdateService.NormalizeVersion(latestVersion);
            string normGame = string.IsNullOrEmpty(localGameVersion) ? string.Empty : GitHubUpdateService.NormalizeVersion(localGameVersion);

            // Compare installed vs latest
            if (!string.IsNullOrEmpty(normInstalled) && !normInstalled.Equals("none", StringComparison.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrEmpty(normLatest) && !normLatest.Equals("unknown", StringComparison.OrdinalIgnoreCase) && !normLatest.Equals("error", StringComparison.OrdinalIgnoreCase))
                {
                    if (normInstalled.Equals(normLatest, StringComparison.OrdinalIgnoreCase))
                    {
                        lblInstalledVersion.ForeColor = System.Drawing.Color.Black;
                        Log("You already have the latest version installed.");
                    }
                    else
                    {
                        lblInstalledVersion.ForeColor = System.Drawing.Color.Red;
                        Log("New update available! Click 'Install / Update' to install.");
                    }
                }
                else
                {
                    lblInstalledVersion.ForeColor = System.Drawing.Color.Black;
                }
            }
            else
            {
                lblInstalledVersion.ForeColor = System.Drawing.Color.Black;
                if (!string.IsNullOrEmpty(normLatest) && !normLatest.Equals("unknown", StringComparison.OrdinalIgnoreCase))
                {
                    Log("StarStrings is not installed yet. Click 'Install / Update' to install.");
                }
            }

            // Compare game vs latest
            if (string.IsNullOrEmpty(normGame) || string.IsNullOrEmpty(normLatest) || normLatest.Equals("unknown", StringComparison.OrdinalIgnoreCase) || normLatest.Equals("error", StringComparison.OrdinalIgnoreCase))
            {
                lblGameVersion.ForeColor = System.Drawing.Color.Red;
            }
            else if (!normGame.Equals(normLatest, StringComparison.OrdinalIgnoreCase))
            {
                lblGameVersion.ForeColor = System.Drawing.Color.Red;
                Log($"Game version ({normGame}) does not match StarStrings GitHub target build ({normLatest}).");
            }
            else
            {
                lblGameVersion.ForeColor = System.Drawing.Color.Black;
                Log("Game version matches StarStrings GitHub target build.");
            }

            // If all three match
            if (!string.IsNullOrEmpty(normInstalled) &&
                !normInstalled.Equals("none", StringComparison.OrdinalIgnoreCase) &&
                !string.IsNullOrEmpty(normLatest) &&
                !string.IsNullOrEmpty(normGame) &&
                normInstalled.Equals(normLatest, StringComparison.OrdinalIgnoreCase) &&
                normLatest.Equals(normGame, StringComparison.OrdinalIgnoreCase))
            {
                Log("Installed version, latest version, and local game version all match.");
            }
        }

        private string GetSelectedEnvironmentPath()
        {
            if (cmbEnvironment.SelectedItem is EnvironmentItem item)
            {
                return item.FullPath;
            }
            return txtStarCitizenPath.Text;
        }

        private async void btnInstall_Click(object sender, EventArgs e)
        {
            string targetEnvPath = GetSelectedEnvironmentPath();

            if (string.IsNullOrWhiteSpace(targetEnvPath) || !Directory.Exists(targetEnvPath))
            {
                MessageBox.Show("Please select or enter a valid Star Citizen directory before installing.", "Invalid Path", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_latestRelease == null || string.IsNullOrEmpty(_latestRelease.ZipUrl))
            {
                Log("No valid release found. Refreshing update status...");
                await CheckForUpdatesAsync();
                if (_latestRelease == null || string.IsNullOrEmpty(_latestRelease.ZipUrl))
                {
                    MessageBox.Show("Could not find release archive to download.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            btnInstall.Enabled = false;
            btnCheckUpdates.Enabled = false;

            try
            {
                Log($"Starting installation of {_latestRelease.Name} into '{targetEnvPath}'...");

                // Step 1: Download & extract files
                await _updateService.DownloadAndExtractZipAsync(_latestRelease.ZipUrl, targetEnvPath);
                Log("Files downloaded and extracted successfully.");

                // Step 2: Ensure user.cfg exists and g_language = english is set
                string userCfgPath = Path.Combine(targetEnvPath, "user.cfg");
                bool modified = _cfgManager.EnsureLanguageConfigured(userCfgPath);
                if (modified)
                {
                    Log("Configured 'g_language = english' in user.cfg.");
                }
                else
                {
                    Log("user.cfg already configured with 'g_language = english'.");
                }

                // Step 3: Save settings
                string installedVersionDisplay = lblLatestVersion.Text;
                lblInstalledVersion.Text = installedVersionDisplay;

                var settings = new SettingsManager.AppSettings
                {
                    StarCitizenPath = txtStarCitizenPath.Text,
                    TargetEnvironmentPath = targetEnvPath,
                    InstalledVersion = installedVersionDisplay
                };
                _settingsManager.Save(settings);

                Log($"Installation of StarStrings {_latestRelease.Name} complete!");
                MessageBox.Show($"StarStrings {_latestRelease.Name} installed successfully!", "Installation Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                Log($"Installation error: {ex.Message}");
                MessageBox.Show($"Installation failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnInstall.Enabled = true;
                btnCheckUpdates.Enabled = true;
            }
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            using var dialog = new FolderBrowserDialog();
            dialog.Description = "Select Star Citizen Directory (e.g., StarCitizen or StarCitizen/LIVE)";
            if (Directory.Exists(txtStarCitizenPath.Text))
            {
                dialog.SelectedPath = txtStarCitizenPath.Text;
            }

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                txtStarCitizenPath.Text = dialog.SelectedPath;
                PopulateEnvironments(dialog.SelectedPath);
            }
        }

        private void txtStarCitizenPath_TextChanged(object sender, EventArgs e)
        {
            if (Directory.Exists(txtStarCitizenPath.Text))
            {
                PopulateEnvironments(txtStarCitizenPath.Text);
            }
        }

        private async void cmbEnvironment_SelectedIndexChanged(object sender, EventArgs e)
        {
            await CheckForUpdatesAsync();
        }
    }
}

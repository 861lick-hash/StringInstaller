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

        private ReleaseInfo? _latestRelease;

        public Form1()
        {
            InitializeComponent();
            _detector = new StarCitizenDetector();
            _updateService = new GitHubUpdateService();
            _cfgManager = new UserCfgManager();
            _settingsManager = new SettingsManager();
        }

        public Form1(
            StarCitizenDetector detector,
            GitHubUpdateService updateService,
            UserCfgManager cfgManager,
            SettingsManager settingsManager)
        {
            InitializeComponent();
            _detector = detector;
            _updateService = updateService;
            _cfgManager = cfgManager;
            _settingsManager = settingsManager;
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
                    cmbEnvironment.Items.Add(path);
                }
            }
            else
            {
                foreach (var env in envs)
                {
                    cmbEnvironment.Items.Add(env);
                }
            }

            if (cmbEnvironment.Items.Count > 0)
            {
                int selectedIndex = 0;
                if (!string.IsNullOrEmpty(preferredEnv))
                {
                    for (int i = 0; i < cmbEnvironment.Items.Count; i++)
                    {
                        if (cmbEnvironment.Items[i]?.ToString()?.Equals(preferredEnv, StringComparison.OrdinalIgnoreCase) == true)
                        {
                            selectedIndex = i;
                            break;
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
            Log("Checking GitHub for the latest StarStrings release...");

            try
            {
                _latestRelease = await _updateService.GetLatestReleaseAsync();
                if (_latestRelease != null && !string.IsNullOrEmpty(_latestRelease.TagName))
                {
                    lblLatestVersion.Text = _latestRelease.TagName;
                    Log($"Latest release found: {_latestRelease.TagName}");

                    if (_latestRelease.TagName.Equals(lblInstalledVersion.Text, StringComparison.OrdinalIgnoreCase))
                    {
                        Log("You already have the latest version installed.");
                    }
                    else
                    {
                        Log("New update available! Click 'Install / Update' to install.");
                    }
                }
                else
                {
                    lblLatestVersion.Text = "Unknown";
                    Log("Failed to retrieve release information from GitHub.");
                }
            }
            catch (Exception ex)
            {
                lblLatestVersion.Text = "Error";
                Log($"Error checking updates: {ex.Message}");
            }
            finally
            {
                btnCheckUpdates.Enabled = true;
            }
        }

        private async void btnInstall_Click(object sender, EventArgs e)
        {
            string targetEnvPath = cmbEnvironment.SelectedItem?.ToString() ?? txtStarCitizenPath.Text;

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
                Log($"Starting installation of {_latestRelease.TagName} into '{targetEnvPath}'...");

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
                string currentTag = _latestRelease.TagName;
                lblInstalledVersion.Text = currentTag;

                var settings = new SettingsManager.AppSettings
                {
                    StarCitizenPath = txtStarCitizenPath.Text,
                    TargetEnvironmentPath = targetEnvPath,
                    InstalledVersion = currentTag
                };
                _settingsManager.Save(settings);

                Log($"Installation of StarStrings {currentTag} complete!");
                MessageBox.Show($"StarStrings {currentTag} installed successfully!", "Installation Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
    }
}

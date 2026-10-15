namespace StarStringsInstaller
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblPath = new System.Windows.Forms.Label();
            this.txtStarCitizenPath = new System.Windows.Forms.TextBox();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.lblEnvironment = new System.Windows.Forms.Label();
            this.cmbEnvironment = new System.Windows.Forms.ComboBox();
            this.lblInstalledVersionHeader = new System.Windows.Forms.Label();
            this.lblInstalledVersion = new System.Windows.Forms.Label();
            this.lblLatestVersionHeader = new System.Windows.Forms.Label();
            this.lblLatestVersion = new System.Windows.Forms.Label();
            this.btnCheckUpdates = new System.Windows.Forms.Button();
            this.btnInstall = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.Label();
            this.txtLog = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            //
            // lblTitle
            //
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblTitle.ForeColor = System.Drawing.Color.MidnightBlue;
            this.lblTitle.Location = new System.Drawing.Point(16, 16);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(262, 25);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "StarStrings QoL Installer";
            //
            // lblPath
            //
            this.lblPath.AutoSize = true;
            this.lblPath.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblPath.Location = new System.Drawing.Point(16, 56);
            this.lblPath.Name = "lblPath";
            this.lblPath.Size = new System.Drawing.Size(117, 15);
            this.lblPath.TabIndex = 1;
            this.lblPath.Text = "Star Citizen Directory:";
            //
            // txtStarCitizenPath
            //
            this.txtStarCitizenPath.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.txtStarCitizenPath.Location = new System.Drawing.Point(16, 76);
            this.txtStarCitizenPath.Name = "txtStarCitizenPath";
            this.txtStarCitizenPath.Size = new System.Drawing.Size(430, 23);
            this.txtStarCitizenPath.TabIndex = 2;
            this.txtStarCitizenPath.TextChanged += new System.EventHandler(this.txtStarCitizenPath_TextChanged);
            //
            // btnBrowse
            //
            this.btnBrowse.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.btnBrowse.Location = new System.Drawing.Point(454, 75);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(85, 25);
            this.btnBrowse.TabIndex = 3;
            this.btnBrowse.Text = "Browse...";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            //
            // lblEnvironment
            //
            this.lblEnvironment.AutoSize = true;
            this.lblEnvironment.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblEnvironment.Location = new System.Drawing.Point(16, 112);
            this.lblEnvironment.Name = "lblEnvironment";
            this.lblEnvironment.Size = new System.Drawing.Size(123, 15);
            this.lblEnvironment.TabIndex = 4;
            this.lblEnvironment.Text = "Target Environment:";
            //
            // cmbEnvironment
            //
            this.cmbEnvironment.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbEnvironment.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.cmbEnvironment.FormattingEnabled = true;
            this.cmbEnvironment.Location = new System.Drawing.Point(145, 109);
            this.cmbEnvironment.Name = "cmbEnvironment";
            this.cmbEnvironment.Size = new System.Drawing.Size(301, 23);
            this.cmbEnvironment.TabIndex = 5;
            this.cmbEnvironment.SelectedIndexChanged += new System.EventHandler(this.cmbEnvironment_SelectedIndexChanged);
            //
            // lblInstalledVersionHeader
            //
            this.lblInstalledVersionHeader.AutoSize = true;
            this.lblInstalledVersionHeader.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblInstalledVersionHeader.Location = new System.Drawing.Point(16, 150);
            this.lblInstalledVersionHeader.Name = "lblInstalledVersionHeader";
            this.lblInstalledVersionHeader.Size = new System.Drawing.Size(104, 15);
            this.lblInstalledVersionHeader.TabIndex = 6;
            this.lblInstalledVersionHeader.Text = "Installed Version:";
            //
            // lblInstalledVersion
            //
            this.lblInstalledVersion.AutoSize = true;
            this.lblInstalledVersion.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblInstalledVersion.Location = new System.Drawing.Point(126, 150);
            this.lblInstalledVersion.Name = "lblInstalledVersion";
            this.lblInstalledVersion.Size = new System.Drawing.Size(36, 15);
            this.lblInstalledVersion.TabIndex = 7;
            this.lblInstalledVersion.Text = "None";
            //
            // lblLatestVersionHeader
            //
            this.lblLatestVersionHeader.AutoSize = true;
            this.lblLatestVersionHeader.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblLatestVersionHeader.Location = new System.Drawing.Point(240, 150);
            this.lblLatestVersionHeader.Name = "lblLatestVersionHeader";
            this.lblLatestVersionHeader.Size = new System.Drawing.Size(90, 15);
            this.lblLatestVersionHeader.TabIndex = 8;
            this.lblLatestVersionHeader.Text = "Latest Version:";
            //
            // lblLatestVersion
            //
            this.lblLatestVersion.AutoSize = true;
            this.lblLatestVersion.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblLatestVersion.Location = new System.Drawing.Point(336, 150);
            this.lblLatestVersion.Name = "lblLatestVersion";
            this.lblLatestVersion.Size = new System.Drawing.Size(71, 15);
            this.lblLatestVersion.TabIndex = 9;
            this.lblLatestVersion.Text = "Checking...";
            //
            // btnCheckUpdates
            //
            this.btnCheckUpdates.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.btnCheckUpdates.Location = new System.Drawing.Point(16, 180);
            this.btnCheckUpdates.Name = "btnCheckUpdates";
            this.btnCheckUpdates.Size = new System.Drawing.Size(130, 30);
            this.btnCheckUpdates.TabIndex = 10;
            this.btnCheckUpdates.Text = "Recheck Updates";
            this.btnCheckUpdates.UseVisualStyleBackColor = true;
            this.btnCheckUpdates.Click += new System.EventHandler(this.btnCheckUpdates_Click);
            //
            // btnInstall
            //
            this.btnInstall.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnInstall.Location = new System.Drawing.Point(155, 180);
            this.btnInstall.Name = "btnInstall";
            this.btnInstall.Size = new System.Drawing.Size(160, 30);
            this.btnInstall.TabIndex = 11;
            this.btnInstall.Text = "Install / Update";
            this.btnInstall.UseVisualStyleBackColor = true;
            this.btnInstall.Click += new System.EventHandler(this.btnInstall_Click);
            //
            // lblStatus
            //
            this.lblStatus.AutoSize = true;
            this.lblStatus.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point);
            this.lblStatus.Location = new System.Drawing.Point(16, 222);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(42, 15);
            this.lblStatus.TabIndex = 12;
            this.lblStatus.Text = "Ready.";
            //
            // txtLog
            //
            this.txtLog.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.txtLog.Location = new System.Drawing.Point(16, 245);
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.ReadOnly = true;
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtLog.Size = new System.Drawing.Size(523, 110);
            this.txtLog.TabIndex = 13;
            //
            // Form1
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(555, 370);
            this.Controls.Add(this.txtLog);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.btnInstall);
            this.Controls.Add(this.btnCheckUpdates);
            this.Controls.Add(this.lblLatestVersion);
            this.Controls.Add(this.lblLatestVersionHeader);
            this.Controls.Add(this.lblInstalledVersion);
            this.Controls.Add(this.lblInstalledVersionHeader);
            this.Controls.Add(this.cmbEnvironment);
            this.Controls.Add(this.lblEnvironment);
            this.Controls.Add(this.btnBrowse);
            this.Controls.Add(this.txtStarCitizenPath);
            this.Controls.Add(this.lblPath);
            this.Controls.Add(this.lblTitle);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "StarStrings Installer";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblPath;
        private System.Windows.Forms.TextBox txtStarCitizenPath;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.Label lblEnvironment;
        private System.Windows.Forms.ComboBox cmbEnvironment;
        private System.Windows.Forms.Label lblInstalledVersionHeader;
        private System.Windows.Forms.Label lblInstalledVersion;
        private System.Windows.Forms.Label lblLatestVersionHeader;
        private System.Windows.Forms.Label lblLatestVersion;
        private System.Windows.Forms.Button btnCheckUpdates;
        private System.Windows.Forms.Button btnInstall;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.TextBox txtLog;
    }
}

using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;

namespace Pomowa
{
    public partial class Settings : Form
    {
        private AboutBox m_AboutBox = new AboutBox();

        public Settings()
        {
            InitializeComponent();
        }

        #region Button Events
        private void Cancel_Click(object sender, EventArgs e)
        {
            AppSettings.Default.Reset();
            this.Close();
        }

        private void Ok_Click(object sender, EventArgs e)
        {
            AppSettings.Default.ResumeCommand = ResumeCommand.Text;
            AppSettings.Default.SuspendCommand = SuspendCommand.Text;
            AppSettings.Default.Save();
            this.Close();
        }

        private void ResumeBrowse_Click(object sender, EventArgs e)
        {
            if (Directory.Exists(ResumeCommand.Text))
            {
                openFileDialog1.FileName = ResumeCommand.Text;
            }

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                ResumeCommand.Text = openFileDialog1.FileName;
            }
        }

        private void SuspendBrowse_Click(object sender, EventArgs e)
        {
            if (Directory.Exists(SuspendCommand.Text))
            {
                openFileDialog1.FileName = SuspendCommand.Text;
            }

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                SuspendCommand.Text = openFileDialog1.FileName;
            }
        }
        #endregion

        #region Form Events
        private void Settings_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.ShowInTaskbar = true;

            if (e.CloseReason != CloseReason.ApplicationExitCall)
            {
                e.Cancel = true;
                this.Hide();

                if (AppSettings.Default.ShowCloseTip)
                {
                    AppSettings.Default.ShowCloseTip = false;
                    AppSettings.Default.Save();
                    notifyIcon1.ShowBalloonTip(2500, "Information", "Pomowa is still running. To close, right-click on the Pomowa icon and select 'Close' from the options.", ToolTipIcon.Info);
                }
            }
        }

        private void Settings_Load(object sender, EventArgs e)
        {
            SystemEvents.PowerModeChanged += new PowerModeChangedEventHandler(SystemEvents_PowerModeChanged);
        }

        private void Settings_VisibleChanged(object sender, EventArgs e)
        {
            if (this.Visible == true)
            {
                ResumeCommand.Text = AppSettings.Default.ResumeCommand;
                SuspendCommand.Text = AppSettings.Default.SuspendCommand;
                this.ShowInTaskbar = true;
            }
        }
        #endregion

        #region ToolStrip Events
        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (m_AboutBox.Visible != true)
            {
                m_AboutBox.Show();
            }
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Show();
        }
        #endregion

        private void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            Process l_Process = new Process();
            l_Process.StartInfo.CreateNoWindow = true;
            l_Process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

            switch (e.Mode)
            {
                case PowerModes.Resume:
                    l_Process.StartInfo.FileName = AppSettings.Default.ResumeCommand;
                    break;
                case PowerModes.Suspend:
                    l_Process.StartInfo.FileName = AppSettings.Default.SuspendCommand;
                    break;
            }

            if (!string.IsNullOrEmpty(l_Process.StartInfo.FileName))
            {
                l_Process.Start();
                l_Process.WaitForExit(5000);

                if (!l_Process.HasExited)
                {
                    l_Process.Kill();
                    notifyIcon1.ShowBalloonTip(2500, "Error", "The command \"" + l_Process.StartInfo.FileName + "\" ran for too long and was terminated.", ToolTipIcon.Error);
                }
                else if (l_Process.ExitCode != 0)
                {
                    notifyIcon1.ShowBalloonTip(2500, "Warning", "The command \"" + l_Process.StartInfo.FileName + "\" exited with a code other than zero.", ToolTipIcon.Warning);
                }
            }
        }
    }
}

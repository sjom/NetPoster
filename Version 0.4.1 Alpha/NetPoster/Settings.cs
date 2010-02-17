using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace NetPoster
{
    public partial class Settings : Form
    {
        public Settings()
        {
            InitializeComponent();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void Settings_Load(object sender, EventArgs e)
        {
            usernameField.Text = Properties.Settings.Default.loginName;
            passwordField.Text = Properties.Settings.Default.loginPass;
            waitMinutes.Value = Properties.Settings.Default.waitTime;
            autoLogin.Checked = Properties.Settings.Default.autoLogin;
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.waitTime = (int)waitMinutes.Value;
            Properties.Settings.Default.loginName = usernameField.Text;
            Properties.Settings.Default.loginPass = passwordField.Text;
            Properties.Settings.Default.autoLogin = autoLogin.Checked;
            Properties.Settings.Default.Save();
            Close();
        }
    }
}

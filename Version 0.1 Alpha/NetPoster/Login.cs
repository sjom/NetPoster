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
    public partial class Login : Form
    {
        private WebBrowser browser;

        public Login()
        {
            InitializeComponent();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void loginButton_Click(object sender, EventArgs e)
        {
            status.Text = "Logging In...";
            loginButton.Enabled = false;
            browser = new WebBrowser();
            browser.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(browser_DocumentCompleted);
            browser.Navigate("http://net12.co.tv/api/post?username=" + usernameText.Text + "&password=" + passwordText.Text);
        }

        void browser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            string html = browser.DocumentText;
            if (html == "autherr")
            {
                status.Text = "Login Failed";
                loginButton.Enabled = true;
            }
            else
            {
                status.Text = "Login Success";
                loginButton.Enabled = true;
                this.Hide();
                MainForm frm = new MainForm(usernameText.Text, passwordText.Text);
                frm.ShowDialog();
                this.Close();
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using mshtml;
using System.IO;
using System.Net;

namespace NetPoster
{
    public partial class MainForm : Form
    {
        private string username;
        private string password;
        private int minutes = 5;
        WebBrowser poster = new WebBrowser();

        public MainForm(string username1, string password1)
        {
            InitializeComponent();
            this.username = username1;
            this.password = password1;
        }

        public void getPosts()
        {
            status.Text = "Getting Posts...";
            HttpWebRequest request = (HttpWebRequest)
            WebRequest.Create("http://net12.co.tv/api/get?username=" + username + "&password=" + password);

            // execute the request
            HttpWebResponse response = (HttpWebResponse)
                request.GetResponse();

            // we will read data via the response stream
            Stream resStream = response.GetResponseStream();
            StreamReader internetStream = new StreamReader(resStream);
            string html = internetStream.ReadToEnd();
            html = html.Replace("<div id=\"updates\">", "<html><head><style type=\"text/css\"> body { font-family: 'Calibri'; font-size: 12px; } .update { padding: 5px; background-color: #CEF0FF; border: solid thin #004A7F; color: #004A7F; } .left { padding: 2px; } .meta { padding: 2px; } a { text-decoration: none; color: #004A7F; } </style></head><body><div class=\"updates\">");
            html = html.Replace("<a href=", "<a id=");
            html = html.Replace("</div>", "</div><br />");
            viewer.DocumentText = html;
            status.Text = "Ready";
            updateTimerText();
            minutes = 5;
            startTimer();
        }

        private void startTimer()
        {
            timer1.Enabled = true;
            timer1.Start();
        }

        private void updateTimerText()
        {
            status.Text = "Refreshing in " + minutes + " minutes";
        }

        private void viewer_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
        }

        private void refreshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            getPosts();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Stop();
            timer1.Enabled = false;
            minutes--;
            if (minutes == 0)
            {
                getPosts();
                minutes = 5;
            }
            else
                updateTimerText();
            startTimer();
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            this.Text = "NetPoster - " + username + "'s Home";
            backgroundWorker1.RunWorkerAsync();
            startTimer();
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            getPosts();
        }

        private void postField_TextChanged(object sender, EventArgs e)
        {
            int length = postField.Text.Length;
            int characters = 250 - length;
            characterCounter.Text = characters.ToString();
            if (characters > 50)
                characterCounter.ForeColor = Color.Green;
            if (characters < 50 && characters > 0)
                characterCounter.ForeColor = Color.Orange;
            if (characters < 0)
                characterCounter.ForeColor = Color.Red;

            if (characters < 0)
                postButton.Enabled = false;
            else
                postButton.Enabled = true;
        }

        private void postButton_Click(object sender, EventArgs e)
        {
            status.Text = "Posting Update...";
            string url = "http://net12.co.tv/api/post?username=" + username + "&password=" + password + "&source=NetPoster&status=" + postField.Text;
            poster.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(poster_DocumentCompleted);
            poster.Navigate(url);
        }

        void poster_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            status.Text = "Posted!";
            postField.Text = "";
            characterCounter.Text = "250";
            characterCounter.ForeColor = Color.Green;
            getPosts();
            timer2.Enabled = true;
            timer2.Start();
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            timer2.Stop();
            timer2.Enabled = false;
            updateTimerText();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult dr = MessageBox.Show("Are you sure you want to exit?", "Quit", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dr == DialogResult.Yes)
                Close();
        }

        private void postField_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                postButton.PerformClick();
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Net;
using System.Diagnostics;
using System.Collections;
using System.Net.Json;
using System.Text.RegularExpressions;

namespace NetPoster
{
    public partial class MainForm : Form
    {
        private string username;
        private string password;
        private int minutes = Properties.Settings.Default.waitTime;
        WebBrowser poster = new WebBrowser();
        // Initializing HTML
        string html = "<html><head>";
        bool success = true;
        private string replyingToID = "-1";
        private string deleteID = "";

        public MainForm(string username1, string password1)
        {
            InitializeComponent();
            this.username = username1;
            this.password = password1;
        }

        public void getPosts()
        {
            html = "";
            File.Delete("C:\\NetPosterText.html");

            status.Text = "Getting Posts...";
            HttpWebRequest request = (HttpWebRequest)
            WebRequest.Create("http://net12.co.tv/api2/get.json?username=" + username + "&password=" + password);

            // execute the request
            HttpWebResponse response = (HttpWebResponse)
                request.GetResponse();

            // we will read data via the response stream
            Stream resStream = response.GetResponseStream();
            StreamReader internetStream = new StreamReader(resStream);
            string json = internetStream.ReadToEnd();

            // Parsing JSON
            System.Net.Json.JsonTextParser parser = new JsonTextParser();
            JsonObject obj = parser.Parse(json);
            
            // Adding Styles
            html += "<style type='text/css'>";
            html += "body {";
            html += "font-family: 'Calibri';";
            html += "font-size: 12px;";
            html += "}";
            html += ".update {";
            html += "background-color: #EDFFFB;";
            html += "border: thin #000 solid; padding: 5px;";
            html += "}";
            html += ".update2 {";
            html += "background-color: #EDFFFB;";
            html += "border: thin #000 solid; padding: 5px;";
            html += "}";
            html += ".buttonClass {";
            html += "background-color: #EDFFFB;";
            html += "border: thin #000 solid; padding: 5px;";
            html += "}";
            html += ".buttonClass:hover {";
            html += "background-color: #000;";
            html += "border: thin #000 solid; padding: 5px; color: #FFF;";
            html += "}";
            html += "a {";
            html += "text-decoration:none;";
            html += "color: #555;";
            html += "}";
            html += "a:hover {";
            html += "text-decoration:underline;";
            html += "}";
            html += "</style>";
            html += "</head>";
            html += "<body>";
            
            // Managing the Updates

            foreach (JsonObject mainVar in obj as JsonObjectCollection)
            {
                switch (mainVar.Name)
                {
                    case "error_code":
                        if (mainVar.GetValue().ToString() != "100")
                            success = false;
                        break;
                    case "updates":
                        List<JsonObject> updates = (List<JsonObject>)mainVar.GetValue();
                        
                        // Beginning the foreach loop, looping through each update.
                        foreach (JsonObject update in updates)
                        {                            
                            // Instantiating the variables for the update.
                            string update_id = "";
                            string replying_to_username = "";
                            string replying_to_id = "";
                            string updateStatus = "";
                            string time = "";
                            string source = "";
                            List<JsonObject> user = new List<JsonObject>();
                            string imageURL = "";
                            string updateUserName = "";

                            // Extracting the variables from the update.
                            foreach (JsonObject var in update as JsonObjectCollection)
                            {
                                switch (var.Name)
                                {
                                    case "update_id":
                                        update_id = var.GetValue().ToString();
                                        break;
                                    case "replying_to_username":
                                        replying_to_username = var.GetValue().ToString();
                                        break;
                                    case "replying_to_id":
                                        replying_to_id = var.GetValue().ToString();
                                        break;
                                    case "status":
                                        updateStatus = parseBoldText(ResolveLinks(var.GetValue().ToString()));
                                        break;
                                    case "time":
                                        time = var.GetValue().ToString();
                                        break;
                                    case "source":
                                        source = var.GetValue().ToString();
                                        break;
                                    case "user":
                                        user = (List<JsonObject>)var.GetValue();
                                        break;
                                }
                            }

                            // Getting the user's ImageURL
                            foreach (JsonObject var in user)
                            {
                                if (var.Name == "avatar")
                                {
                                    imageURL = var.GetValue().ToString();
                                }
                                else if (var.Name == "username")
                                {
                                    updateUserName = var.GetValue().ToString();
                                }
                            }

                            // Generating the HTML code for the variables.
                            html += "<br /><div class='update'>";
                            html += "<table>";
                            html += "<tr>";
                            html += "<td>";
                            html += "<img src='" + imageURL + "' style='width: 50px; height: 50px;' />";
                            html += "</td>";
                            html += "<td>";
                            html += "<b>" + updateUserName + "</b> - " + updateStatus;
                            html += "<br />";

                            //Figuring out the Date
                            string date = findDate(time);

                            if (replying_to_id != "0" || replying_to_username != "")
                                html += "Posted " + date + " from " + source + " in reply to " + replying_to_username + ".";
                            else
                                html += "Posted " + date + " from " + source + ".";
                            html += "</td></tr></table>";
                            html += "</div><br />";
                            if (updateUserName != username)
                            {
                                html += "<div id='taskPanel' class='update2' name='" + updateUserName + "'>";
                                html += "<button id='reply' name='" + update_id + "'>Reply</button>";
                                html += "</div><br />";
                            }
                            if (updateUserName == username)
                            {
                                html += "<div id='taskPanel' class='update2' name='" + updateUserName + "'>";
                                html += "<button id='delete' name='" + update_id + "'>Delete</button>";
                                html += "</div><br />";
                            }
                        }
                        break;
                }
            }

            if (success)
            {
                // Finishing off the HTML
                html += "</body></html>";

                // Creating the temp file
                File.WriteAllText("C:\\NetPosterText.html", html);
                System.Threading.Thread.Sleep(0);

                viewer.Navigate("C:\\NetPosterText.html");
            }

            status.Text = "Ready";
            minutes = Properties.Settings.Default.waitTime;
            updateTimerText();
            startTimer();
        }

        string findDate(string jsonDate)
        {
            string result = "";
            TimeSpan diff = (DateTime.Now.ToUniversalTime()) - (DateTime.Parse(jsonDate).ToUniversalTime());

            if (Math.Floor(diff.TotalSeconds) < 10)
                result = "just now";
            else if (Math.Floor(diff.TotalMinutes) < 1)
            {
                if (Math.Floor(diff.TotalSeconds) == 1)
                    result = Math.Floor(diff.TotalSeconds) + " second ago";
                else
                    result = Math.Floor(diff.TotalSeconds) + " seconds ago";
            }
            else if (Math.Floor(diff.TotalHours) < 1)
            {
                if (Math.Floor(diff.TotalMinutes) == 1)
                    result = Math.Floor(diff.TotalMinutes) + " minute ago";
                else
                    result = Math.Floor(diff.TotalMinutes) + " minutes ago";
            }
            else if (Math.Floor(diff.TotalDays) < 1)
            {
                if (Math.Floor(diff.TotalHours) == 1)
                    result = Math.Floor(diff.TotalHours) + " hour ago";
                else
                    result = Math.Floor(diff.TotalHours) + " hours ago";
            }
            else if (Math.Floor(diff.TotalDays) > 1)
            {
                if (Math.Floor(diff.TotalDays) == 1)
                    result = Math.Floor(diff.TotalDays) + " day ago";
                else
                    result = Math.Floor(diff.TotalDays) + " days ago";
            }

            return result;
        }

        string parseBoldText(string text)
        {
            string result = "";

            result = text;
            Regex reg = new Regex("\\*.*\\*");
            Match match = reg.Match(text);
            string matchStr = match.Value;
            if (matchStr != "")
            {
                string origional = matchStr;
                matchStr = matchStr.Remove(0, 1);
                matchStr = matchStr.Insert(0, "<b>");
                matchStr = matchStr.Remove(matchStr.Length - 1, 1);
                matchStr = matchStr.Insert(matchStr.Length, "</b>");
                result = result.Replace(origional, matchStr);
            }

            return result;
        }


        private static readonly Regex regex = new Regex("((http://|www\\.)([A-Z0-9.-:]{1,})\\.[0-9A-Z?;~&#=\\-_\\./]{2,})", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly string link = "<a target='_blank' href=\"{0}{1}\">{2}</a>";

        public static string ResolveLinks(string body)
        {
            if (string.IsNullOrEmpty(body))
                return body;

            foreach (Match match in regex.Matches(body))
            {
                if (!match.Value.Contains("://"))
                {
                    body = body.Replace(match.Value, string.Format(link, "http://", match.Value, ShortenUrl(match.Value, 50)));
                }
                else
                {
                    body = body.Replace(match.Value, string.Format(link, string.Empty, match.Value, ShortenUrl(match.Value, 50)));
                }
            }

            return body;
        }

        private static string ShortenUrl(string url, int max)
        {
            if (url.Length <= max)
                return url;

            // Remove the protocal
            int startIndex = url.IndexOf("://");
            if (startIndex > -1)
                url = url.Substring(startIndex + 3);

            if (url.Length <= max)
                return url;

            // Remove the folder structure
            int firstIndex = url.IndexOf("/") + 1;
            int lastIndex = url.LastIndexOf("/");
            if (firstIndex < lastIndex)
                url = url.Replace(url.Substring(firstIndex, lastIndex - firstIndex), "...");

            if (url.Length <= max)
                return url;

            // Remove URL parameters
            int queryIndex = url.IndexOf("?");
            if (queryIndex > -1)
                url = url.Substring(0, queryIndex);

            if (url.Length <= max)
                return url;

            // Remove URL fragment
            int fragmentIndex = url.IndexOf("#");
            if (fragmentIndex > -1)
                url = url.Substring(0, fragmentIndex);

            if (url.Length <= max)
                return url;

            // Shorten page
            firstIndex = url.LastIndexOf("/") + 1;
            lastIndex = url.LastIndexOf(".");
            if (lastIndex - firstIndex > 10)
            {
                string page = url.Substring(firstIndex, lastIndex - firstIndex);
                int length = url.Length - max + 3;
                url = url.Replace(page, "..." + page.Substring(length));
            }

            return url;
        } 

        void replyButton_Click(object sender, HtmlElementEventArgs e, string name, string replyToID)
        {
            postField.Text = "@";
            postField.Text += name;
            postField.Text += " ";
            postField.Select(postField.Text.Length, 1);
            replyingToID = replyToID;
        }

        void link_Click(object sender, HtmlElementEventArgs e)
        {
            string href = e.FromElement.GetAttribute("href");
            Process.Start(href);
        }

        void userLink_Click(object sender, HtmlElementEventArgs e)
        {
            string username = e.FromElement.GetAttribute("href").Replace("/", "");
            postField.Text = "@" + username + " ";
            postField.Select();
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
            if (success)
            {
                // Event Handling
                HtmlElementCollection taskPanels = (HtmlElementCollection)viewer.Document.Body.GetElementsByTagName("div");
                foreach (HtmlElement element in taskPanels)
                {
                    if (element.GetAttribute("id") == "taskPanel")
                    {
                        HtmlElement button = element.Children[0];
                        if (button.GetAttribute("id") == "reply")
                        {
                            string name = element.GetAttribute("name");
                            string replyTo = button.GetAttribute("name");
                            button.Click += new HtmlElementEventHandler(delegate { replyButton_Click(null, null, name, replyTo); });
                        }
                        else
                        {
                            string id = button.GetAttribute("name");
                            button.Click += new HtmlElementEventHandler(delegate { button_Click(null, null, id); });
                        }
                    }
                }
            }
            else
                MessageBox.Show("Could not load the updates due to a server error (Error Code: 200).", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        void button_Click(object sender, HtmlElementEventArgs e, string id)
        {
            // Deleting a Post
            status.Text = "Deleting Post...";
            deleteID = id;
            deleteBG.RunWorkerAsync();
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
            /*string url = "";
            string userReply = "";
            int startPos = postField.Text.IndexOf("@");
            if (startPos != -1)
            {
                int length = postField.Text.IndexOf(" ") - startPos;
                MessageBox.Show(length.ToString());
                userReply = postField.Text.Substring(startPos + 1, length);
            }
            MessageBox.Show(startPos.ToString());
            if (startPos == 0)
            {
                MessageBox.Show(userReply);
                url = "http://net12.co.tv/api2/post.json?username=" + username + "&password = " + password + "&source=NetPoster&app_token=e7911465b0fcc7e30db519a75783c2fd&status=" + postField.Text + "&reply_to_username=" + userReply;
            }
            else
            {*/
                string url = "http://net12.co.tv/api2/post.json?username=" + username + "&password=" + password + "&source=NetPoster&app_token=e7911465b0fcc7e30db519a75783c2fd&status=" + postField.Text;
            //}
            postBG.RunWorkerAsync();
            /*poster.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(poster_DocumentCompleted);
            poster.Navigate(url);*/
        }

        private string findBetweenText(string start, string end, string input)
        {
            string result = "";

            int startPos = input.IndexOf(start);
            int length = input.IndexOf(end) - startPos;
            result = input.Substring(startPos, length);

            return result;
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
            //Enter Submit
            /*if (e.KeyCode == Keys.Enter)
                postButton.PerformClick();*/
        }

        private void checkForUpdatesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SJSUpdater.Updates updater = new SJSUpdater.Updates(1, 2, this);
            updater.checkForUpdates(true);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            SJSUpdater.Updates updater = new SJSUpdater.Updates(1, 2, this);
            updater.checkForUpdates(false);
        }

        private void postBG_DoWork(object sender, DoWorkEventArgs e)
        {
            // The Post Method
            string url = "";
            if (replyingToID != "-1")
                url = "http://net12.co.tv/api2/post.json?username=" + username + "&password=" + password + "&source=NetPoster&app_token=e7911465b0fcc7e30db519a75783c2fd&status=" + postField.Text + "&replying_to_id=" + replyingToID;
            else
                url = "http://net12.co.tv/api2/post.json?username=" + username + "&password=" + password + "&source=NetPoster&app_token=e7911465b0fcc7e30db519a75783c2fd&status=" + postField.Text;

            // HTTP Requests
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            StreamReader reader = new StreamReader(response.GetResponseStream());
            string responseString = reader.ReadToEnd();
            reader.Close();
            replyingToID = "-1";
        }

        private void postBG_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            status.Text = "Posted!";
            postField.Text = "";
            characterCounter.Text = "250";
            characterCounter.ForeColor = Color.Green;
            getPosts();
            timer2.Enabled = true;
            timer2.Start();
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Settings set = new Settings();
            set.ShowDialog();
        }

        private void deleteBG_DoWork(object sender, DoWorkEventArgs e)
        {
            string url = "http://net12.co.tv/api2/delete.json?username=" + username + "&password=" + password + "&update_id=" + deleteID;
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            StreamReader read = new StreamReader(response.GetResponseStream());
            read.Close();
        }

        private void deleteBG_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            updateTimerText();
            getPosts();
        }
    }
}

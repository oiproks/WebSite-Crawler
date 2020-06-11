using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Http;
using System.Threading;
using System.Diagnostics;
using System.IO;
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using WebSiteCrawler.Properties;
using System.Configuration;
using System.Xml;
using System.Windows.Input;

namespace WebSiteCrawler
{
    public partial class Form1 : Form
    {
        private int sleep = 10000;
        private int counter = 0;
        private bool running = false;
        private const string LATEST_UPDATE = "Latest update: {0}";
        private const string CYCLE_COUNTER = "I've checked {0} times";
        List<Thread> threadList;

        private delegate void SafeCallDelegate(string text);

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            CheckSettings();

            time.Text = (sleep / 1000).ToString();

            latestUpdate.Text = string.Format(LATEST_UPDATE, DateTime.Now.ToString("dd/MM - HH:mm"));
        }

        private List<Tuple<string, string>> CheckSettings()
        {
            string[] appSettingsKeys = ConfigurationManager.AppSettings.AllKeys.ToArray();
            List<Tuple<string, string>>  settings = new List<Tuple<string, string>>();
            foreach (string key in appSettingsKeys)
            {
                string value = AES.DecryptString(ConfigurationManager.AppSettings[key]);

                if (string.IsNullOrEmpty(value)) {
                    while (string.IsNullOrEmpty(value))
                    {
                        value = Prompt.ShowDialog(key);
                    }

                    var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                    config.AppSettings.Settings[key].Value = AES.EncryptString(value); ;
                    config.Save(ConfigurationSaveMode.Modified);
                    ConfigurationManager.RefreshSection("appSettings");
                }

                settings.Add(new Tuple<string, string>(key, value));
            }

            return settings;
        }

        private string GetCode(string siteUrl)
        {
            try
            {
                HttpClient client = new HttpClient();
                using (HttpResponseMessage response = client.GetAsync(siteUrl).Result)
                {
                    using (HttpContent content = response.Content)
                    {
                        string code = content.ReadAsStringAsync().Result;

                        if (siteUrl.Contains("minambiente"))
                        {
                            code = code.Substring(0, code.IndexOf("<!-- Page cached by Boost"));
                            code = RemovePortion(code, "<html>", "</head>");
                            code = RemovePortion(code, "<header", "</header>");
                            code = RemovePortion(code, "<footer", "</footer>");
                            code = RemovePortion(code, "<script", "</script>");
                        } else if (siteUrl.Contains("beerwulf"))
                        {
                            int start = code.IndexOf("<div class=\"detail-content-block\">");
                            int end = code.IndexOf("<div class=\"fixed-content-container\">");
                            code = code.Substring(start, end-start);
                            if (code.ToLower().Contains("esaurito"))
                                code = "No 🍺 ... 🖕";
                            else
                                code = "🍺 🍻 🎉";
                            if (sleep >= 40)
                            {
                                WriteCodeOnFile(string.Format("Update on {1}: {0}", code, siteUrl));
                            }
                        }
                        return code;
                    }
                }
            } catch (Exception ex)
            {
                WriteCodeOnFile(ex.Message);
            }
            return string.Empty;
        }

        // This removes the header and all the script of the webpage code before storing it
        // I made this as scripts generate random gibberish at each iteration, not real updates
        private string RemovePortion(string text, string startWith, string endWith)
        {
            try
            {
                int check = Regex.Matches(text, startWith).Count;
                while (check >= 1)
                {
                    int indexStart = text.IndexOf(startWith);
                    int indexEnd = text.IndexOf(endWith, indexStart) + endWith.Length;
                    int removeLenght = indexEnd - indexStart;
                    text = text.Remove(indexStart, removeLenght);

                    check = Regex.Matches(text, startWith).Count;
                }
                return text;
            } catch { }
            return text;
        }

        private void WriteCodeOnFile(string code)
        {
            Singleton.Instance.WOF(code);
        }

        private void CheckForNews(string siteUrl)
        {
            try
            {
                string originalPage = string.Empty;
                while (Thread.CurrentThread.IsAlive)
                {
                    Thread.Sleep(sleep);

                    string newHTML = GetCode(siteUrl);

                    originalPage = string.IsNullOrEmpty(originalPage) ? newHTML: originalPage;

                    if (!string.IsNullOrEmpty(newHTML) && !originalPage.Equals(newHTML))
                    {
                        var doc = new HtmlAgilityPack.HtmlDocument();
                        doc.LoadHtml(newHTML);
                        var linkedPages = doc.DocumentNode.Descendants("a")
                                      .Select(a => a.GetAttributeValue("href", null))
                                      .Where(u => !string.IsNullOrEmpty(u));

                        originalPage = newHTML;
                        WriteCodeOnFile(originalPage);
                        string subject = "WEBSITE UPDATE ALERT";
                        string message = string.Format("New update available on {0}", siteUrl);

                        // Find all the links in the page, remove socials, adds the domain to the absolute one
                        foreach (string link in linkedPages)
                        {
                            if (link.ToLower().Contains("facebook")
                                || link.ToLower().Contains("twitter")
                                || link.ToLower().Contains("vimeo"))
                                continue;

                            string url;
                            if (link.StartsWith("/"))
                            {
                                string domain = siteUrl.Substring(0, siteUrl.IndexOf("/", siteUrl.IndexOf("/") + 2));
                                url = string.Concat(domain, link);
                            }
                            else
                                url = link;

                            message = string.Concat(message, Environment.NewLine, url);
                        }

                        if (DateTime.Now.DayOfWeek == DayOfWeek.Sunday || DateTime.Now.DayOfWeek == DayOfWeek.Saturday
                            || DateTime.Now.Hour < 08 || DateTime.Now.Hour > 17)
                        {
                            Tuple<bool, string> answer = mailSender.sendMail(subject, message);
                            if (!answer.Item1)
                            {
                                WriteCodeOnFile(string.Concat(message, Environment.NewLine, string.Format("Unable to send mail:\r\n{0}", answer.Item2)));
                            }
                        }
                        else
                        {
                            this.Invoke(new Action(() =>
                            {
                                latestUpdate.Text = string.Format(LATEST_UPDATE, DateTime.Now.ToString("dd/MM/yyyy HH:mm"));
                            }));

                            this.Invoke(new Action(() =>
                            {
                                MessageBox.Show(
                                this,
                                message,
                                subject,
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Exclamation);
                            }));

                            Process.Start("chrome.exe", siteUrl);
                        }
                    }
                    counter++;

                    this.Invoke(new Action(() =>
                    {
                        Counter.Text = string.Format(CYCLE_COUNTER, counter.ToString());
                    }));
                }
            } catch (Exception ex)
            {
                
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!running)
            {
                threadList = new List<Thread>();
                button1.BackgroundImage = Resources.stop;
                foreach (Tuple<string, string> tuple in CheckSettings())
                {
                    if (tuple.Item1.Contains("website")) {
                        Thread t = new Thread(() => CheckForNews(tuple.Item2));
                        threadList.Add(t);
                        t.Start();

                        running = true;
                    }
                }
            } else
            {
                button1.BackgroundImage = Resources.play;
                foreach (Thread t in threadList)
                    t.Interrupt();
                running = false;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (running)
                button1_Click(sender, e);
            Settings.EditSettings(CheckSettings());
        }

        private void time_TextChanged(object sender, EventArgs e)
        {
            sleep = int.Parse(time.Text) * 1000;
        }

        private void time_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        // All the following is meant to enable the app interface to be dragged around.
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        private void Form1_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }
    }
}

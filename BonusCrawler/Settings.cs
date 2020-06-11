using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WebSiteCrawler
{
    public static class Settings
    {
        static Panel panel;
        static int webSiteNumber = 1, controlsY = 0, counter = 1;

        public static void EditSettings(List<Tuple<string, string>> settings)
        {
            webSiteNumber = 1;
            counter = 1;

            Form form = new Form
            {
                AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F),
                AutoScaleMode = AutoScaleMode.Font,
                BackColor = System.Drawing.Color.White,
                ClientSize = new System.Drawing.Size(237, 250),
                MinimumSize = new System.Drawing.Size(253, 290),
                FormBorderStyle = FormBorderStyle.SizableToolWindow,
                Name = "Test",
                StartPosition = FormStartPosition.CenterScreen,
                Text = "Test",
                TopMost = true
            };

            form.FormClosing += Form_FormClosing;

            Label lblSettings = new Label
            {
                Anchor = (AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right),
                FlatStyle = FlatStyle.Flat,
                Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0))),
                Location = new System.Drawing.Point(12, 9),
                Name = "lblSettings",
                Size = new System.Drawing.Size(213, 23),
                TabIndex = 0,
                Text = "Settings",
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter
            };

            panel = new Panel
            {
                AutoScroll = true,
                Anchor = (AnchorStyles.Bottom | AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right),
                BackColor = System.Drawing.Color.Transparent,
                Location = new System.Drawing.Point(12, 35),
                Margin = new Padding(0),
                Name = "flp",
                Size = new System.Drawing.Size(213, 170),
                MinimumSize = new System.Drawing.Size(213, 170)
            };

            foreach (Tuple<string, string> tuple in settings)
            {
                AddElements(tuple.Item1, tuple.Item2);
            }

            Button btnSave = new Button
            {
                Anchor = AnchorStyles.Right | AnchorStyles.Bottom,
                BackColor = System.Drawing.Color.Transparent,
                BackgroundImage = Properties.Resources.save,
                BackgroundImageLayout = ImageLayout.Zoom,
                FlatStyle = FlatStyle.Flat,
                Location = new System.Drawing.Point(195, 208),
                Name = "Save",
                Size = new System.Drawing.Size(30, 30),
                UseVisualStyleBackColor = false,
                DialogResult = DialogResult.OK
            };
            btnSave.FlatAppearance.BorderColor = System.Drawing.Color.Black;
            btnSave.Click += (sender, e) => {
                for (int x = 1; x <= panel.Controls.Count / 2; x++)
                {
                    string lblName = string.Concat("label", x.ToString());
                    string txtName = string.Concat("textbox", x.ToString());
                    var test = lblName.ToString();
                    var test2 = panel.Controls.Find(test, true);
                    var test3 = test2[0];
                    var test4 = test3.ToString();
                    string key = panel.Controls.Find(lblName.ToString(), true)[0].Text.ToString();
                    string value = panel.Controls.Find(txtName.ToString(), true)[0].Text.ToString();


                    var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

                    if (string.IsNullOrEmpty(value) && (!key.Equals("username") || !key.Equals("password") || !key.Equals("website1")))
                    {
                        config.AppSettings.Settings.Remove(key);
                    }
                    else
                    {
                        value = AES.EncryptString(value);
                        if (ConfigurationManager.AppSettings[key] == null)
                            config.AppSettings.Settings.Add(key, value);
                        else
                            config.AppSettings.Settings[key].Value = value;
                    }

                    config.Save(ConfigurationSaveMode.Modified);
                    ConfigurationManager.RefreshSection("appSettings");
                }
                controlsY = 0;
                counter = 0;
                form.Close();
            };

            Button btnAdd = new Button
            {
                Anchor = AnchorStyles.Left | AnchorStyles.Bottom,
                BackColor = System.Drawing.Color.Transparent,
                BackgroundImage = Properties.Resources.add,
                BackgroundImageLayout = ImageLayout.Zoom,
                FlatStyle = FlatStyle.Flat,
                Location = new System.Drawing.Point(12, 208),
                Name = "button3",
                Size = new System.Drawing.Size(30, 30),
                UseVisualStyleBackColor = false
            };
            btnAdd.FlatAppearance.BorderColor = System.Drawing.Color.Black;
            btnAdd.Click += (sender, e) => {
                string label = string.Concat("website", webSiteNumber.ToString());
                AddElements(label);
            };

            form.Controls.Add(lblSettings);
            form.Controls.Add(panel);
            form.Controls.Add(btnAdd);
            form.Controls.Add(btnSave);

            form.ShowDialog();
        }

        private static void Form_FormClosing(object sender, FormClosingEventArgs e)
        {
            controlsY = 0;
            counter = 0;
        }

        private static void AddElements(string label, string text = "")
        {
            int width = panel.Size.Width - 70;
            if (label.Contains("website"))
            {
                TextBox prev = (TextBox)panel.Controls.Find(string.Format("textbox{0}", (counter - 1).ToString()), false).First();
                width = prev.Size.Width;
                if (//panel.Controls.Count >= 6 &&
                    prev.Location.Y + 43 >= panel.Size.Height &&
                    !panel.VerticalScroll.Visible)
                    width -= SystemInformation.VerticalScrollBarWidth;

                webSiteNumber = int.Parse(label.Substring(label.Length - 1)) + 1;
            }
            Label label1 = new Label
            {
                Anchor = (AnchorStyles.Top | AnchorStyles.Left),
                AutoSize = false,
                Location = new System.Drawing.Point(0, controlsY),
                Name = string.Format("label{0}", counter.ToString()),
                Size = new System.Drawing.Size(60, 20),
                Text = label,
                Margin = new Padding(0, 0, 0, 5),
                Padding = new Padding(0, 5, 0, 0),
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft

            };
            
            TextBox textBox1 = new TextBox
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                BackColor = System.Drawing.SystemColors.Window,
                Location = new System.Drawing.Point(63, controlsY),
                Name = string.Format("textbox{0}", counter.ToString()),
                Size = new System.Drawing.Size(width, 20),
                Text = text
            };
            if (label.Equals("password"))
                textBox1.PasswordChar = '*';

            panel.Controls.Add(label1);
            panel.Controls.Add(textBox1);

            controlsY += 25;
            counter++;
        }
    }
}

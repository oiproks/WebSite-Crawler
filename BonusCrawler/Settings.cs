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
        static FlowLayoutPanel flp;
        static int webSiteNumber = 0, controlsY = 0, counter = 1;

        public static void EditSettings(List<Tuple<string, string>> settings)
        {
            Form form = new Form
            {
                AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F),
                AutoScaleMode = AutoScaleMode.Font,
                BackColor = System.Drawing.Color.White,
                ClientSize = new System.Drawing.Size(237, 250),
                FormBorderStyle = FormBorderStyle.None,
                Name = "Test",
                StartPosition = FormStartPosition.CenterScreen,
                Text = "Test",
                TopMost = true
            };

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

            flp = new FlowLayoutPanel
            {
                AutoScroll = true,
                BackColor = System.Drawing.Color.Transparent,
                Location = new System.Drawing.Point(12, 35),
                Margin = new Padding(0),
                Name = "flp",
                Size = new System.Drawing.Size(213, 170)
            };

            foreach (Tuple<string, string> tuple in settings)
            {
                if (tuple.Item1.Contains("website"))
                    webSiteNumber++;
                AddElements(tuple.Item1, tuple.Item2);
            }

            Button button1 = new Button
            {
                BackColor = System.Drawing.Color.Transparent,
                BackgroundImage = Properties.Resources.back,
                BackgroundImageLayout = ImageLayout.Zoom,
                FlatStyle = FlatStyle.Flat,
                Location = new System.Drawing.Point(145, 208),
                Name = "button1",
                Size = new System.Drawing.Size(30, 30),
                UseVisualStyleBackColor = false,
                DialogResult = DialogResult.Cancel
            };
            button1.FlatAppearance.BorderColor = System.Drawing.Color.Black;
            button1.Click += (sender, e) => {
                form.Close();
            };

            Button button2 = new Button
            {
                BackColor = System.Drawing.Color.Transparent,
                BackgroundImage = Properties.Resources.save,
                BackgroundImageLayout = ImageLayout.Zoom,
                FlatStyle = FlatStyle.Flat,
                Location = new System.Drawing.Point(195, 208),
                Name = "button2",
                Size = new System.Drawing.Size(30, 30),
                UseVisualStyleBackColor = false,
                DialogResult = DialogResult.OK
            };
            button2.FlatAppearance.BorderColor = System.Drawing.Color.Black;
            button2.Click += (sender, e) => {
                for (int x = 1; x <= flp.Controls.Count / 2; x++)
                {
                    string lblName = string.Concat("label", x.ToString());
                    string txtName = string.Concat("textbox", x.ToString());
                    string key = flp.Controls.Find(lblName.ToString(), true)[0].Text.ToString();
                    string value = flp.Controls.Find(txtName.ToString(), true)[0].Text.ToString();

                    value = AES.EncryptString(value);

                    var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                    if (ConfigurationManager.AppSettings[key] == null)
                        config.AppSettings.Settings.Add(key, value);
                    else
                    {
                        if (string.IsNullOrEmpty(value) && (!key.Equals("username") || !key.Equals("password") || !key.Equals("website1")))
                            config.AppSettings.Settings.Remove(key);
                        else
                            config.AppSettings.Settings[key].Value = value;

                    }

                    config.Save(ConfigurationSaveMode.Modified);
                    ConfigurationManager.RefreshSection("appSettings");
                }
                form.Close();
            };

            Button button3 = new Button
            {
                BackColor = System.Drawing.Color.Transparent,
                BackgroundImage = Properties.Resources.add,
                BackgroundImageLayout = ImageLayout.Zoom,
                FlatStyle = FlatStyle.Flat,
                Location = new System.Drawing.Point(12, 208),
                Name = "button3",
                Size = new System.Drawing.Size(30, 30),
                UseVisualStyleBackColor = false
            };
            button3.FlatAppearance.BorderColor = System.Drawing.Color.Black;
            button3.Click += (sender, e) => {
                string label = string.Concat("website", webSiteNumber.ToString());
                AddElements(label);
            };

            form.Controls.Add(lblSettings);
            form.Controls.Add(flp);
            form.Controls.Add(button3);
            form.Controls.Add(button2);
            form.Controls.Add(button1);

            form.ShowDialog();
        }

        private static void AddElements(string label, string text = "")
        {
            Label label1 = new Label
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
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
                Name = string.Format("textbox{0}", counter),
                Size = new System.Drawing.Size(130, 20),
                Text = text
            };
            if (label.Equals("password"))
                textBox1.PasswordChar = '*';

            flp.Controls.Add(label1);
            flp.Controls.Add(textBox1);

            controlsY += 25;
            counter++;

            if (label.Contains("website"))
                webSiteNumber++;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WebSiteCrawler
{
    static class Prompt
    {
        static Button btnOK;

        public static string ShowDialog(string key)
        {
            Form prompt = new Form
            {
                AutoScaleDimensions = new SizeF(6F, 13F),
                AutoScaleMode = AutoScaleMode.Font,
                ClientSize = new Size(224, 99),
                StartPosition = FormStartPosition.CenterScreen
            };

            Label lblMessage = new Label
            {
                AutoSize = true,
                Location = new Point(12, 12),
                MaximumSize = new Size(200, 0),
                MinimumSize = new Size(200, 0),
                Name = "lblMessage",
                Size = new Size(200, 26),
                Text = string.Format(string.Format("Please provide value for \"{0}\":", key))
            };

            TextBox txtValue = new TextBox
            {
                Location = new Point(12, 41),
                Name = "txtValue",
                Size = new Size(200, 20)                
            };
            txtValue.KeyDown += TxtValue_KeyDown;
            if (key.Equals("password"))
                txtValue.PasswordChar = '*';

            btnOK = new Button
            {
                FlatStyle = FlatStyle.System,
                Location = new Point(162, 67),
                Name = "btnOK",
                Size = new Size(50, 23),
                Text = "OK",
                UseVisualStyleBackColor = true,
                DialogResult = DialogResult.OK,
                TabStop = false
            };
            btnOK.Click += (sender, e) => {
                prompt.Close();
            };

            prompt.Controls.Add(btnOK);
            prompt.Controls.Add(txtValue);
            prompt.Controls.Add(lblMessage);
            prompt.AcceptButton = btnOK;
            prompt.FormBorderStyle = FormBorderStyle.None;
            prompt.Text = string.Format("{0} missing", key);

            txtValue.Focus();

            return prompt.ShowDialog() == DialogResult.OK ? txtValue.Text : "";
        }

        private static void TxtValue_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                btnOK.PerformClick();
            }
            e.Handled = false;
        }
    }
}

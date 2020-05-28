using WebSiteCrawler.Properties;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace WebSiteCrawler
{
    static class mailSender
    {
        public static Tuple<bool, string> sendMail(string subject, string message)
        {
            Tuple<bool, string> answer;
            try
            {
                MailMessage mail = new MailMessage();
                SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");

                mail.From = new MailAddress(ConfigurationManager.AppSettings["username"]);
                mail.To.Add(ConfigurationManager.AppSettings["username"]);
                mail.Subject = subject;
                mail.Body = message;

                SmtpServer.Port = 587;
                SmtpServer.Credentials = new System.Net.NetworkCredential(ConfigurationManager.AppSettings["username"], ConfigurationManager.AppSettings["password"]);
                SmtpServer.EnableSsl = true;

                SmtpServer.Send(mail);

                answer = new Tuple<bool, string> (true, "Done");
            }
            catch (Exception ex)
            {
                answer = new Tuple<bool, string>(false, ex.Message);
            }

            return answer;
        }
    }
}

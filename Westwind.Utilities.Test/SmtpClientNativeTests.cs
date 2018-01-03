using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Westwind.Utilities.InternetTools;

namespace Westwind.Utilities.Test
{
    [TestClass]
    public class SmtpClientNativeTests
    {
        bool isMailServerAvailable = false;

        /// <summary>
        /// For this test to run make sure Email Server is available
        /// </summary>
        [TestMethod]
        public void SendMailTest()
        {            
            SmtpClientNative smtp = new SmtpClientNative();
            smtp.MailServer = TestConfigurationSettings.Mailserver;
            smtp.Username = TestConfigurationSettings.MailServerUsername;
            smtp.Password = TestConfigurationSettings.MailServerPassword;
            smtp.UseSsl = TestConfigurationSettings.MailServerUseSsl;

            
            smtp.SenderEmail = "admin@test.com";

            smtp.Recipient = "test@test.com";
            smtp.Subject = "Test Message";
            smtp.Message = "Test Message Content from <b>Test SMTP Client</b>";
            smtp.ContentType = "text/html";
            
            bool result = smtp.SendMail();
            
            if (isMailServerAvailable)                
                Assert.IsTrue(result, smtp.ErrorMessage);
        }

        /// <summary>
        /// For this test to run make sure Email Server is available
        /// </summary>
        [TestMethod]
        public void SendMailAsyncTest()
        {
            SmtpClientNative smtp = new SmtpClientNative();
            smtp.MailServer = TestConfigurationSettings.Mailserver;
            smtp.Username = TestConfigurationSettings.MailServerUsername;
            smtp.Password = TestConfigurationSettings.MailServerPassword;
            smtp.UseSsl = TestConfigurationSettings.MailServerUseSsl;


            smtp.SenderEmail = "admin@test.com";

            smtp.Recipient = "test@test.com";
            smtp.Subject = "Async Test Message";
            smtp.Message = "Test Message Content from <b>Async Test SMTP Client</b>";
            smtp.ContentType = "text/html";

            smtp.SendMailAsync();
        }

    }

}

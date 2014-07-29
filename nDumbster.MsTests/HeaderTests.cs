using System;
using System.Linq;
using System.Net.Mail;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using nDumbster.Smtp;

namespace nDumbster.MsTests
{
    [TestClass]
    public class HeaderTests
    {
        [TestMethod, TestCategory("Integration")]
        public void WhenUsingHeaders_ThenHeadersReturned()
        {
            using (var client = new SmtpClient { Host = "localhost", Port = 25 })
            {
                var msg = new MailMessage("foo@doagree.com", "bar@doagree.com", "some subject", "some body");
                msg.Headers.Add("foo", "bar");
                client.Send(msg);
            }

            int waitLoopCount = 0;
            var smtpServer = SimpleSmtpServer.Start(25);

            while (smtpServer.ReceivedEmailCount == 0 && waitLoopCount < 100)
            {
                waitLoopCount++;
                Thread.Sleep(300);
            }

            var email = smtpServer.ReceivedEmail.First();

            Assert.AreEqual("some body", email.Body);
            Assert.AreEqual("some subject", email.Subject);
            Assert.AreEqual("bar", email.Headers.Get("foo"));
        }
    }
}

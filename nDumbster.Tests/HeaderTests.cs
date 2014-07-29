using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading;
using NUnit.Framework;
using nDumbster.Smtp;

namespace nDumbster.Tests
{
    [TestFixture]
    public class HeaderTests
    {
        [Test]
        public void WhenUsingHeaders_ThenHeadersReturned()
        {
            var smtpServer = SimpleSmtpServer.Start(25);

            using (var client = new SmtpClient { Host = "localhost", Port = 25 })
            {
                var msg = new MailMessage("foo@doagree.com", "bar@doagree.com", "some subject", "some body");
                msg.Headers.Add("foo", "bar");
                client.Send(msg);
            }

            int waitLoopCount = 0;

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

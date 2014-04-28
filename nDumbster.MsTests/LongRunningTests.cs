using System;
using System.Linq;
using System.Net.Mail;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using nDumbster.Smtp;

namespace nDumbster.MsTests
{
    [TestClass]
    public class LongRunningTests
    {
        [TestMethod]
        public void ShortIterationTest()
        {
            var server = SmtpSetup.SmtpServer;
            
            for (int i = 0; i < 1000; i++)
            {
                server.ClearReceivedEmail();

                using (var smtp = new SmtpClient { Host = "localhost", Port = 25 })
                {
                    var msg = new MailMessage
                        {
                            Body = "Email: " + i, 
                            Subject = "Subject: " + i, 
                            From = new MailAddress("somebody@foo.com")
                        };

                    msg.To.Add(new MailAddress("everybody@bar.com"));
                    smtp.Send(msg);
                }

                int waitLoopCount = 0;

                while (SmtpSetup.SmtpServer.ReceivedEmailCount == 0 && waitLoopCount < 100)
                {
                    waitLoopCount++;
                    Thread.Sleep(300);
                }

                var email = server.ReceivedEmail.First();

                Assert.AreEqual("Email: " + i, email.Body);
            }
        }

        [TestMethod]
        public void LongIterationTest()
        {
            var server = SmtpSetup.SmtpServer;

            for (int i = 0; i < 500; i++)
            {
                server.ClearReceivedEmail();

                using (var smtp = new SmtpClient { Host = "localhost", Port = 25 })
                {
                    var msg = new MailMessage
                    {
                        Body = "Email: " + i,
                        Subject = "Subject: " + i,
                        From = new MailAddress("somebody@foo.com")
                    };

                    msg.To.Add(new MailAddress("everybody@bar.com"));
                    smtp.Send(msg);
                }

                Thread.Sleep(100);

                int waitLoopCount = 0;

                while (SmtpSetup.SmtpServer.ReceivedEmailCount == 0 && waitLoopCount < 100)
                {
                    waitLoopCount++;
                    Thread.Sleep(300);
                }

                var email = server.ReceivedEmail.First();

                Assert.AreEqual("Email: " + i, email.Body);
            }
        }


        [TestMethod]
        public void LongWaitInBetweenMessagesTest()
        {
            var server = SmtpSetup.SmtpServer;

            for (int i = 0; i < 3; i++)
            {
                server.ClearReceivedEmail();

                using (var smtp = new SmtpClient { Host = "localhost", Port = 25 })
                {
                    var msg = new MailMessage
                    {
                        Body = "Email: " + i,
                        Subject = "Subject: " + i,
                        From = new MailAddress("somebody@foo.com")
                    };

                    msg.To.Add(new MailAddress("everybody@bar.com"));
                    smtp.Send(msg);
                }

                int waitLoopCount = 0;

                while (SmtpSetup.SmtpServer.ReceivedEmailCount == 0 && waitLoopCount < 20)
                {
                    waitLoopCount++;
                    Thread.Sleep(100);
                }

                var email = server.ReceivedEmail.First();

                Assert.AreEqual("Email: " + i, email.Body);

                Thread.Sleep(TimeSpan.FromSeconds(20));
            }
        }
    }
}

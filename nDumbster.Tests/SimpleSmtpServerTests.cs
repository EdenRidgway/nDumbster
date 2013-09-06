using System;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Reflection;
using System.Threading;
using NUnit.Framework;
using OpenPop.Mime;
using nDumbster.Smtp;
using System.Web.Mail;
using MailMessage = System.Net.Mail.MailMessage;

namespace nDumbster.Tests
{
	/// <summary>
	/// Summary description for SimpleSmtpServerTests
	/// </summary>
	[TestFixture]
	public class SimpleSmtpServerTests
	{
		private SimpleSmtpServer server;
        private SimpleSmtpServer server2;
        
        private int TEST_ALT_PORT = 15525;
        private int ALT_PORT = 22525;

		public SimpleSmtpServerTests()
		{
			server = null;
			server2 = null;
		}

		[SetUp]
		protected void SetUp() 
		{
			server = SimpleSmtpServer.Start(TEST_ALT_PORT);
			server2 = null;
		}

		[TearDown]
		protected void TearDown() 
		{
			if (server != null)
			{
			    server.Stop();
			}

			if (server2 != null)
			{
			    server2.Stop();
			}
		}

        [Test]
		public void SendMessage ()
		{
			using (var smtp = new SmtpClient { Host = "localhost", Port = TEST_ALT_PORT }) 
			{
				var msg = new MailMessage { Body = "This is the body", Subject = "This is the subject", From = new MailAddress("somebody@foo.com") };
                msg.To.Add(new MailAddress("everybody@bar.com"));
                smtp.Send(msg);

				Assert.AreEqual (1, server.ReceivedEmail.Count (), "server.ReceivedEmail.Length");
				var email = server.ReceivedEmail.First ();

	            Assert.AreEqual("everybody@bar.com", email.To.First().Address);
	            Assert.AreEqual("somebody@foo.com", email.From.Address);

	            Assert.AreEqual("This is the subject", email.Subject);
	            Assert.AreEqual("This is the body", email.Body);
			}
        }

		[Test]
		public void SendMessageWithAttachment()
        {
            using (var smtp = new SmtpClient { Host = "localhost", Port = TEST_ALT_PORT })
            {
                var msg = new MailMessage { Body = "This is the body", Subject = "This is the subject", From = new MailAddress("somebody@foo.com") };
                msg.To.Add(new MailAddress("everybody@bar.com"));

                Stream attachmentStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("nDumbster.Tests.rfc2821.txt");
                string attachmentText;
                using (var attachmentReader = new StreamReader(attachmentStream))
                {
                    attachmentText = attachmentReader.ReadToEnd();
                }

                attachmentStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("nDumbster.Tests.rfc2821.txt");
                msg.Attachments.Add(new Attachment(attachmentStream, "rfc2821.txt"));

                smtp.Send(msg);
                Assert.AreEqual(1, server.ReceivedEmail.Count(), "server.ReceivedEmail.Length");
                Assert.AreEqual(1, server.ReceivedEmailCount, "server.ReceivedEmailSize");

                var email = server.ReceivedEmail.First();

                Assert.AreEqual("everybody@bar.com", email.To.First().Address);
                Assert.AreEqual("somebody@foo.com", email.From.Address);

                Assert.AreEqual("This is the subject", email.Subject);
                Assert.AreEqual("This is the body", email.Body);

                var attachments = email.Attachments;
                Assert.That(attachments.Count, Is.EqualTo(1));

                using (var reader = new StreamReader(attachments[0].ContentStream))
                {
                    Assert.That(attachmentText, Is.EqualTo(reader.ReadToEnd()));
                }
            }
		}

		[Test]
		public void SendTwoMessages()
		{
		    using (var smtp = new SmtpClient { Host = "localhost", Port = TEST_ALT_PORT })
		    {
                var msg = new MailMessage { Body = "This is the body.", Subject = "This is the subject", From = new MailAddress("somebody@foo.com") };
                msg.To.Add(new MailAddress("everybody@bar.com"));
                smtp.Send(msg);

                msg = new MailMessage { Body = "This is the second body.", Subject = "This is the second subject", From = new MailAddress("nobody@test.com") };
                msg.To.Add(new MailAddress("test2@test.com"));
                smtp.Send(msg);

		        Assert.AreEqual(2, server.ReceivedEmail.Count(), "server.ReceivedEmail.Length");
		        Assert.AreEqual(2, server.ReceivedEmailCount, "server.ReceivedEmailSize");

		        var email1 = server.ReceivedEmail.First();

                Assert.AreEqual("somebody@foo.com", email1.From.Address);
                Assert.AreEqual("everybody@bar.com", email1.To.First().Address);

		        Assert.AreEqual("This is the subject", email1.Subject);
                Assert.AreEqual("This is the body.", email1.Body);

		        var email2 = server.ReceivedEmail.Skip(1).First();

                Assert.AreEqual("test2@test.com", email2.To.First().Address);
                Assert.AreEqual("nobody@test.com", email2.From.Address);

		        Assert.AreEqual("This is the second subject", email2.Subject);
		        Assert.AreEqual("This is the second body.", email2.Body);
		    }
		}

		[Test]
		[ExpectedException(typeof( System.Net.Sockets.SocketException))]
		public void ServerBindingError()
		{
            // Server is already running. We check that this cause an SocketException to be thrown
            SimpleSmtpServer.Start(TEST_ALT_PORT);

			Assert.Fail("BindingError");
		}

        [Test]
        public void ServerCanRunOnDifferentPort()
        {
            // Server is already running. We check that this cause an SocketException to be thrown
            var alternateServer = SimpleSmtpServer.Start(ALT_PORT);
            alternateServer.Stop();
        }
	}
}


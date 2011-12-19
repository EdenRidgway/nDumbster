using System;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Reflection;
using NUnit.Framework;
using nDumbster.smtp;
using System.Web.Mail;
using MailMessage = System.Web.Mail.MailMessage;

namespace nDumbster.Tests
{
	/// <summary>
	/// Summary description for SimpleSmtpServerTests
	/// </summary>
	[TestFixture]
	public class SimpleSmtpServerTests
	{
		SimpleSmtpServer server;
		SimpleSmtpServer server2;

		public SimpleSmtpServerTests()
		{
			server = null;
			server2 = null;
		}

		[SetUp]
		protected void SetUp() 
		{
			server = SimpleSmtpServer.Start();
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
        public void SendMessage()
        {
            System.Web.Mail.SmtpMail.SmtpServer = "localhost";
            System.Web.Mail.SmtpMail.Send("somebody@foo.com", "everybody@bar.com", "This is the subject", "This is the body.");

            Assert.AreEqual(1, server.ReceivedEmail.Count(), "server.ReceivedEmail.Length");
            Assert.AreEqual(1, server.ReceivedEmailCount, "server.ReceivedEmailSize");

            SmtpMessage email = server.ReceivedEmail.First();

            Assert.AreEqual("<everybody@bar.com>", email.Headers["To"]);
            Assert.AreEqual("<somebody@foo.com>", email.Headers["From"]);

            Assert.AreEqual("text/plain;", email.Headers["Content-Type"]);

            Assert.AreEqual("This is the subject", email.Headers["Subject"]);
            Assert.AreEqual("This is the body.", email.Body);
        }

		[Test]
		public void SendMessageWithAttachment()
        {
            using (var smtp = new SmtpClient { Host = "localhost" })
            {
                var msg = new System.Net.Mail.MailMessage { Body = "This is the body", Subject = "This is the subject", From = new MailAddress("somebody@foo.com") };
                msg.To.Add(new MailAddress("everybody@bar.com"));

                Stream attachementStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("nDumbster.Tests.rfc2821.txt");
                msg.Attachments.Add(new Attachment(attachementStream, "rfc2821.txt"));

                smtp.Send(msg);
                Assert.AreEqual(1, server.ReceivedEmail.Count(), "server.ReceivedEmail.Length");
                Assert.AreEqual(1, server.ReceivedEmailCount, "server.ReceivedEmailSize");

                SmtpMessage email = server.ReceivedEmail.First();

                Assert.AreEqual("everybody@bar.com", email.Headers["To"]);
                Assert.AreEqual("somebody@foo.com", email.Headers["From"]);

                Assert.That(email.Headers["Content-Type"], Is.StringStarting("multipart/mixed"));

                Assert.AreEqual("This is the subject", email.Headers["Subject"]);
            }
		}

		[Test]
		public void SendMessagesWithCarriageReturn() 
		{
			String bodyWithCR = "\n\nKeep these pesky carriage returns\n\nPlease,\nPlease\n\n";

			System.Web.Mail.SmtpMail.SmtpServer = "localhost";
			SmtpMessage.CR = "\n";
			System.Web.Mail.SmtpMail.Send("somebody@foo.com", "everybody@bar.com", "CRTest", bodyWithCR);

			Assert.AreEqual(1, server.ReceivedEmail.Count(), "server.ReceivedEmail.Length");
			Assert.AreEqual(1, server.ReceivedEmailCount, "server.ReceivedEmailSize");

			SmtpMessage email =  server.ReceivedEmail.First();

			Assert.AreEqual("<everybody@bar.com>", email.Headers["To"]);
			Assert.AreEqual("<somebody@foo.com>", email.Headers["From"]);
			
			Assert.AreEqual("CRTest", email.Headers["Subject"]);
			Assert.AreEqual("text/plain;", email.Headers["Content-Type"]);

			Assert.AreEqual(bodyWithCR, email.Body,  "Body with CR");

			server.ClearReceivedEmail();

			String bodyWithCRLF = "\r\n\r\nKeep these pesky carriage returns\r\n\r\nPlease,\r\nPlease\r\n\r\n";

			SmtpMessage.CR = "\r\n";
			System.Web.Mail.SmtpMail.Send("somebody@foo.com", "everybody@bar.com", "CRTest", bodyWithCRLF);

			Assert.AreEqual(1, server.ReceivedEmail.Count(), "server.ReceivedEmail.Length");
			Assert.AreEqual(1, server.ReceivedEmailCount, "server.ReceivedEmailSize");

			email = server.ReceivedEmail.First();

			Assert.AreEqual("<everybody@bar.com>", email.Headers["To"]);
			Assert.AreEqual("<somebody@foo.com>", email.Headers["From"]);
			
			Assert.AreEqual("CRTest", email.Headers["Subject"]);
			Assert.AreEqual("text/plain;", email.Headers["Content-Type"]);

			Assert.AreEqual(bodyWithCRLF, email.Body, "Body with CRLF");

		}

		[Test]
		public void SendTwoMessages()
		{
			System.Web.Mail.SmtpMail.SmtpServer = "localhost";
			System.Web.Mail.SmtpMail.Send("somebody@foo.com", "everybody@bar.com", "This is the subject", "This is the body.");
			System.Web.Mail.SmtpMail.Send("somebody@foo.com", "nobody@bar.com", "This is the second subject", "This is the second body.");

            Assert.AreEqual(2, server.ReceivedEmail.Count(), "server.ReceivedEmail.Length");
			Assert.AreEqual(2, server.ReceivedEmailCount, "server.ReceivedEmailSize");
			
			SmtpMessage email1 =  server.ReceivedEmail.First();
			
			Assert.AreEqual("<everybody@bar.com>", email1.Headers["To"]);
			Assert.AreEqual("<somebody@foo.com>", email1.Headers["From"]);
			
			Assert.AreEqual("text/plain;", email1.Headers["Content-Type"]);

			Assert.AreEqual("This is the subject", email1.Headers["Subject"]);
			Assert.AreEqual("This is the body.", email1.Body);

			SmtpMessage email2 =  server.ReceivedEmail.Skip(1).First();
			
			Assert.AreEqual("<nobody@bar.com>", email2.Headers["To"]);
			Assert.AreEqual("<somebody@foo.com>", email2.Headers["From"]);
			
			Assert.AreEqual("text/plain;", email2.Headers["Content-Type"]);

			Assert.AreEqual("This is the second subject", email2.Headers["Subject"]);
			Assert.AreEqual("This is the second body.", email2.Body);
		}

		[Test]
		public void StopStartServer()
		{
			System.Web.Mail.SmtpMail.SmtpServer = "localhost";
			System.Web.Mail.SmtpMail.Send("somebody@foo.com", "everybody@bar.com", "This is the subject", "This is the body.");

            Assert.AreEqual(1, server.ReceivedEmail.Count(), "server.ReceivedEmail.Length");
			Assert.AreEqual(1, server.ReceivedEmailCount, "server.ReceivedEmailSize");
			
			SmtpMessage email1 =  server.ReceivedEmail.First();
			
			Assert.AreEqual("<everybody@bar.com>", email1.Headers["To"]);
			Assert.AreEqual("<somebody@foo.com>", email1.Headers["From"]);
			
			Assert.AreEqual("text/plain;", email1.Headers["Content-Type"]);

			Assert.AreEqual("This is the subject", email1.Headers["Subject"]);
			Assert.AreEqual("This is the body.", email1.Body);


			server.Stop();
			server = SimpleSmtpServer.Start();

			System.Web.Mail.SmtpMail.Send("somebody@foo.com", "nobody@bar.com", "This is the second subject", "This is the second body.");

            Assert.AreEqual(1, server.ReceivedEmail.Count(), "server.ReceivedEmail.Length");
			Assert.AreEqual(1, server.ReceivedEmailCount, "server.ReceivedEmailSize");

			SmtpMessage email2 =  server.ReceivedEmail.First();
			
			Assert.AreEqual("<nobody@bar.com>", email2.Headers["To"]);
			Assert.AreEqual("<somebody@foo.com>", email2.Headers["From"]);
			
			Assert.AreEqual("text/plain;", email2.Headers["Content-Type"]);

			Assert.AreEqual("This is the second subject", email2.Headers["Subject"]);
			Assert.AreEqual("This is the second body.", email2.Body);
		}

		[Test]
		[ExpectedException(typeof( System.Net.Sockets.SocketException))]
		public void ServerBindingError()
		{
			// Server is already running. We check that this cause an SocketException to be thrown
			SimpleSmtpServer server2 = SimpleSmtpServer.Start();

			Assert.Fail("BindingError");
		}

#if NET_1_0
		// In .Net 1.0 we can't send mail to alternate port because MailMessage.Fields doesn't exists
		[Test]
		public void MultipleServerPortSimple()
		{
			int ALT_PORT = 2525;

			// Start second server
			server2 = SimpleSmtpServer.Start(ALT_PORT);

			// Send to first server
			System.Web.Mail.SmtpMail.SmtpServer = "localhost";
			System.Web.Mail.SmtpMail.Send("somebody@foo.com", "everybody@bar.com", "This is the subject", "This is the body.");

			// Check first server
			Assert.AreEqual(1, server.ReceivedEmail.Length, "server.ReceivedEmail.Length");
			Assert.AreEqual(1, server.ReceivedEmailCount, "server.ReceivedEmailSize");
			
			SmtpMessage email =  server.ReceivedEmail.First();
			
			Assert.AreEqual("<everybody@bar.com>", email.Headers["To"]);
			Assert.AreEqual("<somebody@foo.com>", email.Headers["From"]);
			
			Assert.AreEqual("text/plain;", email.Headers["Content-Type"]);

			Assert.AreEqual("This is the subject", email.Headers["Subject"]);
			Assert.AreEqual("This is the body.", email.Body);

			// Check second server
			Assert.AreEqual(0, server2.ReceivedEmail.Length, "server.ReceivedEmail.Length");
			Assert.AreEqual(0, server2.ReceivedEmailCount, "server.ReceivedEmailSize");
		}
#else
		[Test]
		public void MultipleServerPort()
		{
			int ALT_PORT = 2525;

			// Start second server
			server2 = SimpleSmtpServer.Start(ALT_PORT);

			// Send to first server
			System.Web.Mail.SmtpMail.SmtpServer = "localhost";
			System.Web.Mail.SmtpMail.Send("somebody@foo.com", "everybody@bar.com", "This is the subject", "This is the body.");


			// Send to second server
			MailMessage mail = new MailMessage(); 
			mail.To = "nobody@bar.com"; 
			mail.From = "somebody@foo.com"; 
			mail.Subject = "This is the second subject"; 
			mail.Body = "This is the second body."; 
			mail.Fields.Add("http://schemas.microsoft.com/cdo/configuration/smtpserverport", ALT_PORT.ToString()); 
			SmtpMail.Send(mail);

			// Check first server
            Assert.AreEqual(1, server.ReceivedEmail.Count(), "server.ReceivedEmail.Length");
			Assert.AreEqual(1, server.ReceivedEmailCount, "server.ReceivedEmailSize");
			
			SmtpMessage email =  server.ReceivedEmail.First();
			
			Assert.AreEqual("<everybody@bar.com>", email.Headers["To"]);
			Assert.AreEqual("<somebody@foo.com>", email.Headers["From"]);
			
			Assert.AreEqual("text/plain;", email.Headers["Content-Type"]);

			Assert.AreEqual("This is the subject", email.Headers["Subject"]);
			Assert.AreEqual("This is the body.", email.Body);

			// Check second server
            Assert.AreEqual(1, server2.ReceivedEmail.Count(), "server.ReceivedEmail.Length");
			Assert.AreEqual(1, server2.ReceivedEmailCount, "server.ReceivedEmailSize");

			SmtpMessage email1 =  server2.ReceivedEmail.First();
			
			Assert.AreEqual("<nobody@bar.com>", email1.Headers["To"]);
			Assert.AreEqual("<somebody@foo.com>", email1.Headers["From"]);
			
			Assert.AreEqual("text/plain;", email1.Headers["Content-Type"]);

			Assert.AreEqual("This is the second subject", email1.Headers["Subject"]);
			Assert.AreEqual("This is the second body.", email1.Body);

		}
#endif
	}
}


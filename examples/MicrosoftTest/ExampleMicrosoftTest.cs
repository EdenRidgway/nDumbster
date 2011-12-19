using System;
using System.Web;
using Microsoft.VisualStudio.QualityTools.UnitTesting.Framework;

// Reference the nDumbster namespace.
using nDumbster.smtp;

namespace examples
{
	/// <summary>
	///   An example of calling nDumbster from within a Microsoft Unit Test.
	/// </summary>
	[TestClass]
	public class ExampleMicrosoftTest
	{
		/// <summary>
		///   The starting and stopping of the smtp server can be done in the tearup and teardown,
		///   leaving you free to test your code.
		/// </summary>
		private SimpleSmtpServer smtpServer = null;

		#region Test Setup and Tear Down

		/// <summary>
		/// Initialize() is called at test time by the EDT
		/// test harness before each test is run
		/// We initialize the SmtpServer here.
		/// </summary>
		[TestInitialize()]
		public void Initialize()
		{
			// Initialise smtpServer with a running one.  
			// Note that this is running on localhost port 25 by default.
			smtpServer = SimpleSmtpServer.Start();
		}

		/// <summary>
		/// Cleanup() is called at test time by the EDT
		/// test harness after each test is run, unless the
		/// corresponding Initialize() call threw an exception.
		/// We stop the server at the end of each test.
		/// </summary>
		[TestCleanup()]
		public void Cleanup()
		{
			if (smtpServer != null)	smtpServer.Stop();
		}

		#endregion // test setup and teardown.

		[TestMethod]
		public void CanRecieveSingleMail()
		{

			// Use SmtpMail to send an SMTP message to localhost (port 25)
			// (in the real world you would be testing your own code that talks to an SMTP server).
			System.Web.Mail.SmtpMail.SmtpServer = "localhost";
			System.Web.Mail.SmtpMail.Send("somebody@foo.com", "everybody@bar.com", "This is the subject", "This is the body.");

			// Check that the mail has been received.
			Assert.AreEqual(1, smtpServer.ReceivedEmail.Count);
		}

	}
}


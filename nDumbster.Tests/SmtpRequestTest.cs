using NUnit.Framework;

namespace nDumbster.Tests
{
	/// <summary>
	/// Summary description for SmtpRequestTest.
	/// </summary>
	[TestFixture] 
	public class SmtpRequestTest
	{
		PrivateObjectTester smtpAction;
		PrivateObjectTester smtpState;


		[TestFixtureSetUp]
		public void CollectTypeInfo()
		{
			smtpAction = new PrivateObjectTester("nDumbster.smtp.SmtpActionType,nDumbster", (sbyte)0);
			smtpState = new PrivateObjectTester("nDumbster.smtp.SmtpState,nDumbster", (sbyte)0);
		}

		[Test]
		public void UnrecognizedCommandConnectState() 
		{
			PrivateObjectTester request = new PrivateObjectTester("nDumbster.smtp.SmtpRequest,nDumbster",smtpAction.GetField("UNRECOG"), "", smtpState.GetField("CONNECT"));
			PrivateObjectTester response = new PrivateObjectTester(request.Invoke("Execute"));
			Assert.AreEqual(500,  response.GetProperty("Code"));
		}

		[Test]
		public void UnrecognizedCommandGreetState() 
		{
			PrivateObjectTester request = new PrivateObjectTester("nDumbster.smtp.SmtpRequest,nDumbster",smtpAction.GetField("UNRECOG"), "", smtpState.GetField("GREET"));
			PrivateObjectTester response = new PrivateObjectTester(request.Invoke("Execute"));
			Assert.AreEqual(500,  response.GetProperty("Code"));
		}

		[Test]
		public void UnrecognizedCommandMailState() 
		{
			PrivateObjectTester request = new PrivateObjectTester("nDumbster.smtp.SmtpRequest,nDumbster",smtpAction.GetField("UNRECOG"), "", smtpState.GetField("MAIL"));
			PrivateObjectTester response = new PrivateObjectTester(request.Invoke("Execute"));
			Assert.AreEqual(500,  response.GetProperty("Code"));
		}

		[Test]
		public void UnrecognizedCommandQuitState() 
		{
			PrivateObjectTester request = new PrivateObjectTester("nDumbster.smtp.SmtpRequest,nDumbster",smtpAction.GetField("UNRECOG"), "", smtpState.GetField("QUIT"));
			PrivateObjectTester response = new PrivateObjectTester(request.Invoke("Execute"));
			Assert.AreEqual(500,  response.GetProperty("Code"));
		}

		[Test]
		public void UnrecognizedCommandRcptState() 
		{
			PrivateObjectTester request = new PrivateObjectTester("nDumbster.smtp.SmtpRequest,nDumbster",smtpAction.GetField("UNRECOG"), "", smtpState.GetField("RCPT"));
			PrivateObjectTester response = new PrivateObjectTester(request.Invoke("Execute"));
			Assert.AreEqual(500,  response.GetProperty("Code"));
		}

		[Test]
		public void UnrecognizedCommandDataBodyState() 
		{
			PrivateObjectTester request = new PrivateObjectTester("nDumbster.smtp.SmtpRequest,nDumbster",smtpAction.GetField("UNRECOG"), "", smtpState.GetField("DATA_BODY"));
			PrivateObjectTester response = new PrivateObjectTester(request.Invoke("Execute"));
			Assert.AreEqual(-1,  response.GetProperty("Code"));
		}

		[Test]
		public void UnrecognizedCommandDataHdrState() 
		{
			PrivateObjectTester request = new PrivateObjectTester("nDumbster.smtp.SmtpRequest,nDumbster",smtpAction.GetField("UNRECOG"), "", smtpState.GetField("DATA_HDR"));
			PrivateObjectTester response = new PrivateObjectTester(request.Invoke("Execute"));
			Assert.AreEqual(-1,  response.GetProperty("Code"));
		}
	}
}

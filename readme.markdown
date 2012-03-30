Introduction
===============================================================================
This is a fork of nDumbster, the fake SMTP server, that was originally hosted at: http://ndumbster.sourceforge.net/default.html. nDumbster is useful in automated tests where instead of having to create a wrapper around the mail sending then providing a mock
implementation you actually test the email sending component. The SMTP server keeps the messages in memory so you can query what has been sent.

This version is a signficant improvement over the old version as it now uses OpenPop to parse the mail messages sent to the server. This allows 
you to check the various MIME parts and attachments. It also provides a much richer way of checking the entire mail message.

Enhancements
-------------------------------------------------------------------------------
Apart from being upgraded to .Net 4.0 the following improvements have been made to the library:

    1. The locking issues that lead to slow test times have been removed by using the ConcurrentQueue
	2. Fixed the race condition issue in the code (sending back an SMTP finished before saving the message) 
	   that meant that Assertions made after an SMTP send could fail from time to time.
    3. Generics have been used instead of ArrayLists
	4. Messags are now returned as OpenPop messages which allow you to query the various messages 
	   parts far more easily, such as:
		* Headers are strongly typed
		* Text versus HTML versions can be extracted and compared
		* Attachment handling actually exists
		* Media Types
		* Lists of TO/CC/BCC email addresses
	
Example test:

```c#
SmtpMail.SmtpServer = "localhost";
SmtpMail.Send("somebody@foo.com", "everybody@bar.com", "This is the subject", "This is the body.");

Assert.AreEqual(1, server.ReceivedEmail.Count(), "server.ReceivedEmail.Length");

var email = server.ReceivedEmail.First();

Assert.AreEqual("everybody@bar.com", email.Headers.To.First().Address);
Assert.AreEqual("somebody@foo.com", email.Headers.From.Address);

Assert.AreEqual("text/plain", email.Headers.ContentType.MediaType);

Assert.AreEqual("This is the subject", email.Headers.Subject);
Assert.AreEqual("This is the body.", email.FindFirstPlainTextVersion().GetBodyAsText());
```
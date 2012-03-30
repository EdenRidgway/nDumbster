using System;
using System.Threading;
using OpenPop.Mime;
using nDumbster.smtp;

namespace mailspy
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	class Class1
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			SimpleSmtpServer server= SimpleSmtpServer.Start();
			while(true)
			{
				if (server.ReceivedEmailCount > 0)
				{
					foreach(Message message in server.ReceivedEmail)
					{
						Console.WriteLine("----------------------------------------------------------------");
						Console.WriteLine();
						Console.WriteLine(message.ToString());
					}
					server.ClearReceivedEmail();
				}
				Thread.Sleep(100);
			}
		}
	}
}

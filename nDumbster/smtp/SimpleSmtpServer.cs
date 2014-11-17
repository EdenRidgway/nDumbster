#region Using directives

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using OpenPop.Mime;

#endregion
#region copyright
/*
 * nDumbster - a dummy SMTP server
 * Copyright 2005 Martin Woodward
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
#endregion // copyright

namespace nDumbster.Smtp
{
	/// <summary>
	/// Dummy SMTP server for testing purposes.
	/// </summary>
	/// <example>
	/// The following examples shows how to use nDumbster and <see href="http://www.nunit.org">NUnit</see> to test your function sendMessage.
	/// <code>
	///	[TestFixture]
	///	public class SimpleSmtpServerTest
	///	{
	///		SimpleSmtpServer smtpServer;
	///
	///		[SetUp]
	///		public void Setup()
	///		{
	///
	///			smtpServer = SimpleSmtpServer.Start();
	///
	///		}
	///
	///		[TearDown]
	///		public void TearDown()
	///		{
	///			smtpServer.Stop();
	///		}
	///
	///		[Test]
	///		public void SendEmail()
	///		{
	///			/// Use you own code 
	///			sendMessage(25, "sender@here.com", "Test", "Test Body", "receiver@there.com");
	///			Assert.AreEqual(1, smtpServer.ReceivedEmailCount, "1 mails sent");
	///			SmtpMessage mail= (SmtpMessage)smtpServer.ReceivedEmail[0];
	///			Assert.AreEqual("&lt;receiver@there.com&gt;", mail.Headers["To"], "Receiver");
	///			Assert.AreEqual("&lt;sender@here.com&gt;", mail.Headers["From"], "Sender");
	///			Assert.AreEqual("Test", mail.Headers["Subject"], "Subject");
	///
	///			Assert.AreEqual("Test Body", mailUser.Body, "Body");
	///		}
	///	}
	///	</code>
	/// </example>
	/// <threadsafety static="true" instance="true">
	///	<para>The server create a thread to handle message reception and all access to messages list is protected.</para>
	/// </threadsafety>
	public class SimpleSmtpServer:IDisposable
	{
		#region Members
		/// <summary>
		/// Default SMTP port is 25.
		/// </summary>
		public const int DEFAULT_SMTP_PORT = 25;

		/// <summary>
		/// Stores the port that the SmtpServer listens to
		/// </summary>
		private int port = DEFAULT_SMTP_PORT;
		
		/// <summary>
		/// Stores all of the email received since this instance started up.
		/// </summary>
        private ConcurrentQueue<MailMessage> _receivedMail = new ConcurrentQueue<MailMessage>();
		
		/// <summary>
		/// Indicates whether this server is stopped or not.
		/// </summary>
		private volatile bool _stopped = true;

		/// <summary>
		/// Listen for client connection
		/// </summary>
		protected TcpListener TcpListener = null;

		/// <summary>
		/// Synchronization <see cref="AutoResetEvent">event</see> : Set when server has started (successfully or not)
		/// </summary>
		internal AutoResetEvent StartedEvent = null;

		/// <summary>
		/// Last <see cref="Exception">Exception</see> that happened in main loop thread
		/// </summary>
		internal Exception MainThreadException = null;

	    internal event EventHandler<EventArgs> HasStopped; 

		#endregion // Members

		#region Contructors
		/// <summary>
		/// Constructor.
		/// </summary>
		private SimpleSmtpServer(int port)
		{
			this.port = port;
			this.StartedEvent = new AutoResetEvent(false);
		}
		#endregion // Constructors;

		#region Properties
		/// <summary>
		/// Indicates whether this server is stopped or not.
		/// </summary>
		/// <value><see langword="true"/> if the server is stopped</value>
		virtual public bool Stopped
		{
			get
			{
				return _stopped;
			}
		}
		
		/// <summary>
		/// The port that the SmtpServer listens to
		/// </summary>
		/// <value>Port used to accept client connections</value>
		public int Port
		{
			get
			{
				return this.port;
			}
		}

		/// <summary>
		/// List of email received by this instance since start up.
		/// </summary>
		/// <value><see cref="Array">Array</see> holding received <see cref="SmtpMessage">SmtpMessage</see></value>
        virtual public IEnumerable<MailMessage> ReceivedEmail
		{
			get
			{
                // This will return a snapshot of the contents of the queue
                return _receivedMail.ToList();
			}
		}

		/// <summary>
		/// Erase list of received emails
		/// </summary>
		virtual public void ClearReceivedEmail()
		{
		    MailMessage mailMessage;
            while (_receivedMail.TryDequeue(out mailMessage)) {}
		}

		/// <summary>
		/// Number of messages received by this instance since start up.
		/// </summary>
		/// <value>Number of messages</value>
		virtual public int ReceivedEmailCount
		{
			get
			{
				return _receivedMail.Count;
			}

		}
		#endregion  // Properties

		/// <summary>
		/// Main loop of the SMTP server.
		/// </summary>
		internal void Run()
		{
			_stopped = false;

			try
			{
				try
				{
					// Open a listener to accept client connection
					TcpListener = new TcpListener(IPAddress.Any, Port);
					TcpListener.Start();
				}
				catch(Exception e)
				{
					// If we can't start the listener, we don't start loop
					_stopped = true;
					// And store exception that will be thrown back to the thread
					// that started the server
					MainThreadException = e;
				}
				finally
				{
					// Inform calling thread that we can noew receive messages
					// or that something bad happened.
					StartedEvent.Set();
				}

				// Server: loop until stopped
				while (!Stopped)
				{
					Socket socket = null;
					try
					{
						// Accept an incomming client connection
						socket = TcpListener.AcceptSocket();
					}
					catch
					{
						if (socket != null)
						{
							socket.Close();
						}
						continue; // Non-blocking socket timeout occurred: try accept() again
					}

					// Get the input and output streams
					NetworkStream networkStream = new NetworkStream(socket);
					StreamReader input = new StreamReader(networkStream);
					StreamWriter output = new StreamWriter(networkStream);

					// Fetch all incomming messages from client, and add them to the queue
                    HandleSmtpTransaction(output, input);
					
					// Close client connection, and wait for another one
					socket.Close();
				}
			}
			catch (Exception e)
			{
			    // Send exception back to calling thread
			    var socketException = e as SocketException;
                if (!(socketException != null && !socketException.Message.Contains("WSACancelBlockingCall")))
                {
                    MainThreadException = e;
                }
			}
			finally
			{
				// The server won't listen anymore
				_stopped = true;

				// Stop the listener if it was started
				if (TcpListener != null)
				{
					TcpListener.Stop();
					TcpListener = null;
				}

                if (HasStopped != null)
                {
                    HasStopped(this, new EventArgs());
                }
			}
		}

	    /// <summary>
	    /// Handle an SMTP transaction, i.e. all activity between initial connect and QUIT command.
	    /// </summary>
	    /// <param name="output">output stream</param>
	    /// <param name="input">input stream</param>
	    /// <param name="messageQueue">The message queue to add any messages to</param>
	    /// <returns>List of received SmtpMessages</returns>
        private void HandleSmtpTransaction(StreamWriter output, TextReader input)
		{
			// Initialize the state machine
			SmtpState smtpState = SmtpState.CONNECT;
			SmtpRequest smtpRequest = new SmtpRequest(SmtpActionType.CONNECT, String.Empty, smtpState);

			// Execute the connection request
			SmtpResponse smtpResponse = smtpRequest.Execute();

			// Send initial response
			SendResponse(output, smtpResponse);
			smtpState = smtpResponse.NextState;

			SmtpMessage msg = new SmtpMessage();
            
			while (smtpState != SmtpState.CONNECT)
			{
				string line = input.ReadLine();

				if (line == null)
				{
					break;
				}

				// Create request from client input and current state
				SmtpRequest request = SmtpRequest.CreateRequest(line, smtpState);
				// Execute request and create response object
				SmtpResponse response = request.Execute();
				// Move to next internal state
				smtpState = response.NextState;
				// Store input in message
				msg.Store(response, request.Params);

				// If message reception is complete save it
				if (smtpState == SmtpState.QUIT)
				{
                    // Remove the last carriage return and new line
                    string mimeMessage = msg.RawMessage;
                    byte[] messageBytes = Encoding.ASCII.GetBytes(mimeMessage);
                    Message message = new Message(messageBytes, true);

                    _receivedMail.Enqueue(message.ToMailMessage());

					msg = new SmtpMessage();
                }

                // Send reponse to client after we have stored the email, so when asking for the recived mail list it will be there 
                // (this was not the way it was done before)
                SendResponse(output, response);

			}
		}

		/// <summary>
		/// Send response to client.
		/// </summary>
		/// <param name="output">socket output stream</param>
		/// <param name="smtpResponse">Response to send</param>
		private void SendResponse(StreamWriter output, SmtpResponse smtpResponse)
		{
		    if (smtpResponse.Code <= 0) return;

		    output.WriteLine(smtpResponse.Code + " " + smtpResponse.Message);
		    output.Flush();
		}

		/// <summary>
		/// Forces the server to stop after processing the current request.
		/// </summary>
		public virtual void Stop()
		{
			ClearReceivedEmail();

            AutoResetEvent waitForStoppResetEvent = new AutoResetEvent(false);
			HasStopped += (sender, args) => waitForStoppResetEvent.Set();

            _stopped = true;

            TcpListener.Stop();
			waitForStoppResetEvent.WaitOne(TimeSpan.FromSeconds(5));
	    }

		/// <overloads>
		/// Creates an instance of SimpleSmtpServer and starts it.
		///	</overloads>
		/// <summary>
		/// Creates and starts an instance of SimpleSmtpServer that will listen on the default port.
		/// </summary>
		/// <returns>The <see cref="SimpleSmtpServer">SmtpServer</see> waiting for message</returns>
		public static SimpleSmtpServer Start()
		{
			return Start(DEFAULT_SMTP_PORT);
		}

		/// <summary>
		/// Creates and starts an instance of SimpleSmtpServer that will listen on a specific port.
		/// </summary>
		/// <param name="port">port number the server should listen to</param>
        /// <param name="background">Whether listener thread blocks process from shutdown</param>
		/// <returns>The <see cref="SimpleSmtpServer">SmtpServer</see> waiting for message</returns>
		public static SimpleSmtpServer Start(int port,bool background = false)
		{
			SimpleSmtpServer server = new SimpleSmtpServer(port);

			Thread smtpServerThread = new Thread(server.Run);
            smtpServerThread.IsBackground = background;
			smtpServerThread.Start();
            
			// Block until the server socket is created
			try 
			{
				server.StartedEvent.WaitOne();
			} 
			catch 
			{
				// Ignore don't care.
			}

			// If an exception occured during server startup, send it back.
			if (server.MainThreadException != null)
            {
                throw server.MainThreadException;
			}

			return server;
		}


        public void Dispose()
        {
            Stop();
        }
    }
}

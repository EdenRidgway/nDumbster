#region Using directives

using System;
using System.Collections.Specialized;
using System.Text;

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

namespace nDumbster.smtp
{	
	/// <summary>
	/// Container for a complete SMTP message - headers and message body.
	/// </summary>
	public class SmtpMessage
	{	
		/// <summary>
		/// Message body.
		/// </summary>
		virtual public string Body
		{
			get
			{
				return body.ToString();
			}
		}

		/// <summary>
		/// Number of lines in message body.
		/// </summary>
		public int BodyLineCount
		{
			get
			{
				return bodyLineCount;
			}
		}

		/// <summary>
		/// Headers: Collection of named values.
		/// </summary>
		private NameValueCollection headers; 

		/// <summary>
		/// Message body.
		/// </summary>
		private StringBuilder body;

		/// <summary>
		/// Number of lines in body
		/// </summary>
		private int bodyLineCount;

		/// <summary>
		/// Default carriage return code is CRLF.
		/// </summary>
		public const string DEFAULT_CRLF = "\r\n";

		/// <summary>
		/// Code used for carriage return in the body.
		/// </summary>
		public static string CR = DEFAULT_CRLF;

		/// <summary>
		/// Constructor. Initializes headers collection and body buffer.
		/// </summary>
		public SmtpMessage()
		{
			headers = new NameValueCollection();
			body = new StringBuilder();
			bodyLineCount = 0;
		}

		/// <summary> 
		/// Update the headers or body depending on the SmtpResponse object and line of input.
		/// </summary>
		/// <param name="response">SmtpResponse object</param>
		/// <param name="commandData">remainder of input line after SMTP command has been removed</param>
		internal void Store(SmtpResponse response, string commandData)
		{
			if (commandData != null)
			{
				if (SmtpState.DATA_HDR == response.NextState)
				{
					int headerNameEnd = commandData.IndexOf(":");
					if (headerNameEnd >= 0)
					{
						string name = commandData.Substring(0, (headerNameEnd) - (0)).Trim();
						string value_Renamed = commandData.Substring(headerNameEnd + 1).Trim();
						// We use the Add method instead of [] because we can have multiple values for a name
						headers.Add(name, value_Renamed);
					}
				}
				else if (SmtpState.DATA_BODY == response.NextState)
				{
					if (bodyLineCount > 0)
						body.Append(CR);
					body.Append(commandData);
					bodyLineCount++;
				}
			}
		}

		/// <summary>
		/// Headers of the message.
		/// </summary>
		public NameValueCollection Headers
		{
			get
			{
				return headers;
			}
		}
		
		/// <summary>
		/// String representation of the SmtpMessage.
		/// </summary>
		/// <returns>A String that displays the current SmtpMessage headers and body</returns>
		public override string ToString()
		{
			StringBuilder msg = new StringBuilder();
			foreach (string name in headers.AllKeys)
			{
				foreach(string val in headers.GetValues(name))
				{
					msg.Append(String.Format("{0}: {1}\n",name,val));					
				}
			}
			msg.Append('\n');
			msg.Append(body);
			msg.Append('\n');
			return msg.ToString();
		}
	}
}

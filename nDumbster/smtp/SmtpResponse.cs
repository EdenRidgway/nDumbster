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
	/// SMTP response container.
	/// </summary>
	internal class SmtpResponse
	{

		#region Members
		/// <summary>
		/// Response code - see RFC-2821. 
		/// </summary>
		private int code;
		/// <summary>
		/// Response message. 
		/// </summary>
		private string message;
		/// <summary>
		/// New state of the SMTP server once the request has been executed. 
		/// </summary>
		private SmtpState nextState;

		#endregion // Members

		#region Constructor
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="code">response code</param>
		/// <param name="message">response message</param>
		/// <param name="next">next state of the SMTP server</param>
		public SmtpResponse(int code, string message, SmtpState next)
		{
			this.code = code;
			this.message = message;
			this.nextState = next;
		}
		#endregion // Constructor

		#region Properties
		/// <summary> 
		/// Response code.
		/// </summary>
		virtual public int Code
		{
			get
			{
				return code;
			}

		}
		/// <summary> 
		/// Response message.
		/// </summary>
		virtual public string Message
		{
			get
			{
				return message;
			}

		}

		/// <summary>
		/// Next SMTP server state.
		/// </summary>
		virtual public SmtpState NextState
		{
			get
			{
				return nextState;
			}

		}
		#endregion // Properties
	}
}

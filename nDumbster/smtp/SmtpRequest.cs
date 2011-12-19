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
	/// Contains an SMTP client request and handles state transitions.
	/// </summary>
	/// <remarks>
	/// State transitions are handled using the following state transition table.
	/// <code>
	/// -----------+-------------------------------------------------------------------------------------------------
	/// |                                 State
	/// Action     +-------------+-----------+-----------+--------------+---------------+---------------+------------
	///			   | CONNECT     | GREET     | MAIL      | RCPT         | DATA_HDR      | DATA_BODY     | QUIT
	/// -----------+-------------+-----------+-----------+--------------+---------------+---------------+------------
	/// connect    | 220/GREET   | 503/GREET | 503/MAIL  | 503/RCPT     | 503/DATA_HDR  | 503/DATA_BODY | 503/QUIT
	/// ehlo       | 503/CONNECT | 250/MAIL  | 503/MAIL  | 503/RCPT     | 503/DATA_HDR  | 503/DATA_BODY | 503/QUIT
	/// mail       | 503/CONNECT | 503/GREET | 250/RCPT  | 503/RCPT     | 503/DATA_HDR  | 503/DATA_BODY | 250/RCPT
	/// rcpt       | 503/CONNECT | 503/GREET | 503/MAIL  | 250/RCPT     | 503/DATA_HDR  | 503/DATA_BODY | 503/QUIT
	/// data       | 503/CONNECT | 503/GREET | 503/MAIL  | 354/DATA_HDR | 503/DATA_HDR  | 503/DATA_BODY | 503/QUIT
	/// data_end   | 503/CONNECT | 503/GREET | 503/MAIL  | 503/RCPT     | 250/QUIT      | 250/QUIT      | 503/QUIT
	/// unrecog    | 500/CONNECT | 500/GREET | 500/MAIL  | 500/RCPT     | ---/DATA_HDR  | ---/DATA_BODY | 500/QUIT
	/// quit       | 503/CONNECT | 503/GREET | 503/MAIL  | 503/RCPT     | 503/DATA_HDR  | 503/DATA_BODY | 250/CONNECT
	/// blank_line | 503/CONNECT | 503/GREET | 503/MAIL  | 503/RCPT     | ---/DATA_BODY | ---/DATA_BODY | 503/QUIT
	/// rset       | 250/GREET   | 250/GREET | 250/GREET | 250/GREET    | 250/GREET     | 250/GREET     | 250/GREET
	/// vrfy       | 252/CONNECT | 252/GREET | 252/MAIL  | 252/RCPT     | 252/DATA_HDR  | 252/DATA_BODY | 252/QUIT
	/// expn       | 252/CONNECT | 252/GREET | 252/MAIL  | 252/RCPT     | 252/DATA_HDR  | 252/DATA_BODY | 252/QUIT
	/// help       | 211/CONNECT | 211/GREET | 211/MAIL  | 211/RCPT     | 211/DATA_HDR  | 211/DATA_BODY | 211/QUIT
	/// noop       | 250/CONNECT | 250/GREET | 250/MAIL  | 250/RCPT     | 250|DATA_HDR  | 250/DATA_BODY | 250/QUIT
	/// </code>
	/// </remarks>
	internal class SmtpRequest
	{
		/// <summary>
		/// Parameters of this request (remainder of command line once the command is removed.
		/// </summary>
		virtual public string Params
		{
			get
			{
				return request_Params;
			}

		}

		/// <summary>
		/// SMTP action received from client. 
		/// </summary>
		private SmtpActionType action;
		/// <summary>
		/// Current state of the SMTP state table. 
		/// </summary>
		private SmtpState state;
		/// <summary>
		/// Additional information passed from the client with the SMTP action. 
		/// </summary>
		private string request_Params;

		/// <summary> 
		/// Create a new SMTP client request.
		/// </summary>
		/// <param name="actionType">type of action/command</param>
		/// <param name="request_Params">remainder of command line once command is removed</param>
		/// <param name="state">current SMTP server state</param>
		public SmtpRequest(SmtpActionType actionType, string request_Params, SmtpState state)
		{
			this.action = actionType;
			this.state = state;
			this.request_Params = request_Params;
		}

		/// <summary> 
		/// Execute the SMTP request returning a response. This method models the state transition table for the SMTP server.
		/// </summary>
		/// <returns>Reponse to the request</returns>
		public virtual SmtpResponse Execute()
		{
			SmtpResponse response = null;
			if (action.Stateless)
			{
				if (SmtpActionType.EXPN == action || SmtpActionType.VRFY == action)
				{
					response = new SmtpResponse(252, "Not supported", this.state);
				}
				else if (SmtpActionType.HELP == action)
				{
					response = new SmtpResponse(211, "No help available", this.state);
				}
				else if (SmtpActionType.NOOP == action)
				{
					response = new SmtpResponse(250, "OK", this.state);
				}
				else if (SmtpActionType.VRFY == action)
				{
					response = new SmtpResponse(252, "Not supported", this.state);
				}
				else if (SmtpActionType.RSET == action)
				{
					response = new SmtpResponse(250, "OK", SmtpState.GREET);
				}
				else
				{
					response = new SmtpResponse(500, "Command not recognized", this.state);
				}
			}
			else
			{
				// Stateful commands
				if (SmtpActionType.CONNECT == action)
				{
					if (SmtpState.CONNECT == state)
					{
						response = new SmtpResponse(220, "localhost nDumbster SMTP service ready", SmtpState.GREET);
					}
					else
					{
						response = new SmtpResponse(503, "Bad sequence of commands: " + action, this.state);
					}
				}
				else if (SmtpActionType.EHLO == action)
				{
					if (SmtpState.GREET == state)
					{
						response = new SmtpResponse(250, "OK", SmtpState.MAIL);
					}
					else
					{
						response = new SmtpResponse(503, "Bad sequence of commands: " + action, this.state);
					}
				}
				else if (SmtpActionType.MAIL == action)
				{
					if (SmtpState.MAIL == state || SmtpState.QUIT == state)
					{
						response = new SmtpResponse(250, "OK", SmtpState.RCPT);
					}
					else
					{
						response = new SmtpResponse(503, "Bad sequence of commands: " + action, this.state);
					}
				}
				else if (SmtpActionType.RCPT == action)
				{
					if (SmtpState.RCPT == state)
					{
						response = new SmtpResponse(250, "OK", this.state);
					}
					else
					{
						response = new SmtpResponse(503, "Bad sequence of commands: " + action, this.state);
					}
				}
				else if (SmtpActionType.DATA == action)
				{
					if (SmtpState.RCPT == state)
					{
						response = new SmtpResponse(354, "Start mail input; end with <CRLF>.<CRLF>", SmtpState.DATA_HDR);
					}
					else
					{
						response = new SmtpResponse(503, "Bad sequence of commands: " + action, this.state);
					}
				}
				else if (SmtpActionType.UNRECOG == action)
				{
					if (SmtpState.DATA_HDR == state || SmtpState.DATA_BODY == state)
					{
						response = new SmtpResponse(-1, "", this.state);
					}
					else
					{
						response = new SmtpResponse(500, "Command not recognized", this.state);
					}
				}
				else if (SmtpActionType.DATA_END == action)
				{
					if (SmtpState.DATA_HDR == state || SmtpState.DATA_BODY == state)
					{
						response = new SmtpResponse(250, "OK", SmtpState.QUIT);
					}
					else
					{
						response = new SmtpResponse(503, "Bad sequence of commands: " + action, this.state);
					}
				}
				else if (SmtpActionType.BLANK_LINE == action)
				{
					if (SmtpState.DATA_HDR == state)
					{
						response = new SmtpResponse(-1, "", SmtpState.DATA_BODY);
					}
					else if (SmtpState.DATA_BODY == state)
					{
						response = new SmtpResponse(-1, "", this.state);
					}
					else
					{
						response = new SmtpResponse(503, "Bad sequence of commands: " + action, this.state);
					}
				}
				else if (SmtpActionType.QUIT == action)
				{
					if (SmtpState.QUIT == state)
					{
						response = new SmtpResponse(221, "localhost nDumbster service closing transmission channel", SmtpState.CONNECT);
					}
					else
					{
						response = new SmtpResponse(503, "Bad sequence of commands: " + action, this.state);
					}
				}
				else
				{
					response = new SmtpResponse(500, "Command not recognized", this.state);
				}
			}
			return response;
		}

		/// <summary>
		///  Create an SMTP request object given a line of the input stream from the client and the current internal state.
		///  </summary>
		/// <param name="s">line of input</param>
		/// <param name="state">current state</param>
		/// <returns>A populated SmtpRequest object</returns>
		public static SmtpRequest CreateRequest(string s, SmtpState state)
		{
			SmtpActionType action = null;
			string request_Params = null;

			if (state == SmtpState.DATA_HDR)
			{
				if (s.Equals("."))
				{
					action = SmtpActionType.DATA_END;
				}
				else if (s.Length < 1)
				{
					action = SmtpActionType.BLANK_LINE;
				}
				else
				{
					action = SmtpActionType.UNRECOG;
					request_Params = s;
				}
			}
			else if (state == SmtpState.DATA_BODY)
			{
				if (s.Equals("."))
				{
					action = SmtpActionType.DATA_END;
				}
				else
				{
					action = SmtpActionType.UNRECOG;
					request_Params = s;
				}
			}
			else
			{
				string su = s.ToUpper();
				if (su.StartsWith("EHLO ") || su.StartsWith("HELO"))
				{
					action = SmtpActionType.EHLO;
					request_Params = s.Substring(5);
				}
				else if (su.StartsWith("MAIL FROM:"))
				{
					action = SmtpActionType.MAIL;
					request_Params = s.Substring(10);
				}
				else if (su.StartsWith("RCPT TO:"))
				{
					action = SmtpActionType.RCPT;
					request_Params = s.Substring(8);
				}
				else if (su.StartsWith("DATA"))
				{
					action = SmtpActionType.DATA;
				}
				else if (su.StartsWith("QUIT"))
				{
					action = SmtpActionType.QUIT;
				}
				else if (su.StartsWith("RSET"))
				{
					action = SmtpActionType.RSET;
				}
				else if (su.StartsWith("NOOP"))
				{
					action = SmtpActionType.NOOP;
				}
				else if (su.StartsWith("EXPN"))
				{
					action = SmtpActionType.EXPN;
				}
				else if (su.StartsWith("VRFY"))
				{
					action = SmtpActionType.VRFY;
				}
				else if (su.StartsWith("HELP"))
				{
					action = SmtpActionType.HELP;
				}
				else
				{
					action = SmtpActionType.UNRECOG;
				}
			}

			SmtpRequest req = new SmtpRequest(action, request_Params, state);
			return req;
		}
	}
}

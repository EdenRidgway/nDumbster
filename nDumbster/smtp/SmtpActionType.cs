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
	/// <summary> Represents an SMTP action or command.</summary>
	internal class SmtpActionType
	{
		#region Members
		/// <summary>
		/// Internal value for the action type.
		/// </summary>
		private sbyte action;

		/// <summary>
		/// Internal representation of the CONNECT action.
		///  </summary>
		private const sbyte CONNECT_BYTE = 1;
		/// <summary>
		/// Internal representation of the EHLO action. 
		/// </summary>
		private const sbyte EHLO_BYTE = 2;
		/// <summary>
		/// Internal representation of the MAIL FROM action. 
		/// </summary>
		private const sbyte MAIL_BYTE = 3;
		/// <summary>
		/// Internal representation of the RCPT action.
		///  </summary>
		private const sbyte RCPT_BYTE = 4;
		/// <summary>
		/// Internal representation of the DATA action.
		/// </summary>
		private const sbyte DATA_BYTE = 5;
		/// <summary>
		/// Internal representation of the DATA END (.) action.
		/// </summary>
		private const sbyte DATA_END_BYTE = 6;
		/// <summary>Internal representation of the QUIT action. </summary>
		private const sbyte QUIT_BYTE = 7;
		/// <summary>
		/// Internal representation of an unrecognized action: body text gets this action type.
		/// </summary>
		private const sbyte UNREC_BYTE = 8;
		/// <summary>
		/// Internal representation of the blank line action: separates headers and body text.
		/// </summary>
		private const sbyte BLANK_LINE_BYTE = 9;

		/// <summary>
		/// Internal representation of the stateless RSET action.
		/// </summary>
		private const sbyte RSET_BYTE = -1;
		/// <summary>
		/// Internal representation of the stateless VRFY action.
		/// </summary>
		private const sbyte VRFY_BYTE = -2;
		/// <summary>
		/// Internal representation of the stateless EXPN action.
		/// </summary>
		private const sbyte EXPN_BYTE = -3;
		/// <summary>
		/// Internal representation of the stateless HELP action.
		/// </summary>
		private const sbyte HELP_BYTE = -4;
		/// <summary>
		/// Internal representation of the stateless NOOP action.
		/// </summary>
		private const sbyte NOOP_BYTE = -5;

		/// <summary>
		/// CONNECT action.
		/// </summary>
		public static readonly SmtpActionType CONNECT = new SmtpActionType(CONNECT_BYTE);
		/// <summary>
		/// EHLO action.
		/// </summary>
		public static readonly SmtpActionType EHLO = new SmtpActionType(EHLO_BYTE);
		/// <summary>
		/// MAIL action.
		/// </summary>
		public static readonly SmtpActionType MAIL = new SmtpActionType(MAIL_BYTE);
		/// <summary>
		/// RCPT action.
		/// </summary>
		public static readonly SmtpActionType RCPT = new SmtpActionType(RCPT_BYTE);
		/// <summary>
		/// DATA action.
		/// </summary>
		public static readonly SmtpActionType DATA = new SmtpActionType(DATA_BYTE);
		/// <summary>
		/// "." action.
		/// </summary>
		public static readonly SmtpActionType DATA_END = new SmtpActionType(DATA_END_BYTE);
		/// <summary>
		/// Body text action.
		/// </summary>
		public static readonly SmtpActionType UNRECOG = new SmtpActionType(UNREC_BYTE);
		/// <summary>
		/// QUIT action.
		/// </summary>
		public static readonly SmtpActionType QUIT = new SmtpActionType(QUIT_BYTE);
		/// <summary>
		/// Header/body separator action.
		/// </summary>
		public static readonly SmtpActionType BLANK_LINE = new SmtpActionType(BLANK_LINE_BYTE);

		/// <summary>
		/// Stateless RSET action.
		/// </summary>
		public static readonly SmtpActionType RSET = new SmtpActionType(RSET_BYTE);
		/// <summary>
		/// Stateless VRFY action. 
		/// </summary>
		public static readonly SmtpActionType VRFY = new SmtpActionType(VRFY_BYTE);
		/// <summary>
		/// Stateless EXPN action. 
		/// </summary>
		public static readonly SmtpActionType EXPN = new SmtpActionType(EXPN_BYTE);
		/// <summary>
		/// Stateless HELP action. 
		/// </summary>
		public static readonly SmtpActionType HELP = new SmtpActionType(HELP_BYTE);
		/// <summary>
		/// Stateless NOOP action. 
		/// </summary>
		public static readonly SmtpActionType NOOP = new SmtpActionType(NOOP_BYTE);

		#endregion // Members

		#region Contructors
		/// <summary>
		/// Create a new SMTP action type. Private to ensure no invalid values.
		/// </summary>
		/// <param name="action">one of the _BYTE values</param>
		private SmtpActionType(sbyte action)
		{
			this.action = action;
		}
		#endregion // Constructors

		#region Properties
		/// <summary>
		/// Indicates whether the action is stateless or not.
		/// </summary>
		virtual public bool Stateless
		{
			get
			{
				return action < 0;
			}

		}
		#endregion // Properties

		/// <summary> 
		/// String representation of this SMTP action type.
		/// </summary>
		/// <returns>A String that represents the current SmtpActionType</returns>
		/// 
		public override string ToString()
		{
			switch (action)
			{

				case CONNECT_BYTE:
					return "Connect";

				case EHLO_BYTE:
					return "EHLO";

				case MAIL_BYTE:
					return "MAIL";

				case RCPT_BYTE:
					return "RCPT";

				case DATA_BYTE:
					return "DATA";

				case DATA_END_BYTE:
					return ".";

				case QUIT_BYTE:
					return "QUIT";

				case RSET_BYTE:
					return "RSET";

				case VRFY_BYTE:
					return "VRFY";

				case EXPN_BYTE:
					return "EXPN";

				case HELP_BYTE:
					return "HELP";

				case NOOP_BYTE:
					return "NOOP";

				case UNREC_BYTE:
					return "Unrecognized command / data";

				case BLANK_LINE_BYTE:
					return "Blank line";

				default:
					return "Unknown";

			}
		}
	}
}

using System.Collections;
﻿using UnityEngine;

namespace DRAConsole
{
	public class LogMessage : BaseMessage
	{
		public enum LogLevel { ERROR, WARNING, INFO };

		public int Code { get { return m_Code; } }
		public LogLevel Level { get { return m_Level; } }
		public string LogLevelName { get { return LogMessage.LogLevelToString(m_Level); } }

		protected int m_Code;
		protected LogLevel m_Level;

		public LogMessage(string contents, LogLevel level = LogLevel.INFO, int code = -1) : base(contents)
		{
			m_Level = level;
			m_Code = code;
		}

		public override string ToString()
		{
			return base.ToString(string.Format("[{0}]{1}: ", LogMessage.LogLevelToString(m_Level), (m_Code >= 0 ? string.Format(" ({0})", m_Code) : "")));
		}

		protected static string LogLevelToString(LogLevel level)
		{
			switch(level) {
				case LogLevel.ERROR: return "ERROR";
				case LogLevel.WARNING: return "WARNING";
				case LogLevel.INFO: return "INFO";
				default: return "U";
			}
		}
	}
}

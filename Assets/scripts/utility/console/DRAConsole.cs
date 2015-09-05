using System.Text;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
﻿using UnityEngine;


namespace DRAConsole
{
	public class BaseMessage
	{
		public float CreationTimeStamp { get { return m_CreationTimeStamp; } }

		protected float m_CreationTimeStamp;
		protected string m_Contents;

		public BaseMessage(string contents)
		{
			m_CreationTimeStamp = Time.time;
			m_Contents = contents;
		}

		public override string ToString()
		{
			StringBuilder buffer = new StringBuilder();

			buffer.Append(string.Format("[{0:0.00}] ", m_CreationTimeStamp));
			buffer.Append(m_Contents);

			return buffer.ToString();
		}
	}

	public class Console
	{
		public static Console Singleton
		{
			get {
				if(singleton == null) {
					singleton = new Console();
				}

				return singleton;
			}
		}

		protected static Console singleton;

		public float LastUpdated { get { return (m_Messages.Count > 0 ? m_Messages.Last().CreationTimeStamp : -1.0f); } }

		protected List<BaseMessage> m_Messages;

		private Console()
		{
			m_Messages = new List<BaseMessage>();
		}

		public void AddMessage(BaseMessage new_message)
		{
			m_Messages.Add(new_message);
		}

		public List<BaseMessage> GetMessages(float start_time, float end_time, bool start_inclusive = true, bool end_inclusive = true)
		{
			List<BaseMessage> message_collection = new List<BaseMessage>();

			foreach(BaseMessage msg in m_Messages) {
				if(msg.CreationTimeStamp > start_time && msg.CreationTimeStamp < end_time || (start_inclusive && msg.CreationTimeStamp == start_time) || (end_inclusive && msg.CreationTimeStamp == end_time)) {
					message_collection.Add(msg);
				}
			}

			return message_collection;
		}
	}
}

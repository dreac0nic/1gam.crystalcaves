using System.Text;
using System.Collections;
using System.Collections.Generic;
﻿using UnityEngine;


namespace DRAConsole
{
	public class Message
	{
		protected float m_CreationTimeStamp;
		protected string m_Contents;

		public Message(string contents)
		{
			m_CreationTimeStamp = Time.time;
			m_Contents = contents;
		}

		public override string ToString()
		{
			StringBuilder buffer = new StringBuilder();

			buffer.Append(string.Format("[{0:00.00}] ", m_CreationTimeStamp));
			buffer.Append(m_Contents);

			return buffer.ToString();
		}
	}

	public class Manager
	{
		public static Manager Singleton
		{
			get {
				if(singleton == null) {
					singleton = new Manager();
				}

				return singleton;
			}
		}

		protected static Manager singleton;

		protected List<Message> messages;

		private Manager()
		{
		}

		public void AddMessage(Message new_message)
		{
			messages.Add(new_message);
		}
	}
}

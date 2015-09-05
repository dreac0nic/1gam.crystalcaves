using System.Text;
using System.Collections;
using System.Collections.Generic;
﻿using UnityEngine;
﻿using UnityEngine.UI;
using DRAConsole;

[RequireComponent(typeof(Text))]
public class DRACSimpleTextUpdater : MonoBehaviour
{
	protected Text m_ConsoleText;
	protected float m_LastUpdated = 0.0f;

	public void Awake()
	{
		m_ConsoleText = GetComponent<Text>();
	}

	public void Update()
	{
		if(m_ConsoleText) {
			if(Console.Singleton.LastUpdated > m_LastUpdated) {
				bool first_entry = string.IsNullOrEmpty(m_ConsoleText.text);
				StringBuilder buffer = new StringBuilder();

				foreach(BaseMessage msg in Console.Singleton.GetMessages(m_LastUpdated, Time.time, false)) {
					if(!first_entry) {
						buffer.Append("\n");
					} else {
						first_entry = false;
					}

					buffer.Append(msg.ToString());
				}

				m_ConsoleText.text += buffer.ToString();

				m_LastUpdated = Console.Singleton.LastUpdated;
			}
		} else {
			m_ConsoleText = GetComponent<Text>();
		}
	}
}

using System.Collections;
﻿using UnityEngine;
﻿using UnityEngine.UI;
using DRAConsole;

public class DRACSimpleMessageSender : MonoBehaviour
{
	public Text TargetInput;

	public void SubmitConsoleMessage()
	{
		if(TargetInput) {
			Console.Singleton.AddMessage(new BaseMessage(TargetInput.text));
			TargetInput.text = "";
		}
	}
}

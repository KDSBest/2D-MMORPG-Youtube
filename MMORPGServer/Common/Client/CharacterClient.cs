using Common.Client.Workflow;
using Common.Protocol.Character;
using Common.Protocol.Chat;
using Common.Workflow;
using ReliableUdp;
using System;
using System.Collections.Generic;

namespace Common.Client
{

	public class CharacterClient : BaseClient<CharacterWorkflow>
	{
		public CharacterWorkflow Workflow { get; set; }

		public Action<CharacterMessage> OnNewCharacterMessage { get; set; }

		public override void OnWorkflowSwitch(UdpPeer peer, IWorkflow newWorkflow)
		{
			Workflow = newWorkflow as CharacterWorkflow;
			if(Workflow != null)
			{
				Workflow.OnNewCharacterMessage = OnNewCharacterMessage;
			}
		}

		public void SendCharacterRequest(List<string> names)
		{
			var wf = Workflow;
			if (wf == null)
				return;

			wf.SendCharacterRequest(names);
		}

		public void SendCharacterCreation(CharacterInformation c)
		{
			var wf = Workflow;
			if (wf == null)
				return;

			wf.SendCharacterCreation(c);
		}
	}
}

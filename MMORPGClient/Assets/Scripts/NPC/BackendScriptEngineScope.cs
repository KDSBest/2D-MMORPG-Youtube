using Assets.Scripts.Character;
using Common.IoC;
using Common.Protocol.Quest;
using Common.PublishSubscribe;
using Common.ScriptLanguage.AST;
using Common.ScriptLanguage.VM;
using System.Collections.Generic;

namespace Assets.Scripts.NPC
{
	public class BackendScriptEngineScope : IVMScope
	{
		private readonly IPubSub pubsub;

		public string Scope => "Quest";

		public BackendScriptEngineScope()
		{
			DILoader.Initialize();
			pubsub = DI.Instance.Resolve<IPubSub>();
		}

		public VMVar Execute(string functionName, List<VMVar> parameters)
		{
			string questName;
			switch (functionName)
			{
				case "IsAvailable":
					questName = parameters[0].ValueString;
					if (DI.Instance.Resolve<ICurrentContext>().QuestTracking.AcceptedQuests.Contains(questName))
					{
						return new VMVar()
						{
							Type = VMType.Number,
							ValueNumber = 0
						};
					}

					return new VMVar()
					{
						Type = VMType.Number,
						ValueNumber = -1
					};
				case "Accept":
					questName = parameters[0].ValueString;
					return AcceptQuest(questName);
			}

			return new VMVar()
			{
				Type = VMType.Number,
				ValueNumber = 0
			};
		}

		private VMVar AcceptQuest(string questName)
		{
			pubsub.Publish(new AcceptQuestMessage()
			{
				QuestName = questName
			});

			return new VMVar()
			{
				Type = VMType.Number,
				ValueNumber = -1
			};
		}
	}
}

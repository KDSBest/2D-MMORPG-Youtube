using Assets.Scripts.Character;
using Common.IoC;
using Common.Protocol.Quest;
using Common.PublishSubscribe;
using Common.ScriptLanguage.AST;
using Common.ScriptLanguage.VM;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Assets.Scripts.NPC
{
	public class BackendScriptEngineScope : IVMScope
	{
		private readonly IPubSub pubsub;

		public string Scope => "Quest";
		private int waitInMs = 50;
		private int timoutInMs = 1000 * 10;

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
			string correlationId = Guid.NewGuid().ToString();
			VMVar result;
			bool responseAvailable = false;

			pubsub.Subscribe<QuestResultMessage>((backendResponse) =>
			{
				if (backendResponse.Id == questName)
				{
					if(backendResponse.Result)
					{
						DI.Instance.Resolve<ICurrentContext>().QuestTracking.AcceptedQuests.Add(questName);
					}

					result = new VMVar()
					{
						Type = VMType.Number,
						ValueNumber = backendResponse.Result ? -1 : 0
					};

					responseAvailable = true;
				}

				pubsub.Unsubscribe<QuestResultMessage>(correlationId);
			}, correlationId);

			pubsub.Publish(new AcceptQuestMessage()
			{
				QuestName = questName
			});

			int delay = 0;
			while (!responseAvailable)
			{
				// TODO: Leverage CoRoutine
				Thread.Sleep(waitInMs);
				delay += waitInMs;

				if (delay >= timoutInMs)
					break;
			}

			return new VMVar()
			{
				Type = VMType.Number,
				ValueNumber = 0
			};
		}
	}
}

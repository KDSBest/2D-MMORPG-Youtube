using Assets.Scripts.Character;
using Common;
using Common.IoC;
using Common.Protocol.Quest;
using Common.PublishSubscribe;
using Common.ScriptLanguage.AST;
using Common.ScriptLanguage.VM;
using System;
using System.Collections.Generic;

namespace Assets.Scripts.NPC
{
	public class QuestBackendScope : IVMScope
	{
		private readonly IPubSub pubsub;
		private readonly ICurrentContext context;

		public string Scope => "Quest";

		public QuestBackendScope()
		{
			DILoader.Initialize();
			pubsub = DI.Instance.Resolve<IPubSub>();
			context = DI.Instance.Resolve<ICurrentContext>();
		}

		public VMVar Execute(string functionName, List<VMVar> parameters)
		{
			string questName;
			switch (functionName)
			{
				case "IsAvailable":
					questName = parameters[0].ValueString;
					if (!QuestLoader.Quests[questName].IsAvailable(context.QuestTracking, context.Character.Stats.Level))
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
				case "IsQuestFinished":
					questName = parameters[0].ValueString;
					return IsQuestFinished(questName);
				case "FinishQuest":
					questName = parameters[0].ValueString;
					return FinishQuest(questName);
			}

			return new VMVar()
			{
				Type = VMType.Number,
				ValueNumber = 0
			};
		}


		private VMVar FinishQuest(string questName)
		{
			if(!IsQuestFinished(questName).GetBool())
			{ 
				return new VMVar()
				{
					Type = VMType.Number,
					ValueNumber = 0
				};
			}

			pubsub.Publish(new FinishQuestMessage()
			{
				QuestName = questName
			});

			return new VMVar()
			{
				Type = VMType.Number,
				ValueNumber = -1
			};
		}

		private VMVar IsQuestFinished(string questName)
		{
			if (!context.QuestTracking.AcceptedQuests.Contains(questName))
			{
				return new VMVar()
				{
					Type = VMType.Number,
					ValueNumber = 0
				};
			}

			if(!QuestLoader.Quests.ContainsKey(questName))
			{
				return new VMVar()
				{
					Type = VMType.Number,
					ValueNumber = 0
				};
			}

			if(!QuestLoader.Quests[questName].Task.IsFinished(questName, context.QuestTracking))
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

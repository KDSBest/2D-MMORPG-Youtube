using Common.IoC;
using Common.Protocol.Inventory;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common.QuestSystem
{
	public class Quest
	{
		public string Path;

		public string Name;
		public int Level;
		public string PreQuest;

		public string UnityReferenceTask;
		public IQuestTask Task;
	}
}

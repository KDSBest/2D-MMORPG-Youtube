using Common.IoC;
using Common.Protocol.Inventory;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common.QuestSystem
{
	public class Quest
	{
		public string Name;
		public int Level;
		public string PreQuest;
		public List<QuestReward> Rewards = new List<QuestReward>();

		public IQuestTask Task;

		// For Unity Quest Editor
		public string Path;
		public string UnityReferenceTask;

		public bool IsAvailable(QuestTracking questTracking, int playerLevel)
		{
			if (Level > playerLevel)
				return false;

			if (questTracking.AcceptedQuests.Contains(this.Name))
				return false;

			if (questTracking.FinishedQuests.Contains(this.Name))
				return false;

			if (!string.IsNullOrEmpty(PreQuest) && !questTracking.FinishedQuests.Contains(PreQuest))
				return false;

			return true;
		}
	}
}

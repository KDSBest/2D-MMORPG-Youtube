using System.Collections.Generic;

namespace Common.QuestSystem
{
	public class InventoryQuestTask : IQuestTask
	{
		public string ItemId;
		public int Amount;

		public List<InventoryQuestTask> GetInventoryQuestTasks(string questName, QuestTracking questTracking)
		{
			return new List<InventoryQuestTask>()
			{
				this
			};
		}

		public string GetDisplay(string questName, QuestTracking questTracking)
		{

			return $"Collect @{{{ItemId}}}@ {questTracking.Inventory.GetAmount(ItemId)}/{Amount}";
		}

		public bool IsFinished(string questName, QuestTracking questTracking)
		{
			return questTracking.Inventory.GetAmount(ItemId) >= Amount;
		}

		public bool OnKilled(string questName, string id, QuestTracking questTracking)
		{
			return false;
		}
	}
}

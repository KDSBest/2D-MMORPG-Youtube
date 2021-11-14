namespace Common.QuestSystem
{
	public class InventoryQuestTask : IQuestTask
	{
		public string ItemId;
		public int Amount;

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

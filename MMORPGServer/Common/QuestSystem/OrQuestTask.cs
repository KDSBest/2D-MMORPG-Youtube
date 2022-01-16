using System.Collections.Generic;

namespace Common.QuestSystem
{
	public class OrQuestTask : IQuestTask
	{
		public IQuestTask A;
		public IQuestTask B;
		public List<InventoryQuestTask> GetInventoryQuestTasks(string questName, QuestTracking questTracking)
		{
			// TODO: Client can select
			if (A.IsFinished(questName, questTracking))
			{
				return A.GetInventoryQuestTasks(questName, questTracking);
			}

			return B.GetInventoryQuestTasks(questName, questTracking);
		}

		public string GetDisplay(string questName, QuestTracking questTracking)
		{
			return $"{A.GetDisplay(questName, questTracking)}\r\nOr\r\n{B.GetDisplay(questName, questTracking)}";
		}

		public bool IsFinished(string questName, QuestTracking questTracking)
		{
			return A.IsFinished(questName, questTracking) || B.IsFinished(questName, questTracking);
		}

		public bool OnKilled(string questName, string id, QuestTracking questTracking)
		{
			bool aUpdated = A.OnKilled(questName, id, questTracking);
			bool bUpdated = B.OnKilled(questName, id, questTracking);
			return aUpdated || bUpdated;
		}
	}
}

using System.Collections.Generic;

namespace Common.QuestSystem
{
	public interface IQuestTask
	{
		bool OnKilled(string questName, string id, QuestTracking questTracking);

		string GetDisplay(string questName, QuestTracking questTracking);

		bool IsFinished(string questName, QuestTracking questTracking);

		List<InventoryQuestTask> GetInventoryQuestTasks(string questName, QuestTracking questTracking);
	}
}

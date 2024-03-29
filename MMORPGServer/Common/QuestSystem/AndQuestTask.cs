﻿using System.Collections.Generic;

namespace Common.QuestSystem
{
	public class AndQuestTask : IQuestTask
	{
		public IQuestTask A;
		public IQuestTask B;

		public List<InventoryQuestTask> GetInventoryQuestTasks(string questName, QuestTracking questTracking)
		{
			var result = new List<InventoryQuestTask>();

			result.AddRange(A.GetInventoryQuestTasks(questName, questTracking));
			result.AddRange(B.GetInventoryQuestTasks(questName, questTracking));

			return result;
		}

		public string GetDisplay(string questName, QuestTracking questTracking)
		{
			return $"{A.GetDisplay(questName, questTracking)}\r\nAnd\r\n{B.GetDisplay(questName, questTracking)}";
		}

		public bool IsFinished(string questName, QuestTracking questTracking)
		{
			return A.IsFinished(questName, questTracking) && B.IsFinished(questName, questTracking);
		}

		public bool OnKilled(string questName, string id, QuestTracking questTracking)
		{
			bool aUpdated = A.OnKilled(questName, id, questTracking);
			bool bUpdated = B.OnKilled(questName, id, questTracking);
			return aUpdated || bUpdated;
		}
	}
}

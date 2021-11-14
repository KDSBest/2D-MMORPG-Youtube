using System;

namespace Common.QuestSystem
{
	public class MobKillTask : IQuestTask
	{
		public string MobId;
		public int Amount;
		public Guid Id = Guid.NewGuid();

		public string GetDisplay(string questName, QuestTracking questTracking)
		{

			return $"Kill @{{{MobId}}}@ {questTracking.GetValue(questName, Id.ToString("N"), MobId)}/{Amount}";
		}

		public bool IsFinished(string questName, QuestTracking questTracking)
		{
			return questTracking.GetValue(questName, Id.ToString("N"), MobId) >= Amount;
		}

		public bool OnKilled(string questName, string id, QuestTracking questTracking)
		{
			if (id != MobId)
				return false;

			questTracking.IncrementValue(questName, Id.ToString("N"), MobId);
			return true;
		}
	}
}

namespace Common.QuestSystem
{
	public class OrQuestTask : IQuestTask
	{
		public IQuestTask A;
		public IQuestTask B;

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

using Common.Protocol.Inventory;
using System;
using System.Collections.Generic;

namespace Common.QuestSystem
{
	public class QuestTracking
	{
		public Inventory Inventory { get; set; }

		public Dictionary<string, int> QuestTrackingValues { get; set; }

		private string GetKey(string questName, string questTaskId, string mobId)
		{
			return $"{questName}_{questTaskId}_{mobId}";
		}

		public int GetValue(string questName, string questTaskId, string mobId)
		{
			string key = GetKey(questName, questTaskId, mobId);
			if (!QuestTrackingValues.ContainsKey(key))
				return 0;

			return QuestTrackingValues[key];
		}

		internal void IncrementValue(string questName, string questTaskId, string mobId)
		{
			string key = GetKey(questName, questTaskId, mobId);
			if (!QuestTrackingValues.ContainsKey(key))
			{
				QuestTrackingValues.Add(key, 1);
				return;
			}

			QuestTrackingValues[key] = QuestTrackingValues[key] + 1;

		}
	}
}

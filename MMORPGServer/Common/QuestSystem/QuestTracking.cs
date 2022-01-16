using Common.Protocol.Inventory;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Common.QuestSystem
{
	public class QuestTracking
	{
		[JsonProperty("id")]
		public string Id { get; set; }

		public Inventory Inventory { get; set; }

		public Dictionary<string, int> QuestTrackingValues { get; set; } = new Dictionary<string, int>();

		public List<string> AcceptedQuests { get; set; } = new List<string>();
		public List<string> FinishedQuests { get; set; } = new List<string>();

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

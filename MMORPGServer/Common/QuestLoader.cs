using Common.QuestSystem;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace Common
{
	public static class QuestLoader
	{
		public static Dictionary<string, Quest> Quests = new Dictionary<string, Quest>();
		public static void Load()
		{
			Load("Quests");
		}

		public static void Load(string path)
		{
			var filePathes = Directory.GetFiles(path, "*.quest");
			foreach(var filePath in filePathes)
			{
				var qFile = File.ReadAllText(filePath);
				var quest = JsonConvert.DeserializeObject<Quest>(qFile, new JsonSerializerSettings()
				{
					TypeNameHandling = TypeNameHandling.Auto
				});
				Quests.Add(quest.Name, quest);
			}
		}
	}
}

using Common.GameDesign;
using Common.GameDesign.Skill;
using Common.QuestSystem;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace Common
{
	public static class AoESkillsLoader
	{
		public static Dictionary<SkillCastType, SkillCollision> SkillCollisions = new Dictionary<SkillCastType, SkillCollision>();
		public static void Load()
		{
			Load("AoESkills");
		}

		public static void Load(string path)
		{
			var filePathes = Directory.GetFiles(path, "*.json");
			foreach(var filePath in filePathes)
			{
				var scFile = File.ReadAllText(filePath);
				var sc = JsonConvert.DeserializeObject<SkillCollision>(scFile, new JsonSerializerSettings()
				{
					TypeNameHandling = TypeNameHandling.Auto
				});
				SkillCollisions.Add(sc.CastType, sc);
			}
		}
	}
}

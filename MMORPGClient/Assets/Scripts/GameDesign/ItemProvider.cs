using Assets.Scripts.ScriptableObjects;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.GameDesign
{
	public class ItemProvider
	{
		private Dictionary<string, ItemData> items = new Dictionary<string, ItemData>();

		public ItemData[] Initialize()
		{
			ItemData[] items = Resources.LoadAll<ItemData>("Items");

			this.items.Clear();
			foreach(var item in items)
			{
				this.items.Add(item.name, item);
			}

			return items;
		}

		public string GetName(string id)
		{
			if (!items.ContainsKey(id))
				return "???";

			return items[id].DisplayName;
		}

		public Rarity GetRarity(string id)
		{
			if (!items.ContainsKey(id))
				return Rarity.Common;

			return items[id].Rarity;
		}

		public string GetContent(string id, RarityColorConfig colorConfig)
		{
			if (!items.ContainsKey(id))
				return "???";

			;
			string result = items[id].TooltipContent;

			var rarity = GetRarity(id);
			result = result.Replace("{Rarity}", rarity.ToString());
			result = result.Replace("{RarityColor}", colorConfig.GetColorHTML(rarity));

			return result;
		}
	}
}

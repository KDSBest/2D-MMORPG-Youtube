using Assets.Scripts.GameDesign;
using UnityEngine;

namespace Assets.Scripts.GameDesign
{
	public class ItemProvider
	{
		public string GetName(string id)
		{
			return "Flower";
		}

		public Rarity GetRarity(string id)
		{
			return Rarity.Epic;
		}

		public string GetContent(string id, RarityColorConfig colorConfig)
		{
			string result = 
@"Rarity: <color=#{RarityColor}>{Rarity}</color>
Category: Flower
Used for: Crafting";

			var rarity = GetRarity(id);
			result = result.Replace("{Rarity}", rarity.ToString());
			result = result.Replace("{RarityColor}", colorConfig.GetColorHTML(rarity));

			return result;
		}
	}
}

using Assets.Scripts.GameDesign;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.UI.SubScreen
{
	public class Tooltip : MonoBehaviour
	{
		public TMP_Text Header;
		public TMP_Text Content;

		public void SetItem(string id, ItemProvider itemProvider, RarityColorConfig rarityColorConfig)
		{
			Content.text = itemProvider.GetContent(id, rarityColorConfig);
			Header.text = itemProvider.GetName(id);

			var rarity = itemProvider.GetRarity(id);
			var col = rarityColorConfig.GetColor(rarity);
			Header.color = col;
		}
	}
}
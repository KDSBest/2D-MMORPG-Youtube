using System;
using UnityEngine;

namespace Assets.Scripts.GameDesign
{
	public class RarityColorConfig
	{
		private Color[] Colors = new Color[4];

		public RarityColorConfig()
		{
			Colors[0] = GetColor("#ECF0F1");
			Colors[1] = GetColor("#2ECC71");
			Colors[2] = GetColor("#9B59B6");
			Colors[3] = GetColor("#F1C40F");
		}

		private static Color GetColor(string htmlString)
		{
			Color result;
			ColorUtility.TryParseHtmlString(htmlString, out result);
			return result;
		}

		public Color GetColor(Rarity rarity)
		{
			int colIndex = (int)rarity;
			if (colIndex < 0 || colIndex >= Colors.Length)
				return Colors[0];

			return Colors[colIndex];
		}

		public string GetColorHTML(Rarity rarity)
		{
			Color col = GetColor(rarity);
			return ColorUtility.ToHtmlStringRGBA(col);
		}
	}
}

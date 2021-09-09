using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.SubScreen
{
	public class InventorySlot : MonoBehaviour
	{
		public TMP_Text AmountText;
		public Image Image;

		private int amount = 0;
		public int Amount
		{
			get
			{
				return amount;
			}
			set
			{
				amount = value;
				AmountText.text = amount.ToString();
			}
		}
	}
}

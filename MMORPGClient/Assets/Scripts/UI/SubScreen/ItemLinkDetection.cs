using Assets.Scripts.GameDesign;
using Assets.Scripts.PubSubEvents.Tooltip;
using Common.IoC;
using Common.PublishSubscribe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Assets.Scripts.UI.SubScreen
{

	[RequireComponent(typeof(TMP_Text))]
	public class ItemLinkDetection : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
	{
		private ItemProvider itemProvider;
		private RarityColorConfig rarityColorConfig;
		private TMP_Text Text;
		private IPubSub pubsub;
		private Regex rx;

		private string itemTemplate = "<color=#{RarityColor}><link=\"{Id}\">[{Name}]</link></color>";
		private string itemRegex = "\\@\\{([a-zA-Z0-9\\-_]*)\\}\\@";
		private string itemReplacer = "@{{Id}}@";

		private string currentId = string.Empty;
		private bool mouseHover = false;

		public void Awake()
		{
			DILoader.Initialize();

			rx = new Regex(itemRegex);
			Text = this.GetComponent<TMP_Text>();

			pubsub = DI.Instance.Resolve<IPubSub>();
			itemProvider = DI.Instance.Resolve<ItemProvider>();
			rarityColorConfig = DI.Instance.Resolve<RarityColorConfig>();
		}

		public void SetText(string text)
		{
			string result = text;
			var matches = rx.Matches(result);

			foreach (Match match in matches)
			{
				if (match.Groups.Count != 2)
					continue;

				string id = match.Groups[1].Value;
				string replacer = GetItemReplacer(id);
				string link = GetItemLink(id);
				result = result.Replace(replacer, link);
			}
			Text.text = result;
		}

		private string GetItemLink(string id)
		{
			string result = itemTemplate;
			result = result.Replace("{Name}", itemProvider.GetName(id));
			result = result.Replace("{Id}", id);

			var rarity = itemProvider.GetRarity(id);
			var rarityColor = rarityColorConfig.GetColorHTML(rarity);
			result = result.Replace("{RarityColor}", rarityColor);

			return result;
		}

		private string GetItemReplacer(string id)
		{
			return itemReplacer.Replace("{Id}", id);
		}


		public void Update()
		{
			if (!mouseHover)
			{
				return;
			}

			var pos = Mouse.current.position.ReadValue();
			int linkIndex = TMP_TextUtilities.FindIntersectingLink(Text, pos, null);
			if(linkIndex != -1)
			{
				currentId = Text.textInfo.linkInfo[linkIndex].GetLinkID();
				pubsub.Publish(new ShowTooltip()
				{
					Id = currentId,
					Position = pos
				});
			}
			else
			{
				HideTooltip();
			}
		}

		private void HideTooltip()
		{
			if (!string.IsNullOrEmpty(currentId))
			{
				pubsub.Publish(new HideTooltip(currentId));
				currentId = string.Empty;
			}
		}

		public void OnPointerExit(PointerEventData pointerEventData)
		{
			mouseHover = false;
			HideTooltip();
		}

		public void OnPointerEnter(PointerEventData pointerEventData)
		{
			mouseHover = true;
		}
	}
}

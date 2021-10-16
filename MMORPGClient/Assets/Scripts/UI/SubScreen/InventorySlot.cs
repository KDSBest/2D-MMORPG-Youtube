using Assets.Scripts.PubSubEvents.Tooltip;
using Common.IoC;
using Common.PublishSubscribe;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Assets.Scripts.UI.SubScreen
{
	public class InventorySlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
	{
		public TMP_Text AmountText;
		public Image Image;
		public string Id;

		private int amount = 0;
		private IPubSub pubsub;
		private bool mouseHover;

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
		public void Awake()
		{
			DILoader.Initialize();
			pubsub = DI.Instance.Resolve<IPubSub>();
		}

		public void Update()
		{
			if(!mouseHover)
			{
				return;
			}

			pubsub.Publish(new ShowTooltip()
			{
				Id = Id,
				Position = Mouse.current.position.ReadValue()
			});			
		}

		public void OnPointerExit(PointerEventData pointerEventData)
		{
			mouseHover = false;
			pubsub.Publish(new HideTooltip(Id));
		}

		public void OnPointerEnter(PointerEventData pointerEventData)
		{
			mouseHover = true;
			pubsub.Publish(new ShowTooltip()
			{
				Id = Id,
				Position = pointerEventData.position
			});
		}
	}
}

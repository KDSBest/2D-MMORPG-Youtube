using Assets.Scripts.GameDesign;
using Assets.Scripts.PubSubEvents.Tooltip;
using Assets.Scripts.UI.SubScreen;
using Common.IoC;
using Common.Protocol.PlayerEvent;
using Common.PublishSubscribe;
using System;
using UnityEngine;

namespace Assets.Scripts.UI
{

	public class TooltipScreenController : MonoBehaviour
    {
        private IPubSub pubsub;
		private ItemProvider itemProvider;
		public Tooltip ToolTip;
        private string currentTooltip = string.Empty;
		private RarityColorConfig rarityColorConfig;

		public void Awake()
        {
            DILoader.Initialize();

            pubsub = DI.Instance.Resolve<IPubSub>();
            itemProvider = DI.Instance.Resolve<ItemProvider>();
            rarityColorConfig = DI.Instance.Resolve<RarityColorConfig>();
        }

        public void OnEnable()
        {
            pubsub.Subscribe<ShowTooltip>(OnShowTooltip, this.GetType().Name);
            pubsub.Subscribe<HideTooltip>(OnHideTooltip, this.GetType().Name);
        }

		private void OnHideTooltip(HideTooltip ev)
		{
            if(currentTooltip == ev.Id)
			{
                ToolTip.gameObject.SetActive(false);
			}                
		}

		private void OnShowTooltip(ShowTooltip ev)
		{
            if (currentTooltip != ev.Id)
            {
                currentTooltip = ev.Id;
                ToolTip.SetItem(ev.Id, itemProvider, rarityColorConfig);
            }

            ToolTip.transform.position = new Vector3(ev.Position.x, ev.Position.y);
            ToolTip.gameObject.SetActive(true);
        }

        public void OnDisable()
        {
            pubsub.Unsubscribe<ShowTooltip>(this.GetType().Name);
            pubsub.Unsubscribe<HideTooltip>(this.GetType().Name);
        }
    }
}
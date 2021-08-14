using Common.IoC;
using Common.Protocol.PlayerEvent;
using Common.PublishSubscribe;
using UnityEngine;

namespace Assets.Scripts.UI
{
	public class DailyLoginScreenController : MonoBehaviour
    {
        private IPubSub pubsub;
        public GameObject DailyLogin;

        public void OnEnable()
        {
            DILoader.Initialize();
            pubsub = DI.Instance.Resolve<IPubSub>();

            pubsub.Subscribe<PlayerEventMessage>(OnPlayerEvent, this.GetType().Name);
        }

		private void OnPlayerEvent(PlayerEventMessage ev)
		{
            if(ev.Type == PlayerEventType.DailyLogin)
			{
                DailyLogin.SetActive(true);
			}
		}

		public void OnDisable()
        {
            pubsub.Unsubscribe<PlayerEventMessage>(this.GetType().Name);
        }
	}
}
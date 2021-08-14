using Assets.Scripts.PubSubEvents.StartUI;
using Common.IoC;
using Common.PublishSubscribe;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
	public class LoadingScreenController : MonoBehaviour
    {
        private IPubSub pubsub;
        public GameObject LoadingScreen;
        public TMP_Text ActionText;
        public Slider ProgressBar;

        public void OnEnable()
        {
            DILoader.Initialize();
            pubsub = DI.Instance.Resolve<IPubSub>();

            pubsub.Subscribe<ControlLoadingScreen>(OnControlLoadingScreen, this.GetType().Name);
        }

        public void OnDisable()
        {
            pubsub.Unsubscribe<ControlLoadingScreen>(this.GetType().Name);
        }

        public void OnControlLoadingScreen(ControlLoadingScreen data)
        {
            LoadingScreen.SetActive(data.Visible);
            ActionText.text = data.CurrentAction;
            ProgressBar.value = data.Percentage;
        }
    }
}
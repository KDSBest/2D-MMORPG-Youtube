using Assets.Scripts;
using Assets.Scripts.PubSubEvents.StartUI;
using Common.IoC;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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

        pubsub.Subscribe<ControlLoadingScreen>(OnControlLoadingScreen, this.name);
    }

    public void OnDisable()
    {
        pubsub.Unsubscribe<ControlLoadingScreen>(this.name);
    }

    public void OnControlLoadingScreen(ControlLoadingScreen data)
    {
        LoadingScreen.SetActive(data.Visible);
        ActionText.text = data.CurrentAction;
        ProgressBar.value = data.Percentage;
    }
}

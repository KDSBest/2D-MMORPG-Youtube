using Assets.Scripts;
using Assets.Scripts.PubSubEvents.LoginClient;
using Assets.Scripts.PubSubEvents.StartUI;
using Common.IoC;
using Common.Protocol.Login;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoginScreenController : MonoBehaviour
{
    private IPubSub pubsub;
    public GameObject LoginScreen;
    public TMP_InputField Email;
    public TMP_InputField Password;
    public TMP_Text ResultCode;

    public void OnEnable()
    {
        DILoader.Initialize();
        pubsub = DI.Instance.Resolve<IPubSub>();

        pubsub.Subscribe<ControlLoginScreen>(OnControlLoginScreen, this.name);
        pubsub.Subscribe<LoginRegisterResponseMessage>(OnLoginRegisterResponseMessage, this.name);
    }

    public void OnDisable()
    {
        pubsub.Unsubscribe<ControlLoginScreen>(this.name);
    }

    public void OnControlLoginScreen(ControlLoginScreen data)
    {
        LoginScreen.SetActive(data.Visible);
    }

    public void OnLoginRegisterResponseMessage(LoginRegisterResponseMessage data)
	{
        Debug.Log($"Token: {data.Token}");
        Context.Token = data.Token;
        ResultCode.text = data.Response.ToString();
    }

    public void Login()
    {
        var login = new TryLogin()
        {
            Email = this.Email.text,
            Password = this.Password.text
        };
        pubsub.Publish(login);
    }

    public void Register()
    {
        var register = new TryRegister()
        {
            Email = this.Email.text,
            Password = this.Password.text
        };
        pubsub.Publish(register);
    }
}

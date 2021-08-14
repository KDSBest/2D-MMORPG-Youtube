using Assets.Scripts.PubSubEvents.LoginClient;
using Assets.Scripts.PubSubEvents.StartUI;
using Common.IoC;
using Common.Protocol.Login;
using Common.PublishSubscribe;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.UI
{

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

            pubsub.Subscribe<ControlLoginScreen>(OnControlLoginScreen, this.GetType().Name);
            pubsub.Subscribe<LoginRegisterResponseMessage>(OnLoginRegisterResponseMessage, this.GetType().Name);
        }

        public void OnDisable()
        {
            pubsub.Unsubscribe<ControlLoginScreen>(this.GetType().Name);
            pubsub.Unsubscribe<LoginRegisterResponseMessage>(this.GetType().Name);
        }

        public void OnControlLoginScreen(ControlLoginScreen data)
        {
            LoginScreen.SetActive(data.Visible);
        }

        public void OnLoginRegisterResponseMessage(LoginRegisterResponseMessage data)
        {
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
}
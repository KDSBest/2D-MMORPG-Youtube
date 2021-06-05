using Assets.Scripts.Character;
using Assets.Scripts.ClientWrappers;
using Assets.Scripts.Language;
using Assets.Scripts.PubSubEvents.StartUI;
using Common.Client.Interfaces;
using Common.IoC;
using Common.Protocol.Character;
using Common.Protocol.Login;
using Common.PublishSubscribe;
using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts
{
	public class FrameworkBehaviour : MonoBehaviour
    {
        private const int waitMS = 50;
        private IPubSub pubsub;

        public void OnEnable()
        {
            DILoader.Initialize();
            pubsub = DI.Instance.Resolve<IPubSub>();
            pubsub.Subscribe<LoginRegisterResponseMessage>(OnNewLoginToken, this.name);
            pubsub.Subscribe<CharacterMessage>(OnCharacterMessage, this.name);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

		public void OnDisable()
        {
            pubsub.Unsubscribe<LoginRegisterResponseMessage>(this.name);
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
			this.StartCoroutine(InitLoginClient());
		}

        public void Update()
		{
            UnityDispatcher.ExecuteQueue();
        }

        private void OnCharacterMessage(CharacterMessage data)
        {
            if(!string.IsNullOrEmpty(data.Token))
			{
                DI.Instance.Resolve<ITokenProvider>().Token = data.Token;
                pubsub.Publish(new ControlCharacterScreen()
                {
                    Visible = false
                });

                this.StartCoroutine(InitMapClient());
            }
        }

        public IEnumerator InitMapClient()
        {
            UpdateProgress(0, string.Empty);

            yield return new WaitForEndOfFrame();

            UpdateProgress(0.4f, DI.Instance.Resolve<ILanguage>().ConnectToGame);

            var client = DI.Instance.Resolve<IMapClientWrapper>();
            Task<bool> connectTask = client.ConnectAsync("kdsmmorpgmaptown.westeurope.cloudapp.azure.com", 31000);

            while (!connectTask.Wait(waitMS))
            {
                yield return new WaitForEndOfFrame();
            }

            if (!connectTask.Result)
            {
                UpdateProgress(1, DI.Instance.Resolve<ILanguage>().ConnectionFailed);
                yield break;
            }

            yield return new WaitForEndOfFrame();

            UpdateProgress(0.6f, DI.Instance.Resolve<ILanguage>().ConnectToGame);
            while (!client.IsInitialized)
            {
                yield return new WaitForEndOfFrame();
            }

            UpdateProgress(1, string.Empty, false);

            pubsub.Publish(new PlayerControlEnable());
            this.StartCoroutine(InitChatClient());
        }

        public IEnumerator InitChatClient()
        {
            UpdateProgress(0, string.Empty);

            yield return new WaitForEndOfFrame();

            UpdateProgress(0.4f, DI.Instance.Resolve<ILanguage>().ConnectToChat);

            var client = DI.Instance.Resolve<IChatClientWrapper>();
            Task<bool> connectTask = client.ConnectAsync("kdsmmorpgworldchat.westeurope.cloudapp.azure.com", 30002);

            while (!connectTask.Wait(waitMS))
            {
                yield return new WaitForEndOfFrame();
            }

            if (!connectTask.Result)
            {
                UpdateProgress(1, DI.Instance.Resolve<ILanguage>().ConnectionFailed);
                yield break;
            }

            yield return new WaitForEndOfFrame();

            UpdateProgress(0.6f, DI.Instance.Resolve<ILanguage>().ConnectToChat);
            while (!client.IsInitialized)
            {
                yield return new WaitForEndOfFrame();
            }

            UpdateProgress(1, string.Empty, false);

            pubsub.Publish<ControlChatScreen>(new ControlChatScreen());
        }

        public void OnNewLoginToken(LoginRegisterResponseMessage data)
        {
            Debug.Log($"Token: {data.Token}");
            DI.Instance.Resolve<ITokenProvider>().Token = data.Token;

            if (data.Response == LoginRegisterResponse.Successful)
            {
                this.StartCoroutine(InitCharacterClient());
            }
        }

        public IEnumerator InitCharacterClient()
        {
            pubsub.Publish<ControlLoginScreen>(new ControlLoginScreen()
            {
                Visible = false
            });

            UpdateProgress(0, string.Empty);

            yield return new WaitForEndOfFrame();

            UpdateProgress(0.4f, DI.Instance.Resolve<ILanguage>().ConnectToCharacter);

            var charClient = DI.Instance.Resolve<ICharacterClientWrapper>();
            Task<bool> connectTask = charClient.ConnectAsync("kdsmmorpgchar.westeurope.cloudapp.azure.com", 30001);

            while (!connectTask.Wait(waitMS))
            {
                yield return new WaitForEndOfFrame();
            }

            if (!connectTask.Result)
            {
                UpdateProgress(1, DI.Instance.Resolve<ILanguage>().ConnectionFailed);
                yield break;
            }

            yield return new WaitForEndOfFrame();

            UpdateProgress(0.6f, DI.Instance.Resolve<ILanguage>().ConnectToCharacter);
            while (!charClient.IsInitialized)
            {
                yield return new WaitForEndOfFrame();
            }

            UpdateProgress(1, string.Empty, false);

            pubsub.Publish<ControlCharacterScreen>(new ControlCharacterScreen());
        }

        public IEnumerator InitLoginClient()
		{
            var loginClient = DI.Instance.Resolve<ILoginClientWrapper>();
			UpdateProgress(0, DI.Instance.Resolve<ILanguage>().Starting);

			yield return new WaitForEndOfFrame();

			UpdateProgress(0.4f, DI.Instance.Resolve<ILanguage>().ConnectToLogin);

            Task<bool> connectTask = loginClient.ConnectAsync("kdsmmorpgaccount.westeurope.cloudapp.azure.com", 30000);

            while(!connectTask.Wait(waitMS))
			{
                yield return new WaitForEndOfFrame();
			}

            if (!connectTask.Result)
            {
                UpdateProgress(1, DI.Instance.Resolve<ILanguage>().ConnectionFailed);
                yield break;
            }

            yield return new WaitForEndOfFrame();

            UpdateProgress(0.6f, DI.Instance.Resolve<ILanguage>().EncryptionHandshake);
            while (!loginClient.IsInitialized)
            {
                yield return new WaitForEndOfFrame();
            }

            UpdateProgress(1, string.Empty, false);
            DI.Instance.Resolve<IPubSub>().Publish<ControlLoginScreen>(new ControlLoginScreen());
        }

        private static void UpdateProgress(float percentage, string action, bool visible = true)
		{
			DI.Instance.Resolve<IPubSub>().Publish<ControlLoadingScreen>(new ControlLoadingScreen()
			{
				Visible = visible,
				Percentage = percentage,
				CurrentAction = action
			});
		}
	}
}
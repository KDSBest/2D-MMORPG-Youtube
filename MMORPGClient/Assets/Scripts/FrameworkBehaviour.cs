using Assets.Scripts.ClientWrappers;
using Assets.Scripts.Language;
using Assets.Scripts.PubSubEvents.StartUI;
using Common.IoC;
using Common.Protocol.Login;
using Common.PublishSubscribe;
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

        public void OnNewLoginToken(LoginRegisterResponseMessage data)
        {
            Debug.Log($"Token: {data.Token}");
            Context.Token = data.Token;

            if (data.Response == LoginRegisterResponse.Successful)
            {
                this.StartCoroutine(InitCharacterClient(Context.Token));
            }
        }

        public IEnumerator InitCharacterClient(string token)
        {
            pubsub.Publish<ControlLoginScreen>(new ControlLoginScreen()
            {
                Visible = false
            });

            UpdateProgress(0, DI.Instance.Resolve<ILanguage>().Starting);

            yield return new WaitForEndOfFrame();

            UpdateProgress(0.4f, DI.Instance.Resolve<ILanguage>().ConnectToCharacter);

            var charClient = DI.Instance.Resolve<ICharacterClientWrapper>();
            Task<bool> connectTask = charClient.ConnectAsync("kdsmmorpgchar.westeurope.cloudapp.azure.com", 30001, token);

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

            UpdateProgress(0.6f, DI.Instance.Resolve<ILanguage>().EncryptionHandshake);
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
using Assets.Scripts.ClientWrappers;
using Assets.Scripts.Language;
using Assets.Scripts.PubSubEvents.StartUI;
using Common.IoC;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts
{
	public class FrameworkBehaviour : MonoBehaviour
    {
        public LoginClientWrapper login = new LoginClientWrapper();
        private const int waitMS = 50;

        public void OnEnable()
        {
            DILoader.Initialize();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        public void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            this.StartCoroutine(ExecuteDispatcher());
            this.StartCoroutine(Initialize());
		}

        public IEnumerator ExecuteDispatcher()
		{
            while(true)
			{
                UnityDispatcher.ExecuteQueue();
                yield return new WaitForEndOfFrame();
			}
		}

		public IEnumerator Initialize()
		{
			UpdateProgress(0, DI.Instance.Resolve<ILanguage>().Starting);

			yield return new WaitForEndOfFrame();

			UpdateProgress(0.4f, DI.Instance.Resolve<ILanguage>().ConnectToLogin);

            Task<bool> connectTask = login.ConnectAsync("localhost", 3334);

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
            while (!login.IsInitialized)
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
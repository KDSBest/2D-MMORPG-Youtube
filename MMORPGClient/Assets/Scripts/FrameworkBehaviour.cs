using Assets.Scripts.Character;
using Assets.Scripts.ClientWrappers;
using Assets.Scripts.Language;
using Assets.Scripts.PubSubEvents.StartUI;
using Common.Client.Interfaces;
using Common.IoC;
using Common.Protocol.Character;
using Common.Protocol.Inventory;
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
		private static readonly string host = "localhost";
		private static readonly int worldChatPort = 3333;
		private static readonly int loginPort = 3334;
		private static readonly int characterPort = 3335;
		private static readonly int mapPort = 3336;
		private static readonly int eventPort = 3337;
		private static readonly int inventoryPort = 3338;
		private static readonly int combatPort = 3339;

		private const int waitMS = 50;
		private IPubSub pubsub;

		public void OnEnable()
		{
			DILoader.Initialize();
			pubsub = DI.Instance.Resolve<IPubSub>();
			pubsub.Subscribe<LoginRegisterResponseMessage>(OnNewLoginToken, this.GetType().Name);
			pubsub.Subscribe<CharacterMessage>(OnCharacterMessage, this.GetType().Name);
			SceneManager.sceneLoaded += OnSceneLoaded;
		}

		public void OnDisable()
		{
			pubsub.Unsubscribe<LoginRegisterResponseMessage>(this.GetType().Name);
			SceneManager.sceneLoaded -= OnSceneLoaded;
		}

		public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
		{
			InitLoginClient();
		}

		public void Update()
		{
			UnityDispatcher.ExecuteQueue();
		}

		private void OnCharacterMessage(CharacterMessage data)
		{
			if (!string.IsNullOrEmpty(data.Token))
			{
				var context = DI.Instance.Resolve<ICurrentContext>();
				context.Token = data.Token;
				context.Name = data.Character.Name;

				pubsub.Publish(new ControlCharacterScreen()
				{
					Visible = false
				});

				InitMapClient();
			}
		}

		public void InitMapClient()
		{

			var client = DI.Instance.Resolve<IMapClientWrapper>();
			this.StartCoroutine(InitializeClientWrapper(client, mapPort, DI.Instance.Resolve<ILanguage>().ConnectToGame, () =>
			{
				pubsub.Publish<PlayerControlEnable>(new PlayerControlEnable());
				InitChatClient();
			}, 0, 0.25f));

		}

		public void InitChatClient()
		{
			var client = DI.Instance.Resolve<IChatClientWrapper>();
			this.StartCoroutine(InitializeClientWrapper(client, worldChatPort, DI.Instance.Resolve<ILanguage>().ConnectToChat, () =>
			{
				pubsub.Publish<ControlChatScreen>(new ControlChatScreen());
				InitCombatClient();
			}, 0.25f, 0.35f));
		}

		public void InitCombatClient()
		{
			var client = DI.Instance.Resolve<ICombatClientWrapper>();
			this.StartCoroutine(InitializeClientWrapper(client, combatPort, DI.Instance.Resolve<ILanguage>().ConnectToCombat, () =>
			{
				InitInventoryClient();
			}, 0.35f, 0.5f));
		}

		public void InitInventoryClient()
		{
			var client = DI.Instance.Resolve<IInventoryClientWrapper>();
			this.StartCoroutine(InitializeClientWrapper(client, inventoryPort, DI.Instance.Resolve<ILanguage>().ConnectToInventory, () =>
			{
				DI.Instance.Resolve<IPubSub>().Publish<RequestInventoryMessage>(new RequestInventoryMessage());
				InitPlayerEventClient();
			}, 0.5f, 0.75f));
		}

		public void InitPlayerEventClient()
		{
			var client = DI.Instance.Resolve<IPlayerEventClientWrapper>();
			this.StartCoroutine(InitializeClientWrapper(client, eventPort, DI.Instance.Resolve<ILanguage>().ConnectToPlayerEvent, () =>
			{
			}, 0.75f, 1.0f));
		}

		public void OnNewLoginToken(LoginRegisterResponseMessage data)
		{
			UnityEngine.Debug.Log($"Token: {data.Token}");
			DI.Instance.Resolve<ITokenProvider>().Token = data.Token;

			if (data.Response == LoginRegisterResponse.Successful)
			{
				InitCharacterClient();
			}
		}

		public void InitCharacterClient()
		{
			pubsub.Publish<ControlLoginScreen>(new ControlLoginScreen()
			{
				Visible = false
			});

			var client = DI.Instance.Resolve<ICharacterClientWrapper>();
			this.StartCoroutine(InitializeClientWrapper(client, characterPort, DI.Instance.Resolve<ILanguage>().ConnectToCharacter, () =>
			{
				pubsub.Publish<ControlCharacterScreen>(new ControlCharacterScreen());
			}));
		}

		public void InitLoginClient()
		{
			var client = DI.Instance.Resolve<ILoginClientWrapper>();
			this.StartCoroutine(InitializeClientWrapper(client, loginPort, DI.Instance.Resolve<ILanguage>().ConnectToLogin, () =>
			{
				DI.Instance.Resolve<IPubSub>().Publish<ControlLoginScreen>(new ControlLoginScreen());
			}));
		}

		public IEnumerator InitializeClientWrapper(IClientWrapper client, int port, string statusMessage, Action doneAction, float startProgress = 0, float maxProgress = 1)
		{
			UpdateProgress(startProgress, DI.Instance.Resolve<ILanguage>().Starting);

			yield return new WaitForEndOfFrame();

			UpdateProgress(startProgress + ((maxProgress - startProgress) / 2), statusMessage);

			Task<bool> connectTask = client.ConnectAsync(host, port);

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

			while (!client.IsInitialized)
			{
				yield return new WaitForEndOfFrame();
			}

			UpdateProgress(maxProgress, string.Empty, false);

			if (doneAction != null)
				doneAction();
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
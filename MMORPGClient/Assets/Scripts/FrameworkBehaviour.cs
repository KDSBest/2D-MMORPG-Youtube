using Assets.Scripts.Character;
using Assets.Scripts.ClientWrappers;
using Assets.Scripts.GameDesign;
using Assets.Scripts.Language;
using Assets.Scripts.PubSubEvents.StartUI;
using Common;
using Common.Client;
using Common.Client.Interfaces;
using Common.IoC;
using Common.Protocol.Character;
using Common.Protocol.Inventory;
using Common.Protocol.Login;
using Common.Protocol.Quest;
using Common.PublishSubscribe;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts
{
	public class FrameworkBehaviour : MonoBehaviour
	{
		private string selectedServer = "minikube";
		private ServerConfiguration server;

		private const int waitMS = 50;
		private IPubSub pubsub;

		private ICurrentContext context;

		public void OnEnable()
		{
			Console.SetOut(new UnityConsoleTextWriter());

			LoadServerConfig();

			BaseClientSettings.Cert = File.ReadAllBytes(Path.Combine(Application.streamingAssetsPath, "mmo.cer"));
			DILoader.Initialize();

			this.context = DI.Instance.Resolve<ICurrentContext>();

			pubsub = DI.Instance.Resolve<IPubSub>();
			pubsub.Subscribe<LoginRegisterResponseMessage>(OnNewLoginToken, this.GetType().Name);
			pubsub.Subscribe<CharacterMessage>(OnCharacterMessage, this.GetType().Name);
			SceneManager.sceneLoaded += OnSceneLoaded;
		}

		private void LoadServerConfig()
		{
			string serverConfigFile = Path.Combine(Application.streamingAssetsPath, "Servers", $"{selectedServer}.server.config");
			server = JsonConvert.DeserializeObject<ServerConfiguration>(File.ReadAllText(serverConfigFile));
		}

		public void OnDisable()
		{
			pubsub.Unsubscribe<LoginRegisterResponseMessage>(this.GetType().Name);
			SceneManager.sceneLoaded -= OnSceneLoaded;
		}

		public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
		{
			DI.Instance.Resolve<ItemProvider>().Initialize();
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
				context.Token = data.Token;
				context.Character = data.Character;

				pubsub.Publish(new ControlCharacterScreen()
				{
					Visible = false
				});

				InitMapClient();
			}
			else if(context.Character != null && context.Character.Name == data.Character.Name)
			{
				context.Character = data.Character;
			}
		}

		public void InitMapClient()
		{

			var client = DI.Instance.Resolve<IMapClientWrapper>();
			this.StartCoroutine(InitializeClientWrapper(client, this.server.MapPort, DI.Instance.Resolve<ILanguage>().ConnectToGame, () =>
			{
				pubsub.Publish<PlayerControlEnable>(new PlayerControlEnable());
				InitQuestTrackingClient();
			}, 0, 0.25f));

		}

		public void InitQuestTrackingClient()
		{
			var client = DI.Instance.Resolve<IQuestTrackingClientWrapper>();
			this.StartCoroutine(InitializeClientWrapper(client, this.server.QuestTrackingPort, DI.Instance.Resolve<ILanguage>().ConnectToQuestTracking, () =>
			{
				QuestLoader.Load(Path.Combine(Application.streamingAssetsPath, "Quests"));
				pubsub.Publish<ControlQuestScreen>(new ControlQuestScreen());
				InitChatClient();
			}, 0.25f, 0.30f));
		}

		public void InitChatClient()
		{
			var client = DI.Instance.Resolve<IChatClientWrapper>();
			this.StartCoroutine(InitializeClientWrapper(client, this.server.WorldChatPort, DI.Instance.Resolve<ILanguage>().ConnectToChat, () =>
			{
				pubsub.Publish<ControlChatScreen>(new ControlChatScreen());
				InitCombatClient();
			}, 0.30f, 0.35f));
		}

		public void InitCombatClient()
		{
			var client = DI.Instance.Resolve<ICombatClientWrapper>();
			this.StartCoroutine(InitializeClientWrapper(client, this.server.CombatPort, DI.Instance.Resolve<ILanguage>().ConnectToCombat, () =>
			{
				InitInventoryClient();
			}, 0.35f, 0.5f));
		}

		public void InitInventoryClient()
		{
			var client = DI.Instance.Resolve<IInventoryClientWrapper>();
			this.StartCoroutine(InitializeClientWrapper(client, this.server.InventoryPort, DI.Instance.Resolve<ILanguage>().ConnectToInventory, () =>
			{
				DI.Instance.Resolve<IPubSub>().Publish<RequestInventoryMessage>(new RequestInventoryMessage());
				InitPlayerEventClient();
			}, 0.5f, 0.75f));
		}

		public void InitPlayerEventClient()
		{
			var client = DI.Instance.Resolve<IPlayerEventClientWrapper>();
			this.StartCoroutine(InitializeClientWrapper(client, this.server.EventPort, DI.Instance.Resolve<ILanguage>().ConnectToPlayerEvent, () =>
			{
			}, 0.75f, 1.0f));
		}

		public void OnNewLoginToken(LoginRegisterResponseMessage data)
		{
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
			this.StartCoroutine(InitializeClientWrapper(client, this.server.CharacterPort, DI.Instance.Resolve<ILanguage>().ConnectToCharacter, () =>
			{
				pubsub.Publish<ControlCharacterScreen>(new ControlCharacterScreen());
			}));
		}

		public void InitLoginClient()
		{
			var client = DI.Instance.Resolve<ILoginClientWrapper>();
			this.StartCoroutine(InitializeClientWrapper(client, this.server.LoginPort, DI.Instance.Resolve<ILanguage>().ConnectToLogin, () =>
			{
				DI.Instance.Resolve<IPubSub>().Publish<ControlLoginScreen>(new ControlLoginScreen());
			}));
		}

		public IEnumerator InitializeClientWrapper(IClientWrapper client, int port, string statusMessage, Action doneAction, float startProgress = 0, float maxProgress = 1)
		{
			UpdateProgress(startProgress, DI.Instance.Resolve<ILanguage>().Starting);

			yield return new WaitForEndOfFrame();

			UpdateProgress(startProgress + ((maxProgress - startProgress) / 2), statusMessage);

			Task<bool> connectTask = client.ConnectAsync(this.server.Host, port);

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
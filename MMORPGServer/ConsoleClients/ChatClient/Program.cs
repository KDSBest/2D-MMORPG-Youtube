using Common.Client;
using Common.Client.Interfaces;
using Common.IoC;
using Common.Protocol.Character;
using Common.Protocol.Chat;
using Common.Protocol.Login;
using Common.PublishSubscribe;
using System;
using System.Threading.Tasks;

namespace ChatClient
{
	public class Program
	{
		private static bool isRunning = true;
		private static string email;
		private static string password;
		private static LoginClient loginClient;

		public static async Task Main(string[] args)
		{
			var pubsub = new PubSub();
			var tokenProv = new TokenProvider();
			DI.Instance.Register<IPubSub>(() => pubsub, RegistrationType.Singleton);
			DI.Instance.Register<ITokenProvider>(() => tokenProv, RegistrationType.Singleton);

			Console.WriteLine("Enter Email:");
			email = Console.ReadLine();
			Console.WriteLine("Enter Password:");
			password = Console.ReadLine();
			loginClient = new LoginClient();
			bool connected = await loginClient.ConnectAsync("localhost", 30000);
			if (connected)
			{
				Console.WriteLine("Connected to Login Server.");
			}
			else
			{
				Console.WriteLine("Connection to Login Server failed.");
				return;
			}

			Console.WriteLine("Process Handshake.");
			while (!loginClient.IsConnected)
			{
				await Task.Delay(50);
			}

			pubsub.Subscribe<LoginRegisterResponseMessage>(OnRegisteredLogin, "Main");

			await loginClient.Workflow.RegisterAsync(email, password);
			while (isRunning)
			{
				await Task.Delay(1000);
			}
		}

		private static async Task OnRegisteredLogin(LoginRegisterResponseMessage result)
		{
			if (result.Response == LoginRegisterResponse.AlreadyRegistered)
			{
				Console.WriteLine($"Register failed with {result.Response}.");
				Console.WriteLine($"Try Login...");
				await loginClient.Workflow.LoginAsync(email, password);
				return;
			}

			if (result.Response != LoginRegisterResponse.Successful)
			{
				Console.WriteLine($"Login failed with {result.Response}.");
				return;
			}

			Console.WriteLine($"Register/Login successful.");
			Console.WriteLine($"Token: {result.Token}");
			await loginClient.DisconnectAsync();

			string charToken = string.Empty;
			DI.Instance.Resolve<IPubSub>().Subscribe<CharacterMessage>(async (msg) =>
			{
				Console.WriteLine($"Character: {msg.Character}");

				if (!string.IsNullOrEmpty(msg.Token))
				{
					charToken = msg.Token;
				}
			}, "Main");

			var charClient = new Common.Client.CharacterClient();
			DI.Instance.Resolve<ITokenProvider>().Token = result.Token;
			bool connected = await charClient.ConnectAsync("localhost", 30001);
			if (connected)
			{
				Console.WriteLine("Connected to Char Server.");
			}
			else
			{
				Console.WriteLine("Connection to Char Server failed.");
				return;
			}

			while (string.IsNullOrEmpty(charToken))
			{
				Console.WriteLine("Enter char name:");
				string name = Console.ReadLine();

				charClient.Workflow.SendCharacterCreation(new Common.Protocol.Character.CharacterInformation()
				{
					Name = name
				});

				Console.WriteLine("Wait 1 sec for server to resond.");
				await Task.Delay(1000);
			}


			DI.Instance.Resolve<IPubSub>().Subscribe<ChatMessage>(async (msg) =>
			{
				Console.WriteLine($"{msg.Sender}: {msg.Message}");
			}, "Main");
			var chatClient = new Common.Client.ChatClient();
			DI.Instance.Resolve<ITokenProvider>().Token = charToken;
			connected = await chatClient.ConnectAsync("localhost", 30002);
			if (connected)
			{
				Console.WriteLine("Connected to Chat Server.");
			}
			else
			{
				Console.WriteLine("Connection to Chat Server failed.");
				return;
			}

			while (chatClient.IsConnected)
			{
				string msg = Console.ReadLine();
				if (string.IsNullOrEmpty(msg))
				{
					await chatClient.DisconnectAsync();
					continue;
				}

				chatClient.Workflow.SendChatMessage(msg);
			}

			isRunning = false;
			Console.WriteLine("Disconnected.");
		}
	}
}

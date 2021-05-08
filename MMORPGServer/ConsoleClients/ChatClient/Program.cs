using Common.Client;
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
			Console.WriteLine("Enter Email:");
			email = Console.ReadLine();
			Console.WriteLine("Enter Password:");
			password = Console.ReadLine();
			loginClient = new LoginClient(pubsub);
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
			while (!loginClient.IsConnectedAndLoginWorkflow)
			{
				await Task.Delay(50);
			}

			pubsub.Subscribe<LoginRegisterResponseMessage>(OnRegisteredLogin, "Main");

			await loginClient.RegisterAsync(email, password);
			while(isRunning)
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
				await loginClient.LoginAsync(email, password);
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
			var charClient = new Common.Client.CharacterClient
			{
				OnNewCharacterMessage = (msg) =>
				{
					Console.WriteLine($"Character: {msg.Character}");

					if (!string.IsNullOrEmpty(msg.Token))
					{
						charToken = msg.Token;
					}
				}
			};

			bool connected = await charClient.ConnectAsync("localhost", 30001, result.Token);
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

				charClient.SendCharacterCreation(new Common.Protocol.Character.CharacterInformation()
				{
					Name = name
				});

				Console.WriteLine("Wait 1 sec for server to resond.");
				await Task.Delay(1000);
			}

			var chatClient = new Common.Client.ChatClient
			{
				OnNewChatMessage = (msg) =>
				{
					Console.WriteLine($"{msg.Sender}: {msg.Message}");
				}
			};

			Console.WriteLine("Connect to chat 1? Enter y for yes or anything else for chat 2.");
			int chatPort = 30003;
			if (Console.ReadLine() == "y")
			{
				chatPort = 30002;
			}

			Console.WriteLine($"Connect to chat on port: {chatPort}");
			connected = await chatClient.ConnectAsync("localhost", chatPort, charToken);
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

				chatClient.SendChatMessage(msg);
			}

			isRunning = false;
			Console.WriteLine("Disconnected.");
		}
	}
}

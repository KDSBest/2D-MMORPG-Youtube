using Common.Client;
using Common.Protocol.Login;
using System;
using System.Threading.Tasks;

namespace ChatClient
{
	public class Program
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("Enter Email:");
            string email = Console.ReadLine();
            Console.WriteLine("Enter Password:");
            string password = Console.ReadLine();

            var loginClient = new LoginClient();
            bool connected = await loginClient.ConnectAsync("localhost", 3334);
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
            while(!loginClient.IsConnectedAndLoginWorkflow)
			{
                await Task.Delay(50);
			}

            var result = await loginClient.RegisterAsync(email, password);
            if(result.Response == LoginRegisterResponse.AlreadyRegistered)
			{
                Console.WriteLine($"Register failed with {result.Response}.");
                Console.WriteLine($"Try Login...");
                result = await loginClient.LoginAsync(email, password);
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

                    if(!string.IsNullOrEmpty(msg.Token))
					{
                        charToken = msg.Token;
					}
				}
            };

            connected = await charClient.ConnectAsync("localhost", 3335, result.Token);
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

			connected = await chatClient.ConnectAsync("localhost", 3333, charToken);
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

			Console.WriteLine("Disconnected.");
		}
    }
}

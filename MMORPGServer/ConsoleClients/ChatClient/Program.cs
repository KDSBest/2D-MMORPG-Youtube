﻿using Common.Client;
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

            var result = await loginClient.LoginAsync(email, password);
            if(result.Response != LoginRegisterResponse.Successful)
			{
                Console.WriteLine($"Login failed with {result.Response}.");
                return;
			}

            Console.WriteLine($"Token: {result.Token}");
            await loginClient.DisconnectAsync();

            var chatClient = new Common.Client.ChatClient();
            connected = await chatClient.ConnectAsync("localhost", 3333, result.Token);
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

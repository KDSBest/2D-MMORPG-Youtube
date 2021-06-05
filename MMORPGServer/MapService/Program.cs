using Common.Protocol;
using CommonServer.Udp;
using System;
using System.Threading.Tasks;
using CommonServer.Configuration;
using MapService.WorldManagement;
using Common.IoC;
using Common.PublishSubscribe;

namespace MapService
{
    public class Program
    {
		public static async Task Main(string[] args)
        {
            try
            {
                DI.Instance.Register<IPubSub>(() => new PubSub(), RegistrationType.Singleton);
                DI.Instance.Register<IPlayerWorldManagement>(() => new PlayerWorldManagement(), RegistrationType.Singleton);
                DI.Instance.Resolve<IPlayerWorldManagement>().Initialize();
                UdpManagerListener udpManagerListener = new UdpManagerListener(ProtocolConstants.ConnectionKey, new MapUdpListener());

                Console.WriteLine("Open Map Service");
                await udpManagerListener.StartAsync(PortConfiguration.MapPort);

                await udpManagerListener.UpdateAsync();
                Console.WriteLine("Map Service is Running...");

                while (udpManagerListener.IsRunning)
                {
                    await udpManagerListener.UpdateAsync();
                    await Task.Delay(50);
                }

                Console.WriteLine("Udp Manager stopped.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: {ex}");
                Console.ReadLine();
            }
        }
    }

}

using Assets.Scripts.Character;
using Assets.Scripts.ClientWrappers;
using Assets.Scripts.Language;
using Common.Client;
using Common.Client.Interfaces;
using Common.IoC;
using Common.PublishSubscribe;

namespace Assets.Scripts
{
	public static class DILoader
	{
		private static bool isInitialized = false;

		public static void Initialize()
		{
			if (isInitialized)
				return;

			DI.Instance.Register<ILanguage>(() => new LanguageEn(), RegistrationType.Singleton);
			DI.Instance.Register<ICurrentContext>(() => new CurrentContext(), RegistrationType.Singleton);
			DI.Instance.Register<ITokenProvider>(() => DI.Instance.Resolve<ICurrentContext>(), RegistrationType.Singleton);
			DI.Instance.Register<IPubSub>(() => new UnityPubSub(), RegistrationType.Singleton);

			DI.Instance.Register<ILoginClient>(() => new LoginClient(), RegistrationType.Singleton);
			DI.Instance.Register<ILoginClientWrapper>(() => new LoginClientWrapper(), RegistrationType.Singleton);

			DI.Instance.Register<ICharacterClient>(() => new CharacterClient(), RegistrationType.Singleton);
			DI.Instance.Register<ICharacterClientWrapper>(() => new CharacterClientWrapper(), RegistrationType.Singleton);

			DI.Instance.Register<IChatClient>(() => new ChatClient(), RegistrationType.Singleton);
			DI.Instance.Register<IChatClientWrapper>(() => new ChatClientWrapper(), RegistrationType.Singleton);

			DI.Instance.Register<IMapClient>(() => new MapClient(), RegistrationType.Singleton);
			DI.Instance.Register<IMapClientWrapper>(() => new MapClientWrapper(), RegistrationType.Singleton);

			DI.Instance.Register<IInventoryClient>(() => new InventoryClient(), RegistrationType.Singleton);
			DI.Instance.Register<IInventoryClientWrapper>(() => new InventoryClientWrapper(), RegistrationType.Singleton);

			DI.Instance.Register<IPlayerEventClient>(() => new PlayerEventClient(), RegistrationType.Singleton);
			DI.Instance.Register<IPlayerEventClientWrapper>(() => new PlayerEventClientWrapper(), RegistrationType.Singleton);

			isInitialized = true;
		}
	}
}
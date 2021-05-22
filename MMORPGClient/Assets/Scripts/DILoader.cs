using Assets.Scripts.ClientWrappers;
using Assets.Scripts.Language;
using Common.Client;
using Common.Client.Interfaces;
using Common.IoC;
using Common.PublishSubscribe;
using UnityEngine;

namespace Assets.Scripts
{
	public static class DILoader
	{
		private static bool isInitialized = false;

		public static void Initialize()
		{
			if (isInitialized)
				return;

			Debug.Log("Init DI");
			DI.Instance.Register<ILanguage>(() => new LanguageEn(), RegistrationType.Singleton);
			DI.Instance.Register<IPubSub>(() => new UnityPubSub(), RegistrationType.Singleton);
			DI.Instance.Register<ILoginClient>(() => new LoginClient(), RegistrationType.Singleton);
			DI.Instance.Register<ICharacterClient>(() => new CharacterClient(), RegistrationType.Singleton);
			DI.Instance.Register<ILoginClientWrapper>(() => new LoginClientWrapper(), RegistrationType.Singleton);
			DI.Instance.Register<ICharacterClientWrapper>(() => new CharacterClientWrapper(), RegistrationType.Singleton);

			isInitialized = true;
		}
	}
}
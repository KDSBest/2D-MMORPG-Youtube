using Assets.Scripts.Language;
using Common.IoC;
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
			DI.Instance.Register<IPubSub>(() => new PubSub(), RegistrationType.Singleton);

			isInitialized = true;
		}
	}
}
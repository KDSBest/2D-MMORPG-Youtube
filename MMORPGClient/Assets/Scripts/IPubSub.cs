using System;

namespace Assets.Scripts
{
	public interface IPubSub
	{
		void Publish<T>(T data);
		void Subscribe<T>(Action<T> subscriptionTyped, string name);
		void Unsubscribe<T>(string name);
	}
}
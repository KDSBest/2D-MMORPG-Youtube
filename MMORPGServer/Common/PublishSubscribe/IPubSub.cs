using System;
using System.Threading.Tasks;

namespace Common.PublishSubscribe
{
	public interface IPubSub
	{
		void Publish<T>(T data);
		void Subscribe<T>(Action<T> subscriptionTyped, string name);
		void Subscribe<T>(Func<T, Task> subscriptionTyped, string name);
		void Unsubscribe<T>(string name);
	}

}

using System;
using System.Collections.Generic;
using System.Text;

namespace Common.PublishSubscribe
{
	public interface IPubSub
	{
		void Publish<T>(T data);
		void Subscribe<T>(Action<T> subscriptionTyped, string name);
		void Unsubscribe<T>(string name);
	}

}

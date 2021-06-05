using Common.PublishSubscribe;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Assets.Scripts
{
	public class UnityPubSub : IPubSub
	{
		public const int MAX_WAIT = 1000;
		private ConcurrentDictionary<Type, ConcurrentDictionary<string, object>> registrations = new ConcurrentDictionary<Type, ConcurrentDictionary<string, object>>();

		public void Subscribe<T>(Func<T, Task> subscriptionTyped, string name)
		{
			if (subscriptionTyped == null)
				throw new ArgumentNullException($"{nameof(subscriptionTyped)} can't be null.");

			object subscription = subscriptionTyped;
			if (subscription == null)
				throw new ArgumentNullException($"{nameof(subscriptionTyped)} must be assignable to Action<object>.");

			var registration = registrations.GetOrAdd(typeof(T), (t) => new ConcurrentDictionary<string, object>());
			registration.AddOrUpdate(name, subscription, (n, a) => subscription);
		}

		public void Subscribe<T>(Action<T> subscriptionTyped, string name)
		{
			if (subscriptionTyped == null)
				throw new ArgumentNullException($"{nameof(subscriptionTyped)} can't be null.");

			object subscription = subscriptionTyped;
			if (subscription == null)
				throw new ArgumentNullException($"{nameof(subscriptionTyped)} must be assignable to Action<object>.");

			var registration = registrations.GetOrAdd(typeof(T), (t) => new ConcurrentDictionary<string, object>());
			registration.AddOrUpdate(name, subscription, (n, a) => subscription);
		}

		public void Publish<T>(T data)
		{
			UnityDispatcher.RunOnMainThread(() =>
			{
				var registration = registrations.GetOrAdd(typeof(T), (t) => new ConcurrentDictionary<string, object>());

				var regs = registration.ToArray();
				foreach (var reg in regs)
				{
					if (reg.Value is Action<T>)
					{
						((Action<T>)reg.Value)(data);
					}
					else
					{
						Task t = ((Func<T, Task>)reg.Value)(data);
						t.Wait(MAX_WAIT);
					} 
				}
			});
		}

		public void Unsubscribe<T>(string name)
		{
			var registration = registrations.GetOrAdd(typeof(T), (t) => new ConcurrentDictionary<string, object>());
			registration.TryRemove(name, out object removed);
		}
	}
}
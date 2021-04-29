using Common.PublishSubscribe;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
	public class UnityPubSub : IPubSub
	{
		private ConcurrentDictionary<Type, ConcurrentDictionary<string, object>> registrations = new ConcurrentDictionary<Type, ConcurrentDictionary<string, object>>();

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
					((Action<T>)reg.Value)(data);
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
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Common.IoC
{
	public class DI
	{
		private static readonly Lazy<DI> instance = new Lazy<DI>(() => new DI());

		public static DI Instance {  get { return instance.Value; } }

		private DI()
		{

		}

		private ConcurrentDictionary<Type, object> instances = new ConcurrentDictionary<Type, object>();
		private ConcurrentDictionary<Type, Registration> registrations = new ConcurrentDictionary<Type, Registration>();

		public void Register<T>(Func<T> creationDelegateTyped, RegistrationType type)
		{
			if (creationDelegateTyped == null)
				throw new ArgumentNullException($"{nameof(creationDelegateTyped)} can't be null.");

			Func<object> creationDelegate = creationDelegateTyped as Func<object>;

			if (creationDelegate == null)
				throw new ArgumentNullException($"{nameof(creationDelegateTyped)} must be assignable to Func<object>.");

			var registration = new Registration(creationDelegate, type);
			registrations.AddOrUpdate(typeof(T), registration, (t, reg) => registration);
		}

		public T Resolve<T>()
		{
			if(registrations.TryGetValue(typeof(T), out Registration registration))
			{
				switch (registration.Type)
				{
					case RegistrationType.Singleton:
						var singletonInstance = (T)instances.GetOrAdd(typeof(T), (t) => registration.CreationDelegate());
						return singletonInstance;
					case RegistrationType.New:
						return (T)registration.CreationDelegate();
				}
			}

			throw new InvalidOperationException($"Couldn't find registration for {typeof(T).FullName}");
		}
	}
}

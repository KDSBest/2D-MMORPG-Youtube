using System;

namespace Common.IoC
{
	public class Registration
	{
		public Func<object> CreationDelegate { get; set; }
		public RegistrationType Type { get; set; }

		public Registration(Func<object> creationDelegate, RegistrationType type)
		{
			Type = type;
			CreationDelegate = creationDelegate;
		}
	}
}

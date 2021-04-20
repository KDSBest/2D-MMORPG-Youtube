using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts
{
	public static class UnityDispatcher
	{
		private static ConcurrentQueue<Action> actions = new ConcurrentQueue<Action>();

		public static void RunOnMainThread(Action action)
		{
			actions.Enqueue(action);
		}

		public static void ExecuteQueue()
		{
			while(actions.TryDequeue(out Action action))
			{
				action();
			}
		}
	}
}

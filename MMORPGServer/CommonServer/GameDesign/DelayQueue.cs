using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CommonServer.GameDesign
{

	public class DelayQueue<T>
	{
		private object lockObject = new object();
		private List<DelayQueueEntry<T>> futureEntries = new List<DelayQueueEntry<T>>();

		public Func<DelayQueueEntry<T>, Task> OnExecute;

		public void Enqueue(DelayQueueEntry<T> entry)
		{
			lock (lockObject)
			{
				futureEntries.Add(entry);
			}
		}

		public async Task Update(int timeInMs)
		{
			List<DelayQueueEntry<T>> entries;

			lock (lockObject)
			{
				futureEntries.ForEach(x => x.WaitDuration -= timeInMs);

				entries = futureEntries.Where(x => x.WaitDuration <= 0).ToList();
			}

			for (int i = 0; i < entries.Count; i++)
			{
				await OnExecute(entries[i]);

				lock (lockObject)
				{
					futureEntries.Remove(entries[i]);
				}
			}
		}
	}
}

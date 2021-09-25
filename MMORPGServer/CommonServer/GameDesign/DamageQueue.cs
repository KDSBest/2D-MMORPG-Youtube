using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CommonServer.GameDesign
{

	public class DamageQueue
	{
		private object lockObject = new object();
		private List<DamageInFuture> damageInFutures = new List<DamageInFuture>();

		public Func<DamageInFuture, Task> OnDamage;

		public void Enqueue(DamageInFuture damage)
		{
			lock (lockObject)
			{
				damageInFutures.Add(damage);
			}
		}

		public async Task Update(int timeInMs)
		{
			List<DamageInFuture> damages;

			lock (lockObject)
			{
				damageInFutures.ForEach(x => x.WaitDuration -= timeInMs);

				damages = damageInFutures.Where(x => x.WaitDuration <= 0).ToList();
			}

			for (int i = 0; i < damages.Count; i++)
			{
				await OnDamage(damages[i]);

				lock (lockObject)
				{
					damageInFutures.Remove(damages[i]);
				}
			}
		}
	}
}

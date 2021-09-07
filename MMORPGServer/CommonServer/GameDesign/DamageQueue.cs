using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonServer.GameDesign
{

	public class DamageQueue
	{
		private List<DamageInFuture> damageInFutures = new List<DamageInFuture>();

		public Action<DamageInFuture> OnDamage;

		public void Enqueue(DamageInFuture damage)
		{
			lock (damageInFutures)
			{
				damageInFutures.Add(damage);
			}
		}

		public void Update(int timeInMs)
		{
			lock (damageInFutures)
			{
				damageInFutures.ForEach(x => x.WaitDuration -= timeInMs);

				var damages = damageInFutures.Where(x => x.WaitDuration <= 0).ToList();
				for (int i = 0; i < damages.Count; i++)
				{
					OnDamage(damages[i]);
					damageInFutures.Remove(damages[i]);
				}
			}
		}
	}
}

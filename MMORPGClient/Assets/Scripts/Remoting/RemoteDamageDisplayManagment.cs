using Common.IoC;
using Common.Protocol.Combat;
using Common.PublishSubscribe;
using UnityEngine;

namespace Assets.Scripts.Remoting
{
	public class RemoteDamageDisplayManagment : MonoBehaviour
	{
		public GameObject DamageNumberPrefab;

		private IPubSub pubsub;
		private RemoteTargeting remoteTargeting;


		public void OnEnable()
		{
			DILoader.Initialize();

			remoteTargeting = new RemoteTargeting(this.GetType().Name);

			pubsub = DI.Instance.Resolve<IPubSub>();
			pubsub.Subscribe<DamageMessage>(OnDamage, this.GetType().Name);
		}

		private void OnDamage(DamageMessage msg)
		{
			if (!remoteTargeting.CheckValidTargeting(msg.Target))
				return;

			var go = GameObject.Instantiate(DamageNumberPrefab);
			go.transform.position = remoteTargeting.GetTargetPosition(msg.Target);
			go.transform.SetParent(this.transform, true);
			var dmgDisplay = go.GetComponent<DamageDisplay>();
			dmgDisplay.SetDamage(msg.DamageInfo);
		}
	}
}

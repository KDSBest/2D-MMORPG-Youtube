using Assets.Scripts.Skills;
using Common.GameDesign;
using Common.IoC;
using Common.Protocol.Map;
using Common.PublishSubscribe;
using UnityEngine;

namespace Assets.Scripts.Remoting
{
	public class RemoteSkillCastManagement : MonoBehaviour
	{
		public GameObject RemoteFireballPrefab;
		public GameObject RemoteLightningBoltPrefab;
		private RemoteTargeting remoteTargeting;
		private IPubSub pubsub;

		public void OnEnable()
		{
			DILoader.Initialize();

			remoteTargeting = new RemoteTargeting(this.GetType().Name);

			pubsub = DI.Instance.Resolve<IPubSub>();
			pubsub.Subscribe<SkillCastMessage>(OnCastSkill, this.GetType().Name);
		}

		private void OnCastSkill(SkillCastMessage msg)
		{
			if (!remoteTargeting.CheckValidTargeting(msg.Target))
				return;

			GameObject go;

			switch(msg.Type)
			{
				case SkillCastType.Fireball:
					go = GameObject.Instantiate(RemoteFireballPrefab);

					var fireball = go.GetComponent<Fireball>();
					fireball.Caster = new Vector3(msg.Position.X, msg.Position.Y, 0);
					fireball.Target = remoteTargeting.GetTargetPosition(msg.Target);

					go.transform.position = fireball.Caster;
					break;
				case SkillCastType.LightningBolt:
					go = GameObject.Instantiate(RemoteLightningBoltPrefab);

					go.transform.position = remoteTargeting.GetTargetPosition(msg.Target);
					break;
				default:
					return;
			}

			go.transform.SetParent(this.transform, true);
		}
	}
}
